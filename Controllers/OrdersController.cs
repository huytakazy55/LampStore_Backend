using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet("my-orders")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return Ok(orders);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetOrders()
        {
            var orders = await _orderRepository.GetAllAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderModel>> GetOrder(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrderModel>> CreateOrder([FromBody] OrderModel orderModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Always extract userId from JWT token (don't trust frontend value)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                orderModel.UserId = userId;
            }

            var created = await _orderRepository.CreateOrderAsync(orderModel);
            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, created);
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatusUpdateModel model)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var validStatuses = new[] { "Pending", "Confirmed", "Shipping", "Completed", "Cancelled" };
            if (!validStatuses.Contains(model.Status))
            {
                return BadRequest(new { message = $"Invalid status. Valid: {string.Join(", ", validStatuses)}" });
            }

            await _orderRepository.UpdateStatusAsync(id, model.Status);
            return Ok(new { message = "Status updated", status = model.Status });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            await _orderRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}