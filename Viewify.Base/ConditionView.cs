using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public class ConditionView(bool condition, View? trueCase = null, View? falseCase = null) : View
{

    [Prop] public bool Condition { get; protected set; } = condition;

    [Prop] public View? TrueCase { get; protected set; } = trueCase;

    [Prop] public View? FalseCase { get; protected set; } = falseCase;

    public override View? Render()
    {
        return Condition ? TrueCase : FalseCase;
    }
}
