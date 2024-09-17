using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute(string name, object? value) : Attribute
{
    public string Name { get; init; } = name;

    public object? Value { get; init; } = value;
}
