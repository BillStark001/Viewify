using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;


public class ContextProvider(object? value, bool useKey = false) : Fragment(useKey)
{
    [Prop] public object? Value { get; protected set; } = value;
}

public class ContextProvider<T>(T value) : ContextProvider(value) where T : class
{

    public new T Value
    {
        get => (base.Value as T)!;
        set => base.Value = value;
    }
}
