using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BatchProcessor.Functions.Models;
using System.Text.Json;

namespace BatchProcessor.Functions
{
    public static class HttpStarterForSaga
    {
        [FunctionName("Saga_HttpStart")]
        public static async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            using var sr = new System.IO.StreamReader(req.Body);
            var body = await sr.ReadToEndAsync();

            var input = JsonSerializer.Deserialize<BatchOrchestratorInput>(body);

            if (input == null || input.CreditId == System.Guid.Empty)
                return new BadRequestObjectResult("Provide CreditId in body");

            string instanceId = await starter.StartNewAsync("SagaOrchestrator", input);

            log.LogInformation($"Started SagaOrchestrator with ID = '{instanceId}'");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}