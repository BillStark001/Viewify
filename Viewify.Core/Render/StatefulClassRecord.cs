using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;
using Viewify.Core.Utils;

namespace Viewify.Core.Render;

public class StatefulClassRecord
{

    const BindingFlags F = BindingFlags.Public |
                           BindingFlags.NonPublic |
                           // BindingFlags.Static |
                           // BindingFlags.FlattenHierarchy |
                           BindingFlags.Instance;
    public Type ClassType { get; }

    public IList<(FieldInfo, IEnumerable<(Func<object?, object?>, Action<object?, object?>)>)> DependencyFields { get; }
    public IList<(PropertyInfo, IEnumerable<(Func<object?, object?>, Action<object?, object?>)>)> DependencyProperties { get; }

    public IList<FieldInfo> PropFields { get; }
    public IList<PropertyInfo> PropProperties { get; }

    public IList<(FieldInfo, Type, object?, IDefaultValueFactory?)> StateFields { get; }
    public IList<(PropertyInfo, Type, object?, IDefaultValueFactory?)> StateProperties { get; }

    public IList<(FieldInfo, string)> ContextFields { get; }
    public IList<(PropertyInfo, string)> ContextProperties { get; }


    public IList<MethodInfo> MountEffects { get; }
    public IList<MethodInfo> UnmountEffects { get; }
    public IDictionary<string, IList<MethodInfo>> Effects { get; }

    public IList<(FieldInfo, MethodInfo?)> EffectDepFields { get; }
    public IList<(PropertyInfo, MethodInfo?)> EffectDepProperties { get; }

    const string STATE_GET = nameof(IState<int>.Get);
    const string STATE_SET = nameof(IState<int>.Set);


    static bool IsValidStateDefinition(Type t)
    {
        return t.IsAssignableTo(typeof(IState)) && t.GetGenericTypeDefinition() == typeof(IState<>);
    }

