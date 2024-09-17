using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Viewify.Base;
using Viewify.Core.Fiber;
using Viewify.Core.Model;
using Viewify.Core.Utils;

namespace Viewify.Core;





public sealed class Scheduler
{

    private readonly ConcurrentQueue<(Fiber<ViewNode>, Func<View?>)> _dispatchQueue = new();
    private readonly Dictionary<object, Fiber<ViewNode>> _dict = [];
    private readonly ViewRecordCache _recordCache = new();

    private Fiber<ViewNode> _root;
    private Fiber<ViewNode>? _renderRoot;
    private Fiber<ViewNode>? _wip;
    private Fiber<ViewNode>? _current;


    public Scheduler(View rootNode)
    {
        _root = CreateFiber(rootNode);
    }

    private Fiber<ViewNode> CreateFiber(View? view)
    {
        return new(new(view, this, _recordCache));
    }


    public void Tick()
    {
        // if a dispatch is scheduled, prioritize it
        while (_dispatchQueue.TryDequeue(out var d))
        {
            HandleDispatch(d);
        }

        // if the diff task is complete, commit it
        if (_wip != null && _current == null)
        {
            // TODO commit
        }
        // if it is not complete, perform once
        if (_wip != null && _current != null)
        {
            _current = PerformDiffWorkAndGetNext(_current);
        }

        // else, do nothing
    }


    public void HandleDispatch((Fiber<ViewNode>, Func<View?>) t)
    {
        var (f, a) = t;
        var newViewRoot = a();
        _renderRoot = f;
        _wip = null;
        _current = null;
        // TODO judge the precedence if a render root exists
        if (f != null)
        {
            // discard the old wip fiber and create a new one
            _wip = CreateFiber(newViewRoot);
            _wip.Key = newViewRoot?.Key;
            _wip.Alternate = f;
            _wip.Tag = FiberTag.Update;

            _current = _wip;
        }
    }

    public static IEnumerable<View> GetChildren(View? v)
    {
        if (v == null)
        {
            return [];
        }
        if (v is NativeView || v is Fragment)
        {
            return v.Children ?? [];
        }
        var r = v.Render();
        return r != null ? [r] : [];
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

}
