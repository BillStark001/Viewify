using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base.Data;

namespace Viewify.Base;

public static class State
{
    internal class DefaultValueMarker
    {
        public readonly object? _defaultValue;
        public DefaultValueMarker(object? value)
        {
            _defaultValue = value;
        }
    }

    internal sealed class DefaultValueMarker<T> : DefaultValueMarker, IState<T>
    {
        public new readonly T _defaultValue;
        public DefaultValueMarker(T value) : base(value)
        {
            _defaultValue = value;
        }

        public T Get() => _defaultValue;

        public void Set(T value) => throw new InvalidOperationException("This should not happen");
    }

    public sealed class BindingState<T> : IState<T>
    {
        private readonly IState<T> _bounded;

        public BindingState(IState<T> bounded)
        {
            _bounded = bounded;
        }

        public T Get() => _bounded.Get();

        public void Set(T value) => _bounded.Set(value);
    }

    public static IState<T> UseDefault<T>(T t)
    {
        return new DefaultValueMarker<T>(t);
    }

    public static IState<T> Bind<T>(IState<T> s)
    {
        return new BindingState<T>(s);
    }

    public static IEnumerable<(FieldInfo, object?)> GetAllStateFields<V>(V view) where V: View
    {
        var fields = typeof(V).GetFields()
            .Where(f => f.FieldType.IsAssignableTo(typeof(IState)) && f.FieldType.GenericTypeArguments.Length > 0);
        var mapped = fields
            .Where(f =>
            {
                var val = f.GetValue(view);
                if (val == null)
                    return true;
                if (val.GetType().IsAssignableTo(typeof(DefaultValueMarker)) && val.GetType().GenericTypeArguments.Length > 0)
                    return true;
                return false;
            })
            .Select(f => (f, (f.GetValue(view) as DefaultValueMarker)?._defaultValue));
        return mapped;
    }
}
