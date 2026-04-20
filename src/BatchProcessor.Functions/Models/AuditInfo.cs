using System;
namespace BatchProcessor.Functions.Models
{
    public class AuditInfo
    {
        public Guid CreditId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? User { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}