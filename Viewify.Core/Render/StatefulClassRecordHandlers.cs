using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;
using Viewify.Core.Utils;

namespace Viewify.Core.Render;

public static class StatefulClassRecordHandlers
{
    // states
    public static void InitializeState(
        this StatefulClassRecord record,
        Fiber<ViewNode> node,
        Scheduler scheduler,
        bool temporary = false
    )
    {
        var instance = node.Content.View;

        foreach (var (field, type, defaultValue, factory) in record.StateFields)
        {
            var iv = factory?.Create() ?? defaultValue!;
            IState s = temporary
                ? ImmutableState<int>.Create(type, iv)
                : StateWithDispatch.Create(type, scheduler, node, iv);
            field.SetValue(instance, s);
        }

        foreach (var (property, type, defaultValue, factory) in record.StateProperties)
        {
            var iv = factory?.Create() ?? defaultValue!;
            IState s = temporary
                ? ImmutableState<int>.Create(type, iv)
                : StateWithDispatch.Create(type, scheduler, node, iv);
            property.SetValue(instance, s);
        }
    }

    public static void MigrateStateFiberNodes(this StatefulClassRecord record, IStateful? view, Fiber<ViewNode> fiber)
    {
        foreach (var (field, _, _, _) in record.StateFields)
        {
            var state = field.GetValue(view) as StateWithDispatch;
            if (state != null)
            {
                state.Fiber = fiber;
            }
        }

        foreach (var (property, _, _, _) in record.StateProperties)
        {
            var state = property.GetValue(view) as StateWithDispatch;
            if (state != null)
            {
                state.Fiber = fiber;
            }
        }
    }

    public static void MigrateStates(this StatefulClassRecord record, IStateful? source, IStateful? destination)
    {
        foreach (var (field, _, _, _) in record.StateFields)
        {
            if (field.GetValue(source) is IState sourceState)
            {
                field.SetValue(destination, sourceState);
            }
        }

        foreach (var (property, _, _, _) in record.StateProperties)
        {
            if (property.GetValue(source) is IState sourceState)
            {
                property.SetValue(destination, sourceState);
            }
        }
    }

    // props
    public static bool CompareAndMigrateProps(this StatefulClassRecord record, IStateful? source, IStateful? destination)
    {
        bool hasChange = false;

        foreach (var field in record.PropFields)
        {
            var oldValue = field.GetValue(source);
            var newValue = field.GetValue(destination);
            var neq = !Equals(oldValue, newValue);
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
            var neq = !Equals(oldValue, newValue);
            hasChange = hasChange || neq;
            if (neq)
            {
                property.SetValue(destination, oldValue);
            }
        }

        return hasChange;
    }

    // effect
    public static bool CompareAndCalculateEffectDependencies(this StatefulClassRecord record, IStateful? source, object?[] destination, bool[] changed)
    {
        bool hasChange = false;
        int i = 0;

        foreach (var (field, getter) in record.EffectDepFields)
        {
            var oldValue = destination[i];
            object? newValue = field.GetValue(source);
            if (getter != null) // this is an IState<>
            {
                newValue = getter.Invoke(newValue, null);
            }
            var neq = !Equals(oldValue, newValue);
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
            object? newValue = property.GetValue(source);
            if (getter != null) // this is an IState<>
            {
                newValue = getter.Invoke(newValue, null);
            }
            var neq = !Equals(oldValue, newValue);
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

    public static void ExecuteMountEffects(this StatefulClassRecord record, IStateful? view)
    {
        foreach (var item in record.MountEffects)
        {
            item.Invoke(view, null);
        }
    }
    public static void ExecuteUnmountEffects(this StatefulClassRecord record, IStateful? view)
    {
        foreach (var item in record.UnmountEffects)
        {
            item.Invoke(view, null);
        }
    }

    public static void ExecuteEffects(this StatefulClassRecord record, IStateful? view, bool[] changed)
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
    public static void InitializeContext(this StatefulClassRecord record, IStateful? view, ImmutableTreeHashTable<object> context)
    {
        foreach (var (field, key) in record.ContextFields)
        {
            var val = context.Get(key);
            if (val != null && field.FieldType.IsAssignableFrom(val.GetType()))
            {
                field.SetValue(view, val);
            }
        }
        foreach (var (property, key) in record.ContextProperties)
        {
            var val = context.Get(key);
            if (val != null && property.PropertyType.IsAssignableFrom(val.GetType()))
            {
                property.SetValue(view, val);
            }
        }
    }

    // dependencies

    static void InjectDependency(
        this StatefulClassRecord record,
        IStateful? view,
        IStateful? dependency,
        IEnumerable<(
            Func<object?, object?>,
            Action<object?, object?>
            )> methods
        )
    {
        if (view == null || dependency == null)
        {
            return;
        }
        foreach (var (get, set) in methods)
        {
            set(dependency, get(view));
        }
    }

    public static void InjectDependencies(this StatefulClassRecord record, IStateful? view)
    {
        foreach (var (f, methods) in record.DependencyFields)
        {
            record.InjectDependency(view, f.GetValue(view) as IStateful, methods);
        }
        foreach (var (p, methods) in record.DependencyProperties)
        {
            record.InjectDependency(view, p.GetValue(view) as IStateful, methods);
        }
    }

    //TODO
}
