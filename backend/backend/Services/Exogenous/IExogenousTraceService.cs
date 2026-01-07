using CryptoAgent.Api.Models.Exogenous;

namespace CryptoAgent.Api.Services.Exogenous;

public interface IExogenousTraceService
{
    Task<ExogenousDecisionTraceDto?> GetTraceAsync(DateTime tickUtc, int topNarratives = 5, int topItems = 10);
}
