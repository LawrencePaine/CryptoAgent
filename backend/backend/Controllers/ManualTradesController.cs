using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api")]
public class ManualTradesController : ControllerBase
{
    private readonly PaperTradeExecutionService _tradeExecutionService;
    private readonly PortfolioRepository _portfolioRepository;
    private readonly MarketDataService _marketDataService;
    private readonly PortfolioValuationService _valuationService;

    public ManualTradesController(
        PaperTradeExecutionService tradeExecutionService,
        PortfolioRepository portfolioRepository,
        MarketDataService marketDataService,
        PortfolioValuationService valuationService)
    {
        _tradeExecutionService = tradeExecutionService;
        _portfolioRepository = portfolioRepository;
        _marketDataService = marketDataService;
        _valuationService = valuationService;
    }

    [HttpPost("manual-trades")]
    public async Task<ActionResult<ManualTradeResponse>> ExecuteManualTrade([FromBody] ManualTradeRequest request, CancellationToken ct)
    {
        if (request.SizeGbp <= 0)
        {
            return BadRequest("sizeGbp must be greater than zero");
        }

        if (!TryParseAsset(request.Asset, out var symbol))
        {
            return BadRequest("asset must be BTC or ETH");
        }

        if (!TryParseAction(request.Action, out var side))
        {
            return BadRequest("action must be BUY or SELL");
        }

        try
        {
            var result = await _tradeExecutionService.ExecuteAsync(PortfolioBook.Manual, symbol, request.SizeGbp, side, ct);
            var valuation = _valuationService.Calculate(result.Portfolio, result.Market);
            var dto = result.Portfolio.ToDto(valuation);

            return Ok(new ManualTradeResponse
            {
                Portfolio = dto,
                Trade = result.Trade,
                Market = result.Market
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("manual-portfolio")]
    public async Task<ActionResult<PortfolioDto>> GetManualPortfolio(CancellationToken ct)
    {
        var portfolio = await _portfolioRepository.GetAsync(PortfolioBook.Manual);
        var market = await _marketDataService.GetSnapshotAsync();
        var valuation = _valuationService.Calculate(portfolio, market);
        return Ok(portfolio.ToDto(valuation));
    }

    private static bool TryParseAsset(string asset, out string symbol)
    {
        symbol = asset.Trim().ToUpperInvariant();
        return symbol is "BTC" or "ETH";
    }

    private static bool TryParseAction(string action, out OrderSide side)
    {
        var normalized = action.Trim().ToUpperInvariant();
        if (normalized == "BUY")
        {
            side = OrderSide.Buy;
            return true;
        }

        if (normalized == "SELL")
        {
            side = OrderSide.Sell;
            return true;
        }

        side = OrderSide.Buy;
        return false;
    }
}
