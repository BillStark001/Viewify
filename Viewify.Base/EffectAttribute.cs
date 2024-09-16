using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

[AttributeUsage(AttributeTargets.Method)]
public class EffectAttribute: Attribute
{
    public string[] Dependencies { get; init; }

    public EffectAttribute(params string[] dependencies)
    {
        this.Dependencies = (string[]) dependencies.Clone();
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class MountEffectAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class UnmountEffectAttribute : Attribute
{

}


