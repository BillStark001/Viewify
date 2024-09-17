using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viewify.Core.Utils;

namespace Viewify.Core.Render;

public class ViewRecordCache
{
    private static ViewRecord GenerateCache(Type t)
    {
        return new ViewRecord(t);
    }

    private readonly AdaptiveLRUCache<Type, ViewRecord> _cache = new(32, GenerateCache);


    public ViewRecord Get(Type t)
    {
        return _cache.Get(t);
    }

}
