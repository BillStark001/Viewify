using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Viewify.Base;
using Viewify.Core.Tree;

namespace Viewify.Core;

public class Scheduler : IViewifyInstance
{
    private readonly ConcurrentQueue<(Fiber<ViewNode>, Action<ViewNode>?)> _dispatchQueue = new();

    private Fiber<ViewNode> _root;
    private Fiber<ViewNode>? _wip;
    private Fiber<ViewNode>? _current;


    public Scheduler(ViewRecord rootNode)
    {
        _root = new(new(rootNode));
        _root.Content.MarkNeedVisit();
    }


    public bool HandleDispatch()
    {
        var ret = false;
        while (_dispatchQueue.TryDequeue(out var res))
        {
            var (node, action) = res;
            // TODO if alternate is null, then there is an error
            if (node.Content.State == ViewNode.NodeState.Visited && node.Alternate != null)
                node = node.Alternate;
            if (action != null)
                action(node.Content);
            
            ret = true;
            node.Content.MarkNeedVisit();
        }
        return ret;
    }

    public void StartReconcile()
    {
        _wip = new(new(_root.Content.Record));
        _wip.Alternate = _root;
        _current = _wip;
    }

    public void Tick()
    {
        if (_wip == null)
        {

            var changed = HandleDispatch();
            if (changed)
                StartReconcile();
            // TODO init work
            return;
        }

        TickReconcile();
    }
    
    public void TickReconcile()
    {

        if (_current == null)
            return;

        var alter = _current.Alternate;
        if (alter == null)
        {
            // this one is newly inserted
            this.MakeChildrenNodes(_current);
        }
        else
        {
            if (alter.Content.State == ViewNode.NodeState.NeedVisit)
            {
                // TODO render children nodes from alter
            }
            else
            {
                // reuse old nodes
                // TODO copy the children from alter
            }
            alter.Content.State = ViewNode.NodeState.Visited;
            alter.Alternate = _current; // to handle dispatch
        }
        _current.Content.State = ViewNode.NodeState.Idle;
        _current = _current.Next();
    }
}
