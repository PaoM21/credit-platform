namespace CreditService.Infrastructure.Entities
{
    public class ProcessedMessage
    {
        public Guid Id { get; set; }
        public string MessageId { get; set; } = default!;
        public string HandlerName { get; set; } = default!;
        public DateTime ProcessedAt { get; set; }
    }
}