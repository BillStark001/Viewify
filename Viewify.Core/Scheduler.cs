using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Viewify.Base;
using Viewify.Core.Model;
using Viewify.Core.Tree;
using Viewify.Core.Utils;

namespace Viewify.Core;





public class Scheduler
{

    private readonly ConcurrentQueue<(Fiber<View?>, Func<View?>)> _dispatchQueue = new();
    private readonly Dictionary<object, Fiber<View?>> _dict = new();

    private Fiber<View?> _root;
    private Fiber<View?>? _renderRoot;
    private Fiber<View?>? _wip;
    private Fiber<View?>? _current;


    public Scheduler(View rootNode)
    {
        _root = new(rootNode);
    }


    public void Tick()
    {
        // if a dispatch is scheduled, prioritize it
        while (_dispatchQueue.TryDequeue(out var d)) {
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


    public void HandleDispatch((Fiber<View?>, Func<View?>) t)
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
            _wip = new(newViewRoot)
            {
                Key = newViewRoot?.Key,
                Alternate = f,
                Tag = FiberTag.Update,
            };
            _current = _wip;
        }
    }

    public static IEnumerable<View> GetChildren(View? v)
    {
        if (v == null)
        { 
            return Enumerable.Empty<View>();
        }
        if (v is NativeView || v is Fragment)
        {
            return v.Children;
        }
        var r = v.Render();
        return r != null ? new View[] { r } : Enumerable.Empty<View>();
    }

    public Fiber<View?>? PerformDiffWorkAndGetNext(Fiber<View?> current)
    {

        // TODO add necessary hooks

        var children = GetChildren(current.Content);
        ReconcileChildren(current, children, current.Content is Fragment f && f.UseKey);

        return current.Next();

    }


    public void ReconcileChildren(Fiber<View?> v, IEnumerable<View> children, bool useKey = false)
    {
        if (useKey)
        {
            // create key records
            _dict.Clear();
            var c = v.Alternate?.Child;
            while (c != null)
            {
                if (c.Key != null)
                {
                    _dict[c.Key] = c;
                }
            }
        }
        var _children = children.GetEnumerator();
        var _hasNext = _children.MoveNext();
        var oldFiber = v.Alternate?.Child;
        var newView = _hasNext ? _children.Current : null;
        var newFiber = v;
        var prevNewFiber = v;
        var initial = true;


        do
        {
            var sameType = oldFiber != null && newView != null
                && oldFiber.Content?.GetType() == newView?.GetType();
            var sameKey = useKey && newView?.Key != null && oldFiber?.Key == newView?.Key;
            var keyExists = useKey && newView?.Key != null &&  _dict.ContainsKey(newView.Key);

            if (sameType)
            {
                if (!sameKey && keyExists)
                {
                    oldFiber!.Tag = FiberTag.Insert;
                    // TODO record oldFiber to insert
                }
                else if (!sameKey && !keyExists)
                {
                    // TODO warn the user the requirement of unique key props
                }
                newFiber = new Fiber<View?>(newView)
                {
                    Return = v,
                    Alternate = oldFiber,
                    Tag = FiberTag.Update,
                };
            }
            else
            {
                // 1. remove
                if (oldFiber != null)
                {
                    oldFiber.Tag = FiberTag.Remove;
                    // TODO record oldFiber to delete
                }
                // 2.add
                if (newView != null)
                {
                    newFiber = new Fiber<View?>(newView)
                    {
                        Return = v,
                        Tag = FiberTag.Create,
                    };
                }
            }


            // commit change

            if (initial)
            {
                v.Child = newFiber;
            }
            else
            {
                prevNewFiber.Sibling = newFiber;
            }

            // move the cursor
            initial = false;
            prevNewFiber = newFiber;
            oldFiber = oldFiber?.Sibling;
            _hasNext = _children.MoveNext();
            newView = _hasNext ? _children.Current : null;
        } while (oldFiber != null || _hasNext);

    }

}
