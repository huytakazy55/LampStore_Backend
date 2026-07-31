using System.ComponentModel.DataAnnotations;

namespace LampStoreProjects.Data
{
    /// <summary>
    /// Backs request-level idempotency for order creation. A client-supplied key is
    /// inserted here (unique on Key) before any order side effects run, so a
    /// duplicate/retried request — double-submit, network retry, duplicate tab —
    /// fails on the unique constraint and gets routed back to the original order
    /// instead of creating a second one.
    /// </summary>
    public class IdempotencyKey
    {
        [Key]
        [MaxLength(100)]
        public string RequestKey { get; set; } = string.Empty;

        public Guid? OrderId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
