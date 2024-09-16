using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Viewify.Base;

namespace Viewify.Core.Model;

public class ViewRecord
{
    public Type ViewType { get; }

    public IList<FieldInfo> PropFields { get; }
    public IList<PropertyInfo> PropProperties { get; }

    public IList<(FieldInfo, object?, IDefaultValueFactory?)> StateFields { get; }
    public IList<(PropertyInfo, object?, IDefaultValueFactory?)> StateProperties { get; }

    public IList<FieldInfo> ContextFields { get; }
    public IList<PropertyInfo> ContextProperties { get; }


    public IList<(MethodInfo, string[]?, bool, bool)> EffectMethods { get; }


    public ViewRecord(Type viewType)
    {
        if (!viewType.IsAssignableTo(typeof(View)) && !viewType.IsAssignableTo(typeof(Dependency)))
        {
            throw new InvalidOperationException();
        }

        ViewType = viewType;

        var fProps = new List<FieldInfo>();

        var pProps = new List<PropertyInfo>();

        var fStates = new List<(FieldInfo, object?, IDefaultValueFactory?)>();

        var pStates = new List<(PropertyInfo, object?, IDefaultValueFactory?)>();

        var fContexts = new List<FieldInfo>();

        var pContexts = new List<PropertyInfo>();


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

        foreach (var f in ViewType.GetFields())
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
                fStates.Add((f, stateFlag1?.Value, stateFlag2?.Factory));
            }
            else if (contextFlag != null)
            {
                fContexts.Add(f);
            }
        }

        foreach (var p in ViewType.GetProperties())
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
                pStates.Add((p, stateFlag1?.Value, stateFlag2?.Factory));
            }
            else if (contextFlag != null)
            {
                pContexts.Add(p);
            }
        }

        PropFields = fProps.AsReadOnly();
        PropProperties = pProps.AsReadOnly();
        StateFields = fStates.AsReadOnly();
        StateProperties = pStates.AsReadOnly();
        ContextFields = fContexts.AsReadOnly();
        ContextProperties = pContexts.AsReadOnly();

        // effects

        var mEffects = new List<(MethodInfo, string[]?, bool, bool)>();

        foreach (var p in ViewType.GetMethods())
        {
            var effectAttrs = p.GetCustomAttributes<EffectAttribute>();
            List<string> deps = new();
            bool normalFlag = false;
            foreach (var a in effectAttrs)
            {
                deps.AddRange(a.Dependencies);
                normalFlag = true;
            }
            bool mountFlag = p.GetCustomAttribute<MountEffectAttribute>() != null;
            bool unmountFlag = p.GetCustomAttribute<UnmountEffectAttribute>() != null;

            if (!mountFlag && normalFlag && deps.Count == 0)
            {
                mountFlag = true;
            }
            if (deps.Count > 0 || mountFlag || unmountFlag)
            {
                mEffects.Add((p, deps.ToArray(), mountFlag, unmountFlag));
            }
        }

        EffectMethods = mEffects.AsReadOnly();
    }

}
