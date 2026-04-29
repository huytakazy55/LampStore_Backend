using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LampStoreProjects.Models;
using LampStoreProjects.Repositories;
using LampStoreProjects.Services;
using LampStoreProjects.Data;
using Microsoft.AspNetCore.Identity;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(IOrderRepository orderRepository, IEmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _orderRepository = orderRepository;
            _emailService = emailService;
            _userManager = userManager;
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
        public async Task<ActionResult<OrderModel>> CreateOrder([FromBody] OrderModel orderModel, [FromServices] PayOSClient? payOSClient)
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

            // Send emails asynchronously (fire and forget to not block response)
            var storeUrl = Request.Headers.Origin.FirstOrDefault() ?? "https://capylumine.com";
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminEmails = adminUsers.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)).ToList()!;
            _ = Task.Run(async () =>
            {
                if (!string.IsNullOrEmpty(created.Email))
                {
                    await _emailService.SendOrderConfirmationEmailAsync(created, storeUrl);
                }
                await _emailService.SendNewOrderNotificationToAdminAsync(created, adminEmails!, storeUrl);
            });

            if (created.PaymentMethod == "bank" && payOSClient != null)
            {
                try
                {
                    var paymentData = new CreatePaymentLinkRequest
                    {
                        OrderCode = created.OrderCode,
                        Amount = (int)created.TotalAmount,
                        Description = $"Thanh toan {created.OrderCode}",
                        CancelUrl = $"{storeUrl}/checkout?orderCancel=true&orderCode={created.OrderCode}",
                        ReturnUrl = $"{storeUrl}/checkout?orderSuccess=true&orderCode={created.OrderCode}"
                    };

                    var createPayment = await payOSClient.PaymentRequests.CreateAsync(paymentData);
                    created.CheckoutUrl = createPayment.CheckoutUrl;
                }
                catch (Exception ex)
                {
                    // Log error but don't fail order creation
                    Console.WriteLine($"PayOS Error: {ex.Message}");
                }
            }

            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, created);
        }

        /// <summary>
        /// Create order for guest user (no authentication required).
        /// Requires a GuestToken to track the order.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("guest")]
        public async Task<ActionResult<OrderModel>> CreateGuestOrder([FromBody] OrderModel orderModel, [FromServices] PayOSClient? payOSClient)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrEmpty(orderModel.GuestToken))
            {
                return BadRequest(new { message = "GuestToken is required for guest orders." });
            }

            // Ensure no userId is set for guest orders
            orderModel.UserId = null;

            var created = await _orderRepository.CreateOrderAsync(orderModel);

            // Send confirmation email + admin notification asynchronously
            var storeUrlGuest = Request.Headers.Origin.FirstOrDefault() ?? "https://capylumine.com";
            var adminUsersGuest = await _userManager.GetUsersInRoleAsync("Admin");
            var adminEmailsGuest = adminUsersGuest.Select(u => u.Email).Where(e => !string.IsNullOrEmpty(e)).ToList()!;
            _ = Task.Run(async () =>
            {
                if (!string.IsNullOrEmpty(created.Email))
                {
                    await _emailService.SendOrderConfirmationEmailAsync(created, storeUrlGuest);
                }
                await _emailService.SendNewOrderNotificationToAdminAsync(created, adminEmailsGuest!, storeUrlGuest);
            });

            if (created.PaymentMethod == "bank" && payOSClient != null)
            {
                try
                {
                    var paymentData = new CreatePaymentLinkRequest
                    {
                        OrderCode = created.OrderCode,
                        Amount = (int)created.TotalAmount,
                        Description = $"Thanh toan {created.OrderCode}",
                        CancelUrl = $"{storeUrlGuest}/checkout?orderCancel=true&orderCode={created.OrderCode}",
                        ReturnUrl = $"{storeUrlGuest}/checkout?orderSuccess=true&orderCode={created.OrderCode}"
                    };

                    var createPayment = await payOSClient.PaymentRequests.CreateAsync(paymentData);
                    created.CheckoutUrl = createPayment.CheckoutUrl;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"PayOS Error: {ex.Message}");
                }
            }

            return CreatedAtAction(nameof(GetOrder), new { id = created.Id }, created);
        }

        /// <summary>
        /// Get orders by guest token (for guest users to view their orders)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("guest/{guestToken}")]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetGuestOrders(string guestToken)
        {
            if (string.IsNullOrEmpty(guestToken))
                return BadRequest(new { message = "GuestToken is required." });

            var orders = await _orderRepository.GetByGuestTokenAsync(guestToken);
            return Ok(orders);
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