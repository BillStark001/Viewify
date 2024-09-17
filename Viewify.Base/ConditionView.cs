using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public class ConditionView(bool condition, View? trueCase = null, View? falseCase = null) : View
{

    [Prop] public bool Condition { get; private set; } = condition;

    [Prop] public View? TrueCase { get; private set; } = trueCase;

    [Prop] public View? FalseCase { get; private set; } = falseCase;

    public override View? Render()
    {
        return Condition ? TrueCase : FalseCase;
    }
}
