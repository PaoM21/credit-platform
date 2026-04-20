namespace CreditService.Infrastructure.Entities
{
    public class OutboxEvent
    {
        public Guid Id { get; set; }
        public string MessageId { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string Payload { get; set; } = default!; // JSON
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}