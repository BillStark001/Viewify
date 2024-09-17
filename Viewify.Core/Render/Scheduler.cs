using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Viewify.Base;
using Viewify.Core.Utils;

namespace Viewify.Core.Render;


public sealed class Scheduler
{

    private readonly ConcurrentQueue<(Fiber<ViewNode>, Action)> _dispatchQueue = new();
    private readonly Dictionary<object, Fiber<ViewNode>> _dict = [];
    private readonly ViewRecordCache _recordCache = new();

    private readonly Fiber<ViewNode> _root;
    private Fiber<ViewNode>? _renderRoot;
    private Fiber<ViewNode>? _wipRoot;
    private Fiber<ViewNode>? _current;

    public INativeHandler Handler { get; init; }


    public Scheduler(View rootNode, INativeHandler handler)
    {
        _root = CreateFiber(rootNode);
        Handler = handler;

        // render initial view
        _renderRoot = _root;
        CreateWipRoot();
    }

    private Fiber<ViewNode> CreateFiber(View? view)
    {
        return new(new(view, this, _recordCache));
    }


    public void Tick()
    {
        // precedence: 
        // - handle dispatch
        // - commit work
        // - create work
        // - diffing & reconcilation

        // if a dispatch is scheduled, prioritize it

        var dispatchHandled = false;
        while (_dispatchQueue.TryDequeue(out var d))
        {
            HandleDispatch(d);
            dispatchHandled = true;
        }
        if (dispatchHandled)
        {
            return;
        }

        if (_wipRoot == null)
        {
            CreateWipRoot();
        }
        else
        {
            if (_current == null)
            {
                // diffing is complete, commit
                CommitWipRoot();
            } 
            else
            {
                _current = PerformDiffWorkAndGetNext(_current);
            }
        }

        // else, do nothing
    }

    public void Dispatch(Fiber<ViewNode> node, Action action)
    {
        _dispatchQueue.Enqueue((node, action));
    }

    public void CreateWipRoot()
    {
        if (_renderRoot == null)
        {
            return;
        }

        var v = _renderRoot.Content.View;

        _wipRoot = CreateFiber(_renderRoot.Content.View);
        _wipRoot.Key = v?.Key;
        _wipRoot.Alternate = _renderRoot;
        _wipRoot.Parent = _renderRoot.Parent;
        _wipRoot.Tag = FiberTag.Update;

        _current = _wipRoot;

    }

    public void HandleDispatch((Fiber<ViewNode>, Action) t)
    {
        var (f, a) = t;
        a();

        // halt the current render
        _wipRoot = null;
        _current = null;

        // assign render root
        // if one already exists, check ancestry and use the higher one
        var renderRootFound = false;
        var current = f;
        while (_renderRoot != null && current != null)
        {
            if (current == _renderRoot)
            {
                renderRootFound = true;
                break;
            }
            current = current.Parent;
        }

        if (!renderRootFound)
        {
            _renderRoot = f;
        }
    }

    public static IEnumerable<View> GetChildren(View? v)
    {
        if (v == null)
        {
            yield break;
        }
        if (v is NativeView || v is Fragment)
        {
            if (v.Children != null)
            {
                foreach (var child in v.Children)
                {
                    yield return child;
                }
            }
        }
        var r = v.Render();
        if (r == null)
        {
            yield break;
        }
        yield return r;
    }

    public Fiber<ViewNode>? PerformDiffWorkAndGetNext(Fiber<ViewNode> current)
    {

        // TODO add necessary hooks
        var currentView = current.Content.View;
        var children = GetChildren(currentView);
        ReconcileChildren(current, children, currentView is Fragment f && f.UseKey);

        return current.Next();

    }

    public void ReconcileChildren(Fiber<ViewNode> v, IEnumerable<View> children, bool useKey = false)
    {
        // old fiber node

        var oldFiber = v.Alternate?.Child;
        if (useKey)
        {
            // create key records
            // oldFiber turns into the first fiber without key
            _dict.Clear();
            var currentFiber = oldFiber;
            var oldFiberSet = false;
            while (currentFiber != null)
            {
                if (currentFiber.Key != null)
                {
                    _dict[currentFiber.Key] = currentFiber;
                }
                else if (!oldFiberSet)
                {
                    oldFiber = currentFiber;
                    oldFiberSet = true;
                }
                currentFiber = currentFiber.Sibling;
            }
            if (!oldFiberSet)
            {
                oldFiber = null;
            }
        }

        // new views to create fiber nodes
        // note: null is a valid view type, 
        // so check the exhaustion of children by _hasNext
        var _children = children.GetEnumerator();
        var _hasNext = _children.MoveNext();
        var newView = _hasNext ? _children.Current : null;

        Fiber<ViewNode> prevNewFiber = v;

        var initial = true;

        while (oldFiber != null || _hasNext)
        {

            var newKey = newView?.Key;
            Fiber<ViewNode>? _f = null;
            var useKeyAndHasNewKey = useKey && newKey != null;
            var withKeyedOldView = useKeyAndHasNewKey && _dict.TryGetValue(newKey!, out _f);
            var logicalOldFiber = useKeyAndHasNewKey ? _f : oldFiber;

            // note: if the new view has a key and key is used,
            // logicalOldFiber is either the one with exact key match or null,
            // otherwise it is just the "physical" old fiber.

            var logicalOldView = logicalOldFiber?.Content.View;
            var viewHasSameType = logicalOldView?.GetType() == newView?.GetType();

            // used in arrayed fragments

            Fiber<ViewNode>? newFiber = null;

            if (viewHasSameType)
            {
                newFiber = CreateFiber(newView);
                newFiber.Parent = v;
                newFiber.Alternate = logicalOldFiber;
                newFiber.Tag = FiberTag.Update;
                if (withKeyedOldView)
                {
                    logicalOldFiber!.Tag = FiberTag.Insert;
                    newFiber.AddOperativeFiber(logicalOldFiber);
                }
                if (useKey && newKey == null)
                {
                    // TODO
                    // warn the user to add a key prop
                }
            }
            else
            {
                // remove if exists & add
                if (_hasNext)
                {
                    newFiber = CreateFiber(newView);
                    newFiber.Parent = v;
                    newFiber.Tag = FiberTag.Create;
                    if (useKey)
                    {
                        newFiber.Key = newKey;
                    }
                }
                if (logicalOldFiber != null)
                {
                    logicalOldFiber.Tag = FiberTag.Remove;
                    (newFiber ?? prevNewFiber).AddOperativeFiber(logicalOldFiber);
                }
            }


            // commit change and chain all fibers together
            if (initial)
            {
                v.Child = newFiber;
                initial = false;
            }
            else
            {
                prevNewFiber.Sibling = newFiber;
            }

            // replace prevNewFiber
            if (newFiber != null)
            {
                // prevNewFiber is still needed in null cases
                // since it is useful to record operative fibers
                prevNewFiber = newFiber;
            }

            // move the cursor
            // oldFiber
            oldFiber = oldFiber?.Sibling;
            while (useKey && oldFiber?.Key != null)
            {
                // find the next oldFiber node without a key, or null
                oldFiber = oldFiber.Sibling;
            }

            // newView
            _hasNext = _children.MoveNext();
            newView = _hasNext ? _children.Current : null;
        }

    }


    public void CommitWipRoot()
    {
        var currentFiber = _wipRoot;
        while (currentFiber != null)
        {
            currentFiber.Content.OnVisit(currentFiber);
            currentFiber.Detach();
            currentFiber = currentFiber.Next();
        }

        _wipRoot = null;
        _renderRoot = null;
    }
}
