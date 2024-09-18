using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Render;

public abstract class StateWithDispatch : IState
{
    public Scheduler Scheduler { get; set; } = null!;
    public Fiber<ViewNode> Fiber { get; set; } = null!;

    public void Dispatch(Action a)
    {
        Scheduler.Dispatch(Fiber, a);
    }

    public abstract object? GetValue();
    public abstract void SetValue(object? value);

    public static StateWithDispatch Create(Type type, Scheduler scheduler, Fiber<ViewNode> fiber, object initialValue)
    {
        var genericType = typeof(StateWithDispatch<>).MakeGenericType(type);
        return (StateWithDispatch)Activator.CreateInstance(genericType, scheduler, fiber, initialValue)!;
    }
}

public class StateWithDispatch<T> : StateWithDispatch, IState<T>
{

    private T _value;
    public T Get() => _value;

    public override object? GetValue() => _value;
    public override void SetValue(object? value)
    {
        _value = (T)value!;
    }

    public void Set(T value)
    {
        Dispatch(() => _value = value);
    }

    public StateWithDispatch(Scheduler scheduler, Fiber<ViewNode> fiber, T initialValue)
    {
        Scheduler = scheduler;
        Fiber = fiber;
        _value = initialValue;
    }
}
