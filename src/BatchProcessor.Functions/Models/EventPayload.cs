using System;

namespace BatchProcessor.Functions.Models
{
    public class EventPayload
    {
        public string MessageType { get; set; }
        public Guid CreditId { get; set; }
        public string CorrelationId { get; set; }
        public string MessageId { get; set; }
        public object Data { get; set; }
    }
}