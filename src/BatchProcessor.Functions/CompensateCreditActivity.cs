using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
namespace BatchProcessor.Functions
{
    public static class CompensateCreditActivity
    {
        [FunctionName("CompensateCreditActivity")]
        public static async Task Run([ActivityTrigger] Guid creditId, ILogger log)
        {
            log.LogWarning("Compensating credit {CreditId}, marking RolledBack", creditId);
            // Implementar compensación real (marcar estado, publicar evento, etc.)
            await Task.Delay(50);
        }
    }
}