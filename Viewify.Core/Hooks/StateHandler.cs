using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Hooks;
public class StateRecord
{
    public int Index { get; set; }
}

internal class StateHandler : HookHandler<StateRecord, IState>
{
    public override StateRecord Make(ViewNode node, int index)
    {
        return new()
        {
            Index = index
        };
    }

    public override IState Use(ViewNode node, StateRecord hook, Action markUpdate)
    {
        throw new NotImplementedException();
    }
}
