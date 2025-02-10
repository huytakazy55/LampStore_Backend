using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public DeliveriesController(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeliveryModel>>> GetDeliveries()
        {
            var deliveries = await _deliveryRepository.GetAllAsync();
            return Ok(deliveries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeliveryModel>> GetDelivery(Guid id)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(id);
            if (delivery == null)
            {
                return NotFound();
            }
            return Ok(delivery);
        }

        [HttpPost]
        public async Task<ActionResult> CreateDelivery(DeliveryModel deliveryModel)
        {
            await _deliveryRepository.AddAsync(deliveryModel);
            return CreatedAtAction(nameof(GetDelivery), new { id = deliveryModel.Id }, deliveryModel);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDelivery(Guid id, DeliveryModel deliveryModel)
        {
            if (id != deliveryModel.Id)
            {
                return BadRequest();
            }
            await _deliveryRepository.UpdateAsync(deliveryModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDelivery(Guid id)
        {
            await _deliveryRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}