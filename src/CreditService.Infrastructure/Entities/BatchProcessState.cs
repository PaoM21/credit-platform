namespace CreditService.Infrastructure.Entities
{
    public class BatchProcessState
    {
        public Guid Id { get; set; }
        public string BatchId { get; set; } = default!;
        public long LastProcessedId { get; set; }
        public string Status { get; set; } = default!; // e.g., Running, Completed, Failed
        public DateTime UpdatedAt { get; set; }
    }
}