    public StatefulClassRecord(Type type)
    {
        if (!type.IsAssignableTo(typeof(IStateful)))
        {
            throw new InvalidOperationException();
        }

        ClassType = type;

        var fDeps = new List<(FieldInfo, IEnumerable<(Func<object?, object?>, Action<object?, object?>)>)>();

        var pDeps = new List<(PropertyInfo, IEnumerable<(Func<object?, object?>, Action<object?, object?>)>)>();

        var fProps = new List<FieldInfo>();

        var pProps = new List<PropertyInfo>();

        var fStates = new List<(FieldInfo, Type, object?, IDefaultValueFactory?)>();

        var pStates = new List<(PropertyInfo, Type, object?, IDefaultValueFactory?)>();

        var fContexts = new List<(FieldInfo, string)>();

        var pContexts = new List<(PropertyInfo, string)>();


        PropAttribute? propFlag;
        StateAttribute? stateFlag1;
        ContextAttribute? contextFlag;
        List<(Func<object?, object?>, Action<object?, object?>)> injectMethods = new();

        bool isValidState;
        bool isDependency;

        void setFlags(Attribute a, Type? t)
        {
            if (a is PropAttribute ap)
            {
                propFlag = ap;
            }
            if (a is StateAttribute ads)
            {
                stateFlag1 = ads;
            }
            if (a is ContextAttribute ac)
            {
                contextFlag = ac;
            }
            if (a is InjectAttribute ai)
            {
                var _f = ai.Source != null ? ClassType.GetField(ai.Source) : null;
                var _ft = t?.GetField(ai.Prop);
                Func<object?, object?>? getter = _f != null ? _f.GetValue : null;
                Action<object?, object?>? setter = _ft != null ? _ft.SetValue : null;
                if (setter != null && getter != null)
                {
                    injectMethods.Add((getter, setter));
                }
            }
        }

        foreach (var f in ClassType.GetFields(F))
        {
            propFlag = null;
            stateFlag1 = null;
            contextFlag = null;
            injectMethods.Clear();
            isValidState = IsValidStateDefinition(f.FieldType);
            isDependency = f.FieldType.IsAssignableTo(typeof(IDependency));

            foreach (var a in f.GetCustomAttributes())
            {
                setFlags(a, f.FieldType);
            }

            if (propFlag != null)
            {
                fProps.Add(f);
            }
            else if (isValidState)
            {
                var genericArgument = f.FieldType.GetGenericArguments()[0];
                fStates.Add((f, genericArgument, stateFlag1?.Default, stateFlag1?.Factory));
            }
            else if (contextFlag != null)
            {
                fContexts.Add((f, f.FieldType.GetUniqueName()));
            }
            else if (isDependency)
            {
                fDeps.Add((f, injectMethods.ToImmutableList()));
            }
        }

        foreach (var p in ClassType.GetProperties(F))
        {
            propFlag = null;
            stateFlag1 = null;
            contextFlag = null;
            injectMethods.Clear();
            isValidState = IsValidStateDefinition(p.PropertyType);
            isDependency = p.PropertyType.IsAssignableTo(typeof(IDependency));

            foreach (var a in p.GetCustomAttributes())
            {
                setFlags(a, p.PropertyType);
            }

            if (propFlag != null)
            {
                pProps.Add(p);
            }
            else if (isValidState)
            {
                var genericArgument = p.PropertyType.GetGenericArguments()[0];
                pStates.Add((p, genericArgument, stateFlag1?.Default, stateFlag1?.Factory));
            }
            else if (contextFlag != null)
            {
                pContexts.Add((p, p.PropertyType.GetUniqueName()));
            }
            else if (isDependency)
            {
                pDeps.Add((p, injectMethods.ToImmutableList()));
            }
        }

        PropFields = fProps.AsReadOnly();
        PropProperties = pProps.AsReadOnly();
        StateFields = fStates.AsReadOnly();
        StateProperties = pStates.AsReadOnly();
        ContextFields = fContexts.AsReadOnly();
        ContextProperties = pContexts.AsReadOnly();
        DependencyFields = fDeps.AsReadOnly();
        DependencyProperties = pDeps.AsReadOnly();

        // effects

        var mountEffects = new List<MethodInfo>();
        var unmountEffects = new List<MethodInfo>();
        var effects = new Dictionary<string, List<MethodInfo>>();

        foreach (var p in ClassType.GetMethods(F))
        {
            var effectAttrs = p.GetCustomAttributes<EffectAttribute>();
            List<string> deps = [];
            bool normalFlag = false;
            foreach (var a in effectAttrs)
            {
                deps.AddRange(a.Dependencies);
                normalFlag = true;
            }
            bool mountFlag = p.GetCustomAttribute<MountEffectAttribute>() != null;
            bool unmountFlag = p.GetCustomAttribute<UnmountEffectAttribute>() != null;

            if (!mountFlag && normalFlag)
            {
                mountFlag = true;
            }

            // records
            if (mountFlag)
            {
                mountEffects.Add(p);
            }
            if (unmountFlag)
            {
                unmountEffects.Add(p);
            }
            foreach (var dep in deps)
            {
                var hasKey = effects.TryGetValue(dep, out var lst);
                if (!hasKey)
                {
                    lst = [];
                    effects.Add(dep, lst);
                }
                lst!.Add(p);
            }
        }

        MountEffects = mountEffects.AsReadOnly();
        UnmountEffects = unmountEffects.AsReadOnly();
        var effects2 = new Dictionary<string, IList<MethodInfo>>();
        foreach (var (k, v) in effects)
        {
            effects2[k] = v.AsReadOnly();
        }
        Effects = effects2.ToImmutableDictionary();


        // effect dependencies
        List<(FieldInfo, MethodInfo?)> effectDepFields = [];
        List<(PropertyInfo, MethodInfo?)> effectDepProperties = [];

        foreach (var k in Effects.Keys)
        {
            var f = type.GetField(k, F);
            var p = type.GetProperty(k, F);

            if (f != null)
            {
                effectDepFields.Add((f, f.FieldType.IsAssignableTo(typeof(IState)) ? f.FieldType.GetMethod(STATE_GET, F) : null));
            }
            else if (p != null)
            {
                effectDepProperties.Add((p, p.PropertyType.IsAssignableTo(typeof(IState)) ? p.PropertyType.GetMethod(STATE_GET, F) : null));
            }
        }

        EffectDepFields = effectDepFields.AsReadOnly();
        EffectDepProperties = effectDepProperties.AsReadOnly();
    }

}
