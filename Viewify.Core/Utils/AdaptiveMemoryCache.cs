using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viewify.Core.Utils;

public class AdaptiveLRUCache<TKey, TValue> where TKey: notnull
{
    private class CacheItem
    {
        public TKey Key { get; set; } = default!;
        public TValue Value { get; set; } = default!;
        public int AccessCount { get; set; }
        public long ComputationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
    }

    private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cacheMap;
    private readonly LinkedList<CacheItem> _lruList;
    private readonly Func<TKey, TValue> _computeFunc;

    private int _capacity;

    private readonly object _lock = new();

    public AdaptiveLRUCache(int initialCapacity, Func<TKey, TValue> computeFunc)
    {
        _capacity = initialCapacity;
        _computeFunc = computeFunc;
        _cacheMap = new Dictionary<TKey, LinkedListNode<CacheItem>>(_capacity);
        _lruList = new LinkedList<CacheItem>();
    }

    public TValue Get(TKey key)
    {
        lock (_lock)
        {
            if (_cacheMap.TryGetValue(key, out LinkedListNode<CacheItem>? node))
            {
                // Cache hit
                var item = node.Value;
                _lruList.Remove(node);
                _lruList.AddFirst(node);
                item.AccessCount++;
                item.LastAccessTime = DateTime.Now;
                return item.Value;
            }
            else
            {
                // Cache miss
                var startTime = DateTime.Now;
                var value = _computeFunc(key);
                var computationTime = (DateTime.Now - startTime).Ticks;

                var cacheItem = new CacheItem
                {
                    Key = key,
                    Value = value,
                    AccessCount = 1,
                    ComputationTime = computationTime,
                    LastAccessTime = DateTime.Now
                };

                if (_cacheMap.Count >= _capacity)
                {
                    RemoveLeastUsed();
                }

                var newNode = _lruList.AddFirst(cacheItem);
                _cacheMap[key] = newNode;

                return value;
            }
        }
    }

    private void RemoveLeastUsed()
    {
        var leastUsed = _lruList.Last;
        _lruList.RemoveLast();
        _cacheMap.Remove(leastUsed!.Value.Key);
    }

    public void AdjustCacheSize()
    {
        lock (_lock)
        {
            var totalAccessCount = _lruList.Sum(item => item.AccessCount);
            var totalComputationTime = _lruList.Sum(item => item.ComputationTime);
            var averageComputationTime = totalComputationTime / _lruList.Count;

            var cacheEfficiency = (double)totalAccessCount * averageComputationTime / (_capacity * 1000000); // 假设1ms是一个合理的缓存开销

            if (cacheEfficiency > 2.0 && _capacity < 1000)
            {
                _capacity = (int)(_capacity * 1.5);
            }
            else if (cacheEfficiency < 0.5 && _capacity > 10)
            {
                _capacity = (int)(_capacity * 0.75);
                while (_cacheMap.Count > _capacity)
                {
                    RemoveLeastUsed();
                }
            }
        }
    }
}