using OrderApi.DataLayer.Interfaces;
using OrderApi.Models;

namespace OrderApi.DataLayer.Implementations
{
    public class MemoryOrderQueue : IOrderQueue
    {
        private readonly Queue<OrderItem> _queue;
        private int _count;

        public MemoryOrderQueue()
        {
            _queue = new Queue<OrderItem>();
        }

        public void EnqueueItem(OrderItem orderItem)
        {
            _queue.Enqueue(orderItem);
            _count++;
        }

        public List<OrderItem> DequeueAllItems()
        {
            List<OrderItem> res = new List<OrderItem>();

            while (_queue.TryDequeue(out OrderItem orderItem))
            {
                res.Add(orderItem);
                _count--;
            }

            return res;
        }

        public int ItemCount()
        {
            return _count;
        }
    }
}
