using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemRepository _orderitemRepository;

        public OrderItemsController(IOrderItemRepository orderitemRepository)
        {
            _orderitemRepository = orderitemRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItemModel>>> GetOrderItems()
        {
            var orderitems = await _orderitemRepository.GetAllAsync();
            return Ok(orderitems);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItemModel>> GetOrderItem(int id)
        {
            var orderitem = await _orderitemRepository.GetByIdAsync(id);
            if (orderitem == null)
            {
                return NotFound();
            }
            return Ok(orderitem);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrderItem(OrderItemModel orderitemModel)
        {
            await _orderitemRepository.AddAsync(orderitemModel);
            return CreatedAtAction(nameof(GetOrderItem), new { id = orderitemModel.Id }, orderitemModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateOrderItem(int id, OrderItemModel orderitemModel)
        {
            if (id != orderitemModel.Id)
            {
                return BadRequest();
            }
            await _orderitemRepository.UpdateAsync(orderitemModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrderItem(int id)
        {
            await _orderitemRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}