using OrderApi.DataLayer.Interfaces;
using OrderApi.Models;

namespace OrderApi.DataLayer.Implementations
{
    public class OrderPublisher : IOrderPublisher
    {
        public void PublishOrders(List<OrderItem> summarizedOrderItems)
        {
            foreach (OrderItem orderItem in summarizedOrderItems)
            {
                Console.WriteLine($"Aggregated order: Product Id={orderItem.productId}, Quantity total={orderItem.quantity}");    
            }
        }
    }
}
