using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;

namespace Viewify.Core.Utils;

public class ImmutableTreeHashTable<T> where T : class
{
    private readonly ImmutableDictionary<string, T> _data;
    private readonly ImmutableTreeHashTable<T>? _parent;
    private readonly MemoryCache _memoryCache;

    public ImmutableTreeHashTable(
        ImmutableTreeHashTable<T>? parent = null,
        IDictionary<string, T>? data = null)
    {
        _parent = parent;
        _data = data?.ToImmutableDictionary() ?? ImmutableDictionary<string, T>.Empty;
        _memoryCache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 1024
        });
    }

    public T? Get(string key)
    {
        if (_data.TryGetValue(key, out T? value))
        {
            return value;
        }
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue;
        }
        if (_parent != null)
        {
            var parentValue = _parent.Get(key);
            if (parentValue != null)
            {
                _memoryCache.Set(key, parentValue, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    Size = 1
                });
            }
            return parentValue;
        }

        return null;
    }

    public ImmutableTreeHashTable<T> CopyAndSet(string key, T value)
    {
        var newData = _data.SetItem(key, value);
        return new ImmutableTreeHashTable<T>(_parent, newData);
    }

    public ImmutableTreeHashTable<T> CopyAndRemove(string key)
    {
        var newData = _data.Remove(key);
        return new ImmutableTreeHashTable<T>(_parent, newData);
    }
}