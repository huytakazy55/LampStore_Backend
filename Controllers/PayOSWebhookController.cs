using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LampStoreProjects.Repositories;
using PayOS;
using PayOS.Models.Webhooks;
using System;
using Microsoft.EntityFrameworkCore;
using LampStoreProjects.Data;
using System.Linq;

namespace LampStoreProjects.Controllers
{
    [ApiController]
    [Route("api/payments/payos-webhook")]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly PayOSClient _payOSClient;
        private readonly ApplicationDbContext _context;

        public PayOSWebhookController(PayOSClient payOSClient, ApplicationDbContext context)
        {
            _payOSClient = payOSClient;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] Webhook webhookBody)
        {
            try
            {
                // Verify signature to ensure the webhook comes from PayOS
                var webhookData = await _payOSClient.Webhooks.VerifyAsync(webhookBody);

                if (webhookData.Code == "00")
                {
                    // Payment successful
                    long orderCode = webhookData.OrderCode;

                    // Find order by orderCode
                    var order = await _context.Orders!.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
                    if (order != null)
                    {
                        // Update order status
                        order.Status = "Processing"; // Move from Pending to Processing
                        // You can also add a PaymentStatus field if you want, e.g., order.PaymentStatus = "Paid";
                        
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"[PayOS] Order {orderCode} paid successfully.");
                    }
                    else
                    {
                        Console.WriteLine($"[PayOS] Order with code {orderCode} not found.");
                    }
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PayOS] Webhook Error: {ex.Message}");
                // Return 200 to prevent PayOS from retrying if it's a verification failure we don't care about, 
                // or return BadRequest if we want them to know it failed.
                return Ok(new { success = false, message = ex.Message });
            }
        }
    }
}
