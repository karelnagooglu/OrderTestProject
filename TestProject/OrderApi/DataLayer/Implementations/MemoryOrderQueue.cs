using System.Collections.Concurrent;
using OrderApi.DataLayer.Interfaces;
using OrderApi.Models;

namespace OrderApi.DataLayer.Implementations
{
    public class MemoryOrderQueue : IOrderQueue
    {
        private readonly ConcurrentQueue<OrderItem> _queue;
        private int _count;

        public MemoryOrderQueue()
        {
            _queue = new ConcurrentQueue<OrderItem>();
        }

        public void EnqueueItem(OrderItem orderItem)
        {
            _queue.Enqueue(orderItem);
            Interlocked.Increment(ref _count);
        }

        public List<OrderItem> DequeueAllItems()
        {
            List<OrderItem> res = new List<OrderItem>();

            while (_queue.TryDequeue(out OrderItem orderItem))
            {
                res.Add(orderItem);
                Interlocked.Decrement(ref _count);
            }

            return res;
        }

        public int ItemCount()
        {
            return _count;
        }
    }
}