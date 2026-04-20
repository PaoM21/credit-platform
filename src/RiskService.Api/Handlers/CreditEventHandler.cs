 using System.Threading.Tasks; using Microsoft.Extensions.Logging;
namespace RiskService.Api.Handlers
{
    public class CreditEventHandler
    {
        private readonly ILogger _log;
        public CreditEventHandler(ILogger log) => _log = log;
            public Task HandleAsync(string payloadJson)
    {
        _log.LogInformation("Risk handler processing payload: {Payload}", payloadJson);
        // Implementar lógica de riesgo aquí
        return Task.CompletedTask;
    }
}
}
