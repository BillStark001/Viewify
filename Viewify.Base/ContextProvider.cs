using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public class ContextProvider<T> : View
{

    [Prop] public T Value { get; private set; }

    public ContextProvider(T value)
    {
        Value = value;
    }

    public override View? Render()
    {
        throw new InvalidOperationException();
    }
}
