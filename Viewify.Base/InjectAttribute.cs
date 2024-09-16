using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
    public string Name { get; init; }

    public object? Value { get; init; }

    public InjectAttribute(string name, object? value)
    {
        Name = name;
        Value = value;
    }
}
