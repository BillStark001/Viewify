using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Base;

public struct ImmutableWrapper<T> where T : class
{
    private readonly T value;

    public ImmutableWrapper(T value)
    {
        this.value = value;
    }

    public static T operator ~(ImmutableWrapper<T> w) => w.value;

    public override string? ToString()
    {
        return value.ToString();
    }
}

public struct StringWrapper
{
    private readonly string value;

    public StringWrapper(string value)
    {
        this.value = value ?? "";
    }

    public int Length => value.Length;

    public string Value => value;

    public static string operator ~(StringWrapper w) => w.value;

    public bool IsEmpty => string.IsNullOrEmpty(value);

    public override string ToString()
    {
        return value;
    }
}
