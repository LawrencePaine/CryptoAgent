using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly PerformanceRepository _performanceRepository;

    public PerformanceController(PerformanceRepository performanceRepository)
    {
        _performanceRepository = performanceRepository;
    }

    [HttpGet("monthly")]
    public async Task<ActionResult<IEnumerable<object>>> GetMonthlyPerformance()
    {
        var all = await _performanceRepository.GetAllAsync();

        var groups = all.GroupBy(x => new { x.DateUtc.Year, x.DateUtc.Month })
                        .OrderBy(g => g.Key.Year)
                        .ThenBy(g => g.Key.Month)
                        .Select(g =>
                        {
                            var first = g.First();
                            var last = g.Last();
                            var pnl = last.PortfolioValueGbp - first.PortfolioValueGbp;
                            var aiCost = last.CumulatedAiCostGbp - first.CumulatedAiCostGbp;
                            var fees = last.CumulatedFeesGbp - first.CumulatedFeesGbp;

                            return new
                            {
                                Year = g.Key.Year,
                                Month = g.Key.Month,
                                StartValue = first.PortfolioValueGbp,
                                EndValue = last.PortfolioValueGbp,
                                PnlGbp = pnl,
                                AiCostGbp = aiCost,
                                FeesGbp = fees,
                                NetAfterAiAndFeesGbp = pnl - aiCost - fees,
                                VaultEndGbp = last.VaultGbp
                            };
                        })
                        .ToList();

        return Ok(groups);
    }
}
