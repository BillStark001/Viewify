using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Core.Utils;

public static class StringUtils
{
    public static string GetUniqueName(this Type type)
    {
        string assemblyName = type.Assembly.GetName().Name ?? "";
        string namespaceName = type.Namespace ?? "";
        string typeName = type.Name;

        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            string genericArgNames = string.Join(",", genericArgs.Select(t => GetUniqueName(t)));
            typeName = $"{typeName.Split('`')[0]}<{genericArgNames}>";
        }

        return $"{namespaceName}.{typeName}, {assemblyName}";
    }

}
