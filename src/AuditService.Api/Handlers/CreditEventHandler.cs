 using System.Threading.Tasks; using Microsoft.Extensions.Logging;
namespace AuditService.Api.Handlers
{
    public class CreditEventHandler
    {
        private readonly ILogger _log;
        public CreditEventHandler(ILogger log) => _log = log;
            public Task HandleAsync(string payloadJson)
    {
        _log.LogInformation("Audit handler processing payload: {Payload}", payloadJson);
        // Implementar lógica de riesgo aquí
        return Task.CompletedTask;
    }
}
}
