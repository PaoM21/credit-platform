namespace CreditService.Domain.Entities
{
    public enum CreditStatus { Draft = 0, Active = 1, Delinquent = 2, Closed = 3 }

    public class Credit
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public CreditStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; }
    }
}