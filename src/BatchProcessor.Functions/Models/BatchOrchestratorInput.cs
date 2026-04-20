using System;

namespace BatchProcessor.Functions.Models
{
    public class BatchOrchestratorInput
    {
        public Guid CreditId { get; set; }
        public string BatchId { get; set; } = default!;
        public long TotalItems { get; set; }
        public int PageSize { get; set; } = 1000;
        public int MaxDegreeOfParallelism { get; set; } = 8;
    }
}