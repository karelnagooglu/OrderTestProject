using Microsoft.AspNetCore.Mvc;
using OrderApi.DataLayer.Interfaces;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOrderQueue _orderQueue;

        public OrderController(IOrderQueue orderQueue, ILogger<Worker> logger)
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
                order.OrderItems.ForEach(_orderQueue.EnqueueItem);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError("Error in {PostOrder} method at {time}. Msg: {msg}", nameof(PostOrder), DateTimeOffset.Now, e.Message);
                throw;
            }
        }
    }
}
