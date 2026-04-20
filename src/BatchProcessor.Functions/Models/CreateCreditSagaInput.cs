#nullable enable
using System;

namespace BatchProcessor.Functions.Models
{
    public class CreateCreditSagaInput
    {
        public Guid CreditId { get; set; }
        public string? CorrelationId { get; set; }
    }
}