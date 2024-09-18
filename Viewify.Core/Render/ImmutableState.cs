using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Render;

public class ImmutableState<T>(T value = default!) : IState<T>
{
    public T Value { get; set; } = value;

    public T Get() => Value;

    public void Set(T value)
    {
        throw new InvalidOperationException();
    }

    public static IState Create(Type type, object initialValue)
    {
        var genericType = typeof(ImmutableState<>).MakeGenericType(type);
        return (IState)Activator.CreateInstance(genericType, initialValue)!;
    }
}
