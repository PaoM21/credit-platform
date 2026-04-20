using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using BatchProcessor.Functions.Models;

namespace BatchProcessor.Functions
{
    public static class AuditRegisterActivity
    {
        [FunctionName("AuditRegisterActivity")]
        public static async Task Run([ActivityTrigger] AuditInfo info, ILogger log)
        {
            log.LogInformation("Registering audit for credit {CreditId} action {Action}", info.CreditId, info.Action);
            await Task.Delay(50);
        }
    }
}