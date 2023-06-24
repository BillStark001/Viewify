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


    public static IState<V> operator |(IState<V> a, V b)
    {
        a.Set(b);
        return a;
    }

    public static V operator ~(IState<V> state)
    {
        return state.Get();
    }

    public static IState<V> operator -(IState<V> state)
    {
        return State.Bind(state);
    }

}
