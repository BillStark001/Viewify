using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Core.Utils;

namespace Viewify.Core.Render;

public class ViewRecordCache
{
    private static StatefulClassRecord GenerateCache(Type t)
    {
        return new StatefulClassRecord(t);
    }

    private readonly AdaptiveLRUCache<Type, StatefulClassRecord> _cache = new(32, GenerateCache);


    public StatefulClassRecord Get(Type t)
    {
        return _cache.Get(t);
    }

}
