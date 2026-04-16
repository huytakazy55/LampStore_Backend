using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        public async Task<ActionResult<OrderModel>> CreateOrder([FromBody] OrderModel orderModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
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