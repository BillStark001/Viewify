using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Model;

public static class ViewRecordHandlers
{
    // states
    public static void InitializeState(this ViewRecord record, View? instance)
    {
        foreach (var (field, type, defaultValue, factory, _, setter) in record.StateFields)
        {
            if (field.GetValue(instance) is IState state)
            {
                SetStateValue(state, defaultValue, factory, type, setter);
            }
        }

        foreach (var (property, type, defaultValue, factory, _, setter) in record.StateProperties)
        {
            if (property.GetValue(instance) is IState state)
            {
                SetStateValue(state, defaultValue, factory, type, setter);
            }
        }
    }

    public static void SetStateValue(
        IState state, object? defaultValue,
        IDefaultValueFactory? factory,
        Type genericArgument, MethodInfo? setValue
       )
    {
        if (setValue == null)
            throw new InvalidOperationException("Set method not found on IState<T>");

        object? value = factory?.Create() ?? defaultValue;
        if (value != null && !genericArgument.IsInstanceOfType(value))
            throw new InvalidOperationException($"Default value type mismatch. Expected {genericArgument.Name}, got {value.GetType().Name}");

        setValue.Invoke(state, [value]);
    }

    public static void MigrateStates(this ViewRecord record, View? source, View? destination)
    {
        foreach (var (field, _, _, _, g1, s1) in record.StateFields)
        {
            if (field.GetValue(source) is IState sourceState && field.GetValue(destination) is IState destState)
            {
                MigrateState(sourceState, destState, g1, s1);
            }
        }

        foreach (var (property, _, _, _, g1, s1) in record.StateProperties)
        {
            if (property.GetValue(source) is IState sourceState && property.GetValue(destination) is IState destState)
            {
                MigrateState(sourceState, destState, g1, s1);
            }
        }
    }

    public static void MigrateState(
        IState sourceState, IState destState,
        MethodInfo? sourceGetMethod, MethodInfo? destSetMethod)
    {
        if (sourceGetMethod == null || destSetMethod == null)
            throw new InvalidOperationException("Get or Set method not found on IState<T>");

        var value = sourceGetMethod.Invoke(sourceState, null);
        destSetMethod.Invoke(destState, [value]);
    }

    // props
    public static bool CompareAndMigrateProps(this ViewRecord record, View? source, View? destination)
    {
        bool hasChange = false;

        foreach (var field in record.PropFields)
        {
            var oldValue = field.GetValue(source);
            var newValue = field.GetValue(destination);
            var neq = oldValue != newValue;
            hasChange = hasChange || neq;
            if (neq)
            {
                field.SetValue(destination, oldValue);
            }
        }

        foreach (var property in record.PropProperties)
        {
            var oldValue = property.GetValue(source);
            var newValue = property.GetValue(destination);
            var neq = oldValue != newValue;
            hasChange = hasChange || neq;
            if (neq)
            {
                property.SetValue(destination, oldValue);
            }
        }

        return hasChange;
    }

    // effect
    public static bool CompareAndCalculateEffectDependencies(this ViewRecord record, View? source, object?[] destination, bool[] changed)
    {
        bool hasChange = false;
        int i = 0;

        foreach (var (field, getter) in record.EffectDepFields)
        {
            var oldValue = destination[i];
            var newValue = getter != null
                ? getter.Invoke(source, null) : field.GetValue(source);
            var neq = oldValue != newValue;
            hasChange = hasChange || neq;
            changed[i] = neq;
            if (neq)
            {
                destination[i] = newValue;
            }
            ++i;
        }

        foreach (var (property, getter) in record.EffectDepProperties)
        {
            var oldValue = destination[i];
            var newValue = getter != null
                ? getter.Invoke(source, null) : property.GetValue(source);
            var neq = oldValue != newValue;
            hasChange = hasChange || neq;
            changed[i] = neq;
            if (neq)
            {
                destination[i] = newValue;
            }
            ++i;
        }

        return hasChange;
    }

    public static void ExecuteMountEffects(this ViewRecord record, View? view)
    {
        foreach (var item in record.MountEffects)
        {
            item.Invoke(view, null);
        }
    }
    public static void ExecuteUnmountEffects(this ViewRecord record, View? view)
    {
        foreach (var item in record.UnmountEffects)
        {
            item.Invoke(view, null);
        }
    }

    public static void ExecuteEffects(this ViewRecord record, View? view, bool[] changed)
    {
        int i = 0;

        foreach (var (field, _) in record.EffectDepFields)
        {
            if (changed[i])
            {
                foreach (var item in record.Effects[field.Name])
                {
                    item.Invoke(view, null);
                }
            }
            ++i;
        }

        foreach (var (property, _) in record.EffectDepProperties)
        {
            if (changed[i])
            {
                foreach (var item in record.Effects[property.Name])
                {
                    item.Invoke(view, null);
                }
            }
            ++i;
        }
    }

    // context
    // TODO

    // custom hooks
    //TODO
}
