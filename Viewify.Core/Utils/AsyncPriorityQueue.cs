using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Viewify.Core.Utils;
public class AsyncPriorityQueue<TElement, TPriority>
{
    private readonly PriorityQueue<(TElement Element, long Index), TPriority> _queue;
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
    private long _index = 0;

    public AsyncPriorityQueue()
    {
        _queue = new PriorityQueue<(TElement, long), TPriority>();
    }

    public void Enqueue(TElement item, TPriority priority)
    {
        lock (_queue)
        {
            _queue.Enqueue((item, _index++), priority);
        }
        _signal.Release();
    }

    public async Task<TElement> DequeueAsync(CancellationToken cancellationToken = default)
    {
        await _signal.WaitAsync(cancellationToken);
        lock (_queue)
        {
            var (element, _) = _queue.Dequeue();
            return element;
        }
    }

    public bool IsEmpty
    {
        get
        {
            lock (_queue)
            {
                return _queue.Count == 0;
            }
        }
    }
}