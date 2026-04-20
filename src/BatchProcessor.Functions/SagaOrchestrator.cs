using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using BatchProcessor.Functions.Models;

namespace BatchProcessor.Functions
{
    public static class SagaOrchestrator
    {
        [FunctionName("SagaOrchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = context.GetInput<CreateCreditSagaInput>();

            if (input == null || input.CreditId == Guid.Empty)
            {
                throw new ArgumentException(
                    "CreateCreditSagaInput con CreditId válido es requerido");
            }

            try
            {
                var isValid = await context.CallActivityAsync<bool>(
                    "ValidateCreditActivity",
                    input.CreditId);

                if (!isValid)
                {
                    await context.CallActivityAsync(
                        "CompensateCreditActivity",
                        input.CreditId);

                    return;
                }

                var auditInfo = new AuditInfo
                {
                    CreditId = input.CreditId,
                    Action = "Validated"
                };

                await context.CallActivityAsync(
                    "AuditRegisterActivity",
                    auditInfo);

                var evt = new EventPayload
                {
                    MessageType = "CreditValidated",
                    CreditId = input.CreditId,
                    CorrelationId = input.CorrelationId,
                    MessageId = context.NewGuid().ToString(),
                    Data = new { input.CreditId }
                };

                await context.CallActivityAsync(
                    "PublishEventActivity",
                    evt);
            }
            catch
            {
                await context.CallActivityAsync(
                    "CompensateCreditActivity",
                    input.CreditId);

                throw;
            }
        }
    }
}