using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Viewify.Base;

namespace Viewify.Core.Tree;

public static class TreeMethods
{
    public static void ConnectChildren(Fiber<ViewNode> current, IEnumerable<ViewRecord> children)
    {
        var first = true;
        foreach (var childNode in children)
        {
            Fiber<ViewNode> newFiber = new(new(childNode), childNode.Key);
            if (first)
            {
                first = false;
                current.Child = newFiber;
                newFiber.Return = current;
            }
            else
            {
                current.Sibling = newFiber;
                newFiber.Return = current.Return;
            }
            current = newFiber;
        }
    }

    /// <summary>
    /// if view is native or fragment: <br/>
    ///   use its children as fiber children <br/>
    /// if view is non-native: <br/>
    ///   render it, then use the render result as fiber children <br/>
    /// </summary>
    /// <param name="v"></param>
    /// <param name="current"></param>
    public static void MakeChildrenNodes(this IViewifyInstance v, Fiber<ViewNode> current)
    {
        var node = ~current;
        if (node.Record == null)
        {
            // do nothing
        }
        else if (node.Record.IsNative && node.Record.Children != null)
        {
            ConnectChildren(current, node.Record.Children);
        }
        else if (node.Record is ClassViewRecord recClass)
        {
            var children = recClass.View.Render(recClass.Children ?? Enumerable.Empty<ViewRecord>());
            ConnectChildren(current, children);
        }
        else if (node.Record is FuncViewRecord recFunc)
        {
            var children = recFunc.View(v, recFunc.Children ?? Enumerable.Empty<ViewRecord>());
            ConnectChildren(current, children);
        }
    }




    public static Fiber<ViewNode>? ReconcileOnUpdate(this IViewifyInstance v, Fiber<ViewNode> current)
    {
        var node = ~current;
        if (node.State == ViewNode.NodeState.Idle || node.State == ViewNode.NodeState.Visited)
            return current.Next();

        // else it must reconcile

        if (node.State == ViewNode.NodeState.NeedVisit)
        {
            // TODO check hooks
        }
        MakeChildrenNodes(v, current);
        if (node.State == ViewNode.NodeState.NeedVisit)
        {
            // TODO check hooks
        }
        node.State = ViewNode.NodeState.Visited;

        // TODO post hook calls (e.g. use effect)

        return current.Next();
    }
}
