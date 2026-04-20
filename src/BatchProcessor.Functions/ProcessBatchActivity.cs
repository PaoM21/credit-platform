using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using BatchProcessor.Functions.Models;
using BatchProcessor.Functions.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BatchProcessor.Functions
{
    public static class ProcessBatchActivity
    {
        [FunctionName("ProcessBatchActivity")]
        public static async Task<string> RunAsync(
            [ActivityTrigger] BatchRange batch,
            ILogger log)
        {
            log.LogInformation($"Processing batch {batch.BatchId} {batch.StartId}-{batch.EndId}");

            using var db = DbContextFactory.Create();

            var items = await db.Credits
                .OrderBy(c => c.CreatedAt)
                .Skip((int)(batch.StartId - 1))
                .Take((int)(batch.EndId - batch.StartId + 1))
                .ToListAsync();

            foreach (var c in items)
            {
                if (c.Amount > 0 && c.Status != 1)
                {
                    c.Status = 1;
                    c.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (items.Any())
                await db.SaveChangesAsync();

            var state = await db.BatchProcessStates
                .FirstOrDefaultAsync(s => s.BatchId == batch.BatchId);

            if (state == null)
            {
                state = new BatchProcessStateLite
                {
                    Id = Guid.NewGuid(),
                    BatchId = batch.BatchId,
                    LastProcessedId = batch.EndId,
                    Status = "InProgress",
                    UpdatedAt = DateTime.UtcNow
                };
                db.BatchProcessStates.Add(state);
            }
            else
            {
                state.LastProcessedId = batch.EndId;
                state.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync();

            return $"Processed {batch.BatchId} {batch.StartId}-{batch.EndId}";
        }
    }
}