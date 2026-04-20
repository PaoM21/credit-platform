using System; using System.Threading.Tasks; using Microsoft.Azure.WebJobs; using Microsoft.Azure.WebJobs.Extensions.DurableTask; using Microsoft.Extensions.Logging;
namespace BatchProcessor.Functions
{
    public static class ValidateCreditActivity
    {
        [FunctionName("ValidateCreditActivity")]
        public static async Task<bool> Run([ActivityTrigger] Guid creditId, ILogger log)
        {
            log.LogInformation("Validating credit {CreditId}", creditId);
            await Task.Delay(100);
            return true;
        }
    }
}