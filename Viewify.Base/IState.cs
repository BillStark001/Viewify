using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public interface IState
{
}

public interface IState<V> : IState
{

    public V Get();

    public void Set(V value);


    public static IState<V> operator %(IState<V> a, V b)
    {
        a.Set(b);
        return a;
    }


    public static V operator ~(IState<V> state)
    {
        return state.Get();
    }

}

[AttributeUsage(AttributeTargets.Field)]
public class DefaultValueAttribute : Attribute
{
    public object? Value { get; init; }

    public DefaultValueAttribute(object? value)
    {
        Value = value;
    }
}

public interface IDefaultValueFactory
{
    public object? Create();
}

[AttributeUsage(AttributeTargets.Field)]
public class DefaultValueFactoryAttribute : Attribute
{
    public IDefaultValueFactory Factory { get; init; }

    public DefaultValueFactoryAttribute(Type type)
    {
        var factory = Activator.CreateInstance(type) as IDefaultValueFactory;
        if (factory == null)
            throw new InvalidOperationException("Failed to construct the factory.");
        Factory = factory;
    }

    public object? Value => Factory.Create();
}

[AttributeUsage(AttributeTargets.Field)]
public class StateIgnoreAttribute: Attribute
{

}