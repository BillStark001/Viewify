using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Render;

public abstract class StateWithDispatch : IState
{
    public Action<Action> Dispatch { get; set; } = null!;

    public abstract object? GetValue();

    public static StateWithDispatch Create(Type type, Action<Action> dispatch, object initialValue)
    {
        var genericType = typeof(StateWithDispatch<>).MakeGenericType(type);
        return (StateWithDispatch)Activator.CreateInstance(genericType, dispatch, initialValue)!;
    }
}

public class StateWithDispatch<T> : StateWithDispatch, IState<T>
{

    private T _value;
    public T Get() => _value;

    public override object? GetValue() => _value;

    public void Set(T value)
    {
        Dispatch(() => _value = value);
    }

    public StateWithDispatch(Action<Action> dispatch, T initialValue)
    {
        Dispatch = dispatch;
        _value = initialValue;
    }
}
