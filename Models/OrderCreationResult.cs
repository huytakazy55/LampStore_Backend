namespace LampStoreProjects.Models
{
    /// <summary>
    /// Result of an order-creation attempt. Encapsulates server-side validation
    /// failures (out of stock, invalid discount code, price mismatch, etc.) so the
    /// controller can return a proper error response instead of assuming success.
    /// </summary>
    public class OrderCreationResult
    {
        public bool Success { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDetail { get; set; }
        public OrderModel? Order { get; set; }

        /// <summary>
        /// True when this result was served from a prior order matched by an
        /// Idempotency-Key, rather than created fresh by this call.
        /// </summary>
        public bool IsReplay { get; set; }

        public static OrderCreationResult Ok(OrderModel order) => new() { Success = true, Order = order };

        public static OrderCreationResult Replay(OrderModel order) => new() { Success = true, Order = order, IsReplay = true };

        public static OrderCreationResult Fail(string errorCode, string? detail = null) =>
            new() { Success = false, ErrorCode = errorCode, ErrorDetail = detail };
    }
}
