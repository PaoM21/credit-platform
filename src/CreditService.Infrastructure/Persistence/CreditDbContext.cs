using Microsoft.EntityFrameworkCore;
using CreditService.Domain.Entities;
using CreditService.Infrastructure.Entities;

namespace CreditService.Infrastructure.Persistence
{
    public class CreditDbContext : DbContext
    {
        public CreditDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Credit> Credits { get; set; }
        public DbSet<OutboxEvent> OutboxEvents { get; set; }
        public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
        public DbSet<BatchProcessState> BatchProcessStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Credit>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
                b.Property(x => x.RowVersion).IsRowVersion();
                b.HasIndex(x => x.Status);
                b.HasIndex(x => x.CustomerId);
            });

            modelBuilder.Entity<OutboxEvent>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.MessageId).IsUnique(false);
            });

            modelBuilder.Entity<ProcessedMessage>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.MessageId, x.HandlerName }).IsUnique();
            });

            modelBuilder.Entity<BatchProcessState>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => x.BatchId).IsUnique();
            });
        }
    }
}