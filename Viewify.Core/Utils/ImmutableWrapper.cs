using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Core.Utils;

public struct ImmutableWrapper<T>(T value) where T : class
{
    private readonly T value = value;

    public static T operator ~(ImmutableWrapper<T> w) => w.value;

    public override string? ToString()
    {
        return value.ToString();
    }
}

public struct StringWrapper(string value)
{
    private readonly string value = value ?? "";

    public int Length => value.Length;

    public string Value => value;

    public static string operator ~(StringWrapper w) => w.value;

    public bool IsEmpty => string.IsNullOrEmpty(value);

    public override string ToString()
    {
        return value;
    }
}
