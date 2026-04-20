using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using CreditService.Domain.Entities;
using CreditService.Infrastructure.Persistence;
using CreditService.Infrastructure.Entities;
using CreditService.Application.Commands;

namespace CreditService.Infrastructure.Handlers
{
    public class CreateCreditHandler : IRequestHandler<CreateCreditCommand, Guid>
    {
        private readonly CreditDbContext _db;
        public CreateCreditHandler(CreditDbContext db) => _db = db;
        public async Task<Guid> Handle(CreateCreditCommand request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0) throw new ArgumentException("Amount must be greater than 0");
            if (request.CustomerId == Guid.Empty) throw new ArgumentException("CustomerId is required");

            var credit = new Credit
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Status = CreditStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            var outbox = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                MessageId = Guid.NewGuid().ToString(),
                Type = "CreditCreated",
                Payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    credit.Id,
                    credit.CustomerId,
                    credit.Amount,
                    credit.Status,
                    credit.CreatedAt
                }),
                CreatedAt = DateTime.UtcNow
            };

            using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _db.Credits.Add(credit);
                _db.OutboxEvents.Add(outbox);
                await _db.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }

            return credit.Id;
        }
    }
}
