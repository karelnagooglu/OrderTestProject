using OrderApi.Models;

namespace OrderApi.DataLayer.Interfaces
{
    public interface IOrderPublisher
    {
        void PublishOrders(List<OrderItem> summarizedOrderItems);
    }
}
