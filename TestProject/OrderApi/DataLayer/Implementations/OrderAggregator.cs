using OrderApi.DataLayer.Interfaces;
using OrderApi.Models;

namespace OrderApi.DataLayer.Implementations
{
    public class OrderAggregator : IOrderAggregator
    {
        #region DI

        private readonly IOrderQueue _orderQueue;
        private readonly IOrderPublisher _orderPublisher;

        #endregion

        public OrderAggregator(IOrderQueue orderQueue, IOrderPublisher orderPublisher)
        {
            _orderQueue = orderQueue;
            _orderPublisher = orderPublisher;
        }

        public List<OrderItem> AggregateOrders()
        {
            List<OrderItem> aggregatedOrders = new List<OrderItem>();
            List<OrderItem> orderItems = _orderQueue.DequeueAllItems();

            IEnumerable<IGrouping<string, OrderItem>> groupedItems = orderItems.GroupBy(x => x.productId);

            foreach (var group in groupedItems)
            {
                var quantityTotal = group.Sum(x => x.quantity);

                aggregatedOrders.Add(new OrderItem
                {
                    productId = group.Key,
                    quantity = quantityTotal,
                });
            }

            _orderPublisher.PublishOrders(aggregatedOrders);

            return aggregatedOrders;
        }
    }
}
