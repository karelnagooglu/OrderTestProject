using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderApi;
using OrderApi.DataLayer.Interfaces;
using OrderApi.Models;

namespace Tests
{
    public class OrderAggregatorTests
    {
        private readonly WebApplication _webApplication;
        private readonly int _orderCountTrigger;

        public OrderAggregatorTests()
        {
            _webApplication = AppBuilder.Create(Array.Empty<string>(), true);
            var configuration = _webApplication.Services.GetRequiredService<IConfiguration>();
            _orderCountTrigger = configuration.GetValue<int>("Publishing:OrderCountTrigger");
        }

        [Fact]
        public void OrderAggregatorTest()
        {
            var orderQueue = _webApplication.Services.GetRequiredService<IOrderQueue>();
            var orderAggregator = _webApplication.Services.GetRequiredService<IOrderAggregator>();

            Order order = PrepareTestOrderData();

            // simulace metody PostOrder() Controlleru, ktera vlozi Objednavky do fronty
            order.OrderItems.ForEach(orderQueue.EnqueueItem);

            {
                List<OrderItem> aggregatedOrders = orderAggregator.AggregateOrders();

                Assert.Equal(2, aggregatedOrders.Count);

                Assert.Equal("product_1", aggregatedOrders[0].ProductId);
                Assert.Equal(10, aggregatedOrders[0].Quantity);

                Assert.Equal("product_2", aggregatedOrders[1].ProductId);
                Assert.Equal(10, aggregatedOrders[1].Quantity);
            }

            {
                var aggregatedOrders = orderAggregator.AggregateOrders();

                // predchozi Objednavky uz byly zpracovany, takze ted nevrati nic
                Assert.Equal(0, aggregatedOrders.Count);
            }

            {
                // simulace metody PostOrder() Controlleru, ktera vlozi Objednavky do fronty
                // vola se 2x pro stejna data => polozky jsou duplicitne a mely by byt agregovane
                order.OrderItems.ForEach(orderQueue.EnqueueItem);
                order.OrderItems.ForEach(orderQueue.EnqueueItem);

                var aggregatedOrders = orderAggregator.AggregateOrders();

                Assert.Equal(2, aggregatedOrders.Count);

                Assert.Equal("product_1", aggregatedOrders[0].ProductId);
                Assert.Equal(20, aggregatedOrders[0].Quantity);

                Assert.Equal("product_2", aggregatedOrders[1].ProductId);
                Assert.Equal(20, aggregatedOrders[1].Quantity);
            }
        }

        [Fact]
        public void OrderAggregatorTestViaWorker()
        {
            WebApplication webApplication = AppBuilder.Create(Array.Empty<string>(), true);
            var orderQueue = webApplication.Services.GetRequiredService<IOrderQueue>();
            var worker = webApplication.Services.GetRequiredService<Worker>();

            // zatim nejsou zadna data ke zpracovani
            {
                // simulace cinnosti Workeru
                int count = worker.OneIteration(new CancellationToken()).GetAwaiter().GetResult();

                // zatim neni co publikovat
                Assert.Equal(0, count);
            }

            {
                Order order = PrepareTestOrderData();

                // simulace metody PostOrder() Controlleru, ktera vlozi Objednavky do fronty
                order.OrderItems.ForEach(orderQueue.EnqueueItem);
                
                // simulace cinnosti Workeru
                int count = worker.OneIteration(new CancellationToken()).GetAwaiter().GetResult();

                // nemelo by se nic vypublikovat, protoze neni ani dosazeno limitu 3 objednavek, ani jeste neuplynul interval pro publikovani
                Assert.Equal(0, count);
            }

            {
                Assert.Equal(3, _orderCountTrigger); // pro jistotu kontrola hodnoty v appsettings.json, na ktere je tento test zavisly

                Order order = PrepareTestOrderData();

                // simulace metody PostOrder() Controlleru, ktera vlozi Objednavky do fronty
                // vola se 3x pro stejna data => dosahne se limitu 3 objednavek => vypublikuje se hned
                order.OrderItems.ForEach(orderQueue.EnqueueItem);
                order.OrderItems.ForEach(orderQueue.EnqueueItem);
                order.OrderItems.ForEach(orderQueue.EnqueueItem);

                // simulace cinnosti Workeru
                int count = worker.OneIteration(new CancellationToken()).GetAwaiter().GetResult();

                // vypublikuji se 2 objednavky
                Assert.Equal(2, count);
            }
        }

        private static Order PrepareTestOrderData()
        {
            Order order = new Order
            {
                OrderItems = new List<OrderItem>
                {
                    new OrderItem()
                    {
                        ProductId = "product_1",
                        Quantity = 10,
                    },
                    new OrderItem()
                    {
                        ProductId = "product_2",
                        Quantity = 10,
                    }
                }
            };
            return order;
        }
    }
}