using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductSpikeCase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int productId, int quantity)
        {
            var success = await _orderService.PlaceOrderAsync(productId, quantity);
            if (success)
            {
                return Ok("Order placed successfully.");
            }
            else
            {
                return BadRequest("Failed to place order.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddStock(int stock)
        {
            await _orderService.AddStock(stock);
            return Ok("add stock successfully.");
        }
    }
}
