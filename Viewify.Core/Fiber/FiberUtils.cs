using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Fiber;

public static class FiberUtils
{
    public static void InitializaStates(View instance, Action<Action> dispatcher)
    {
        Type instanceType = instance.GetType();
        FieldInfo[] fields = instanceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {
            var fieldType = field.FieldType;

            if (fieldType.GetGenericTypeDefinition() != typeof(IState<>))
            {
                continue;
            }

            Type valueType = fieldType.GenericTypeArguments[0];
            Type stateType = typeof(StateWithDispatch<>).MakeGenericType(fieldType.GenericTypeArguments);

            var defaultValue = GetDefaultValue(field, valueType);

            object? stateInstance = Activator.CreateInstance(stateType, dispatcher, defaultValue);

            field.SetValue(instance, stateInstance);
        }
    }


    private readonly static MethodInfo _getDefaultValueMethod = typeof(FiberUtils).GetMethod(nameof(GetDefaultValue))!;
    public static object? GetDefaultValue(FieldInfo field, Type valueType)
    {
        return _getDefaultValueMethod.MakeGenericMethod(valueType).Invoke(null, new[] { field });
    }

    public static T? GetDefaultValue<T>(FieldInfo field)
    {
        T? defaultValue = default;

        var defaultValueAttributes = field.GetCustomAttributes<DefaultStateAttribute>();
        var defaultValueFactoryAttributes = field.GetCustomAttributes<DefaultStateFactoryAttribute>();

        if (defaultValueAttributes.Count() > 0)
        {
            var lastValue = defaultValueAttributes.Last().Value;
            defaultValue = lastValue is T lastValueT ? lastValueT : default;
        }
        else if (defaultValueFactoryAttributes.Count() > 0)
        {
            var lastValue = defaultValueFactoryAttributes.Last().Factory.Create();
            defaultValue = lastValue is T lastValueT ? lastValueT : default;
        }

        return defaultValue;
    }
}
