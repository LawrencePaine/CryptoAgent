using CryptoAgent.Api.Models.Exogenous;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Worker.Jobs.Exogenous;
using Microsoft.AspNetCore.Mvc;

namespace CryptoAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExogenousController : ControllerBase
{
    private readonly ExogenousItemRepository _itemRepository;
    private readonly NarrativeRepository _narrativeRepository;
    private readonly ExogenousNarrativeRebuildJob _rebuildJob;

    public ExogenousController(
        ExogenousItemRepository itemRepository,
        NarrativeRepository narrativeRepository,
        ExogenousNarrativeRebuildJob rebuildJob)
    {
        _itemRepository = itemRepository;
        _narrativeRepository = narrativeRepository;
        _rebuildJob = rebuildJob;
    }

    [HttpGet("items")]
    public async Task<ActionResult<List<ExogenousItemDto>>> GetItems([FromQuery] string status = "NEW", [FromQuery] int limit = 50)
    {
        var items = await _itemRepository.GetByStatusAsync(status, limit);
        var results = items.Select(i => new ExogenousItemDto
        {
            Id = i.Id,
            SourceId = i.SourceId,
            SourceCredibilityWeight = i.SourceCredibilityWeight,
            Title = i.Title,
            Url = i.Url,
            PublishedAt = i.PublishedAt,
            FetchedAt = i.FetchedAt,
            RawExcerpt = i.RawExcerpt,
            Language = i.Language,
            Status = Enum.TryParse<ExogenousItemStatus>(i.Status, true, out var parsed) ? parsed : ExogenousItemStatus.NEW,
            Error = i.Error
        }).ToList();

        return Ok(results);
    }

    [HttpGet("narratives")]
    public async Task<ActionResult<List<ExogenousNarrativeDto>>> GetNarratives([FromQuery] string theme)
    {
        if (string.IsNullOrWhiteSpace(theme))
        {
            return BadRequest("theme is required");
        }

        var narratives = await _narrativeRepository.GetByThemeAsync(theme);
        var results = narratives.Select(n => new ExogenousNarrativeDto
        {
            Id = n.Id,
            Theme = Enum.TryParse<ExogenousTheme>(n.Theme, true, out var parsed) ? parsed : ExogenousTheme.NONE,
            Label = n.Label,
            CreatedAt = n.CreatedAt,
            LastUpdatedAt = n.LastUpdatedAt,
            StateScore = n.StateScore,
            DirectionalBias = Enum.TryParse<ExogenousDirectionalBias>(n.DirectionalBias, true, out var bias) ? bias : ExogenousDirectionalBias.NEUTRAL,
            Horizon = Enum.TryParse<ExogenousImpactHorizon>(n.Horizon, true, out var horizon) ? horizon : ExogenousImpactHorizon.NOISE,
            IsActive = n.IsActive
        }).ToList();

        return Ok(results);
    }

    [HttpPost("narratives/rebuild")]
    public async Task<ActionResult> RebuildNarratives(CancellationToken ct)
    {
        await _rebuildJob.RunAsync(ct);
        return Accepted();
    }
}
