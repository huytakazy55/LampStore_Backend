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
        private readonly ILogger<PayOSWebhookController> _logger;

        // Statuses for which a replayed/duplicate webhook must NOT re-apply side effects.
        private static readonly string[] TerminalStatuses = { "Refunded", "Cancelled" };

        public PayOSWebhookController(PayOSClient payOSClient, ApplicationDbContext context, ILogger<PayOSWebhookController> logger)
        {
            _payOSClient = payOSClient;
            _context = context;
            _logger = logger;
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
                        // Idempotency guard: if the order is already in a terminal state (or
                        // already marked Paid), a replayed/duplicate webhook must not re-apply
                        // side effects — just log and return success without reprocessing.
                        if (order.PaymentStatus == "Paid" || TerminalStatuses.Contains(order.Status))
                        {
                            _logger.LogInformation(
                                "[PayOS] Ignoring duplicate/replayed webhook for order {OrderCode}: PaymentStatus={PaymentStatus}, Status={Status}",
                                orderCode, order.PaymentStatus, order.Status);
                            return Ok(new { success = true });
                        }

                        // Update payment status (keep order Status as-is for admin to process)
                        order.PaymentStatus = "Paid";

                        await _context.SaveChangesAsync();
                        _logger.LogInformation("[PayOS] Order {OrderCode} paid successfully. PaymentStatus -> Paid", orderCode);
                    }
                    else
                    {
                        _logger.LogWarning("[PayOS] Order with code {OrderCode} not found.", orderCode);
                    }
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PayOS] Webhook Error");
                // Return 200 to prevent PayOS from retrying if it's a verification failure we don't care about,
                // or return BadRequest if we want them to know it failed.
                return Ok(new { success = false, message = "Webhook processing failed." });
            }
        }
    }
}
