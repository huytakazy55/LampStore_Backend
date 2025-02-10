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
        public async Task<ActionResult> CreateOrder(OrderModel orderModel)
        {
            await _orderRepository.AddAsync(orderModel);
            return CreatedAtAction(nameof(GetOrder), new { id = orderModel.Id }, orderModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrder(Guid id, OrderModel orderModel)
        {
            if (id != orderModel.Id)
            {
                return BadRequest();
            }
            await _orderRepository.UpdateAsync(orderModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrder(Guid id)
        {
            await _orderRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}