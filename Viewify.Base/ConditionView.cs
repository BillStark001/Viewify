using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public class ConditionView : View
{

    [Prop] public bool Condition { get; private set; }

    [Prop] public View? TrueCase { get; private set; }

    [Prop] public View? FalseCase { get; private set; }

    public ConditionView(bool condition, View? trueCase = null, View? falseCase = null)
    {
        Condition = condition;
        TrueCase = trueCase;
        FalseCase = falseCase;
    }

    public override View? Render()
    {
        return Condition ? TrueCase : FalseCase;
    }
}
