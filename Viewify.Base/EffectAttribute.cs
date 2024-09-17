using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

[AttributeUsage(AttributeTargets.Method)]
public class EffectAttribute(params string[] dependencies) : Attribute
{
    public string[] Dependencies { get; init; } = (string[])dependencies.Clone();
}

[AttributeUsage(AttributeTargets.Method)]
public class MountEffectAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Method)]
public class UnmountEffectAttribute : Attribute
{

}


