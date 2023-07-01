using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public class ViewNode
{
    public enum NodeState
    {
        InProgress = 0,
        NeedVisit = 1,
        Visited = 2,
        Idle = 3,
    }

    public ViewRecord Record { get; set; } = null!;

    public NodeState State { get; set; } = NodeState.InProgress;

    public LinkedList<object> Hooks { get; } = new();
    public LinkedList<IState> States { get; } = new();
    public View? View { get; set; }

    public void MarkNeedVisit()
    {
        State = NodeState.NeedVisit;
    }

    public ViewNode(ViewRecord record)
    {
        Record = record;
    }

}
