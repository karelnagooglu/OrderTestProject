using OrderApi.Models;

namespace OrderApi.DataLayer.Interfaces
{
    public interface IOrderQueue
    {
        void EnqueueItem(OrderItem orderItem);
        List<OrderItem> DequeueAllItems();
        int ItemCount();
    }
}
