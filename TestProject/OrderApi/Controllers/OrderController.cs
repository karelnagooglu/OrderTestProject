using Microsoft.AspNetCore.Mvc;
using OrderApi.DataLayer.Interfaces;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderQueue _orderQueue;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderQueue orderQueue, ILogger<OrderController> logger)
        {
            _orderQueue = orderQueue;
            _logger = logger;
        }

        // POST: api/Order
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            try
            {
                // await Task.Run(() => Parallel.ForEach(order.OrderItems, _orderQueue.EnqueueItem));
                order.OrderItems.ForEach(_orderQueue.EnqueueItem);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError("Error in {PostOrder} method. Msg: {Msg}", nameof(PostOrder), e.Message);
                throw;
            }
        }
    }
}
