using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InjectAttribute(string prop, string? source = null, object? value = null) : Attribute
{
    public string Prop { get; init; } = prop;

    public string? Source { get; init; } = source ?? prop;

    public object? Value { get; init; } = value;
}
