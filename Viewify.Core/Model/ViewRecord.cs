using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;
using Viewify.Core.Utils;

namespace Viewify.Core.Model;

public class ViewRecord
{

    const BindingFlags F = BindingFlags.Public |
                           BindingFlags.NonPublic |
                           // BindingFlags.Static |
                           // BindingFlags.FlattenHierarchy |
                           BindingFlags.Instance;
    public Type ViewType { get; }

    public IList<FieldInfo> PropFields { get; }
    public IList<PropertyInfo> PropProperties { get; }

    public IList<(FieldInfo, Type, object?, IDefaultValueFactory?, MethodInfo?, MethodInfo?)> StateFields { get; }
    public IList<(PropertyInfo, Type, object?, IDefaultValueFactory?, MethodInfo?, MethodInfo?)> StateProperties { get; }

    public IList<(FieldInfo, string)> ContextFields { get; }
    public IList<(PropertyInfo, string)> ContextProperties { get; }


    public IList<MethodInfo> MountEffects { get; }
    public IList<MethodInfo> UnmountEffects { get; }
    public IDictionary<string, IList<MethodInfo>> Effects { get; }

    public IList<(FieldInfo, MethodInfo?)> EffectDepFields { get; }
    public IList<(PropertyInfo, MethodInfo?)> EffectDepProperties { get; }

    const string STATE_GET = nameof(IState<int>.Get);
    const string STATE_SET = nameof(IState<int>.Set);


    public ViewRecord(Type viewType)
    {
        if (!viewType.IsAssignableTo(typeof(View)) && !viewType.IsAssignableTo(typeof(Dependency)))
        {
            throw new InvalidOperationException();
        }

        ViewType = viewType;

        var fProps = new List<FieldInfo>();

        var pProps = new List<PropertyInfo>();

        var fStates = new List<(FieldInfo, Type, object?, IDefaultValueFactory?, MethodInfo?, MethodInfo?)>();

        var pStates = new List<(PropertyInfo, Type, object?, IDefaultValueFactory?, MethodInfo?, MethodInfo?)>();

        var fContexts = new List<(FieldInfo, string)>();

        var pContexts = new List<(PropertyInfo, string)>();


        PropAttribute? propFlag;
        DefaultStateAttribute? stateFlag1;
        DefaultStateFactoryAttribute? stateFlag2;
        ContextAttribute? contextFlag;
        bool stateFlag0;

        void setFlags(Attribute a)
        {
            if (a is PropAttribute ap)
            {
                propFlag = ap;
            }
            if (a is DefaultStateAttribute ads)
            {
                stateFlag1 = ads;
                stateFlag2 = null;
            }
            if (a is DefaultStateFactoryAttribute ads2)
            {
                stateFlag1 = null;
                stateFlag2 = ads2;
            }
            if (a is ContextAttribute ac)
            {
                contextFlag = ac;
            }
        }

        foreach (var f in ViewType.GetFields(F))
        {
            propFlag = null;
            stateFlag1 = null;
            stateFlag2 = null;
            contextFlag = null;
            stateFlag0 = f.FieldType.IsAssignableTo(typeof(IState));

            foreach (var a in f.GetCustomAttributes())
            {
                setFlags(a);
            }

            if (propFlag != null)
            {
                fProps.Add(f);
            }
            else if (stateFlag0)
            {
                var genericArgument = f.FieldType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IState<>))
                    .GetGenericArguments()[0];
                fStates.Add((f, genericArgument, stateFlag1?.Value, stateFlag2?.Factory,
                    f.FieldType.GetMethod(STATE_GET, F),
                    f.FieldType.GetMethod(STATE_SET, F)));
            }
            else if (contextFlag != null)
            {
                fContexts.Add((f, f.FieldType.GetUniqueName()));
            }
        }

        foreach (var p in ViewType.GetProperties(F))
        {
            propFlag = null;
            stateFlag1 = null;
            stateFlag2 = null;
            contextFlag = null;
            stateFlag0 = p.PropertyType.IsAssignableTo(typeof(IState));

            foreach (var a in p.GetCustomAttributes())
            {
                setFlags(a);
            }

            if (propFlag != null)
            {
                pProps.Add(p);
            }
            else if (stateFlag0)
            {
                var genericArgument = p.PropertyType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IState<>))
                    .GetGenericArguments()[0];
                pStates.Add((p, genericArgument, stateFlag1?.Value, stateFlag2?.Factory,
                    p.PropertyType.GetMethod(STATE_GET, F),
                    p.PropertyType.GetMethod(STATE_SET, F)));
            }
            else if (contextFlag != null)
            {
                pContexts.Add((p, p.PropertyType.GetUniqueName()));
            }
        }

        PropFields = fProps.AsReadOnly();
        PropProperties = pProps.AsReadOnly();
        StateFields = fStates.AsReadOnly();
        StateProperties = pStates.AsReadOnly();
        ContextFields = fContexts.AsReadOnly();
        ContextProperties = pContexts.AsReadOnly();

        // effects

        var mountEffects = new List<MethodInfo>();
        var unmountEffects = new List<MethodInfo>();
        var effects = new Dictionary<string, List<MethodInfo>>();

        foreach (var p in ViewType.GetMethods(F))
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
            var f = viewType.GetField(k, F);
            var p = viewType.GetProperty(k, F);

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
