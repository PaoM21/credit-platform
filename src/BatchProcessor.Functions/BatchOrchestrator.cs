using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using BatchProcessor.Functions.Models;

namespace BatchProcessor.Functions
{
    public static class BatchOrchestrator
    {
        [FunctionName("BatchOrchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var input = context.GetInput<BatchOrchestratorInput>();

            var pageSize = input.PageSize;
            var maxDegree = input.MaxDegreeOfParallelism;
            long total = input.TotalItems;

            if (total == 0)
            {
                total = input.TotalItems;
            }

            var batches = new List<BatchRange>();

            for (long i = 0; i < total; i += pageSize)
            {
                var batchIdBase = input.BatchId ?? context.InstanceId;

                batches.Add(new BatchRange
                {
                    BatchId = $"{batchIdBase}_{i / pageSize}",
                    StartId = i + 1,
                    EndId = Math.Min(i + pageSize, total)
                });
            }

            var results = new List<string>();

            for (int offset = 0; offset < batches.Count; offset += maxDegree)
            {
                var chunk = batches.Skip(offset).Take(maxDegree);

                var tasks = chunk
                    .Select(b => context.CallActivityAsync<string>("ProcessBatchActivity", b))
                    .ToArray();

                var chunkResults = await Task.WhenAll(tasks);
                results.AddRange(chunkResults);
            }

            return results;
        }

        [FunctionName("BatchOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger("POST")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var input = await req.Content.ReadAsAsync<BatchOrchestratorInput>();

            string instanceId = await starter.StartNewAsync("BatchOrchestrator", input);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}