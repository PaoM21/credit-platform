using System;
using Microsoft.EntityFrameworkCore;

namespace BatchProcessor.Functions.Infrastructure
{
    public class LocalCreditDbContext : DbContext
    {
        public LocalCreditDbContext(DbContextOptions options) : base(options) { }
            public DbSet<CreditLite> Credits { get; set; }
    public DbSet<BatchProcessStateLite> BatchProcessStates { get; set; }
}

public class CreditLite
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; } // map enum as int
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BatchProcessStateLite
{
    public Guid Id { get; set; }
    public string BatchId { get; set; }
    public long LastProcessedId { get; set; }
    public string Status { get; set; }
    public DateTime UpdatedAt { get; set; }
}
}
