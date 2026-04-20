using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CreditService.Infrastructure.Persistence;
using CreditService.Infrastructure.Entities;

namespace Tests.Integration
{
    public class IdempotencyTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions _options;
            public IdempotencyTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<CreditDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new CreditDbContext(_options);
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ProcessingSameMessageTwice_InsertsProcessedMessageOnce_HandlerInvokedOnce()
    {
        var messageId = Guid.NewGuid().ToString();
        var handlerInvocations = 0;

        // Simulated business handler
        Func<string, Task> businessHandler = async (payload) =>
        {
            handlerInvocations++;
            await Task.CompletedTask;
        };

        // First processing: insert ProcessedMessage and call handler
        using (var ctx = new CreditDbContext(_options))
        {
            var pm = new ProcessedMessage
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                HandlerName = "TestHandler",
                ProcessedAt = DateTime.UtcNow
            };
            ctx.ProcessedMessages.Add(pm);
            await ctx.SaveChangesAsync();
            await businessHandler("p1");
        }

        // Second processing attempt: inserting duplicate should fail and not call handler
        using (var ctx2 = new CreditDbContext(_options))
        {
            var pm2 = new ProcessedMessage
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                HandlerName = "TestHandler",
                ProcessedAt = DateTime.UtcNow
            };

            ctx2.ProcessedMessages.Add(pm2);
            Exception? ex = null;
            try
            {
                await ctx2.SaveChangesAsync();
                await businessHandler("p2"); // would run only if save succeeded (unexpected)
            }
            catch (DbUpdateException dbex)
            {
                ex = dbex;
            }

            var count = await ctx2.ProcessedMessages.CountAsync(pm => pm.MessageId == messageId && pm.HandlerName == "TestHandler");
            Assert.Equal(1, count);
        }

        Assert.Equal(1, handlerInvocations);
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }
}
}
