using OrderApi.Models;

namespace OrderApi.DataLayer.Interfaces
{
    public interface IOrderAggregator
    {
        List<OrderItem> AggregateOrders();
    }
}
