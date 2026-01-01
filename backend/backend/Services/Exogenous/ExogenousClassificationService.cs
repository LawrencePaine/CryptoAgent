using System.Text.Json;
using CryptoAgent.Api.Data.Entities;
using CryptoAgent.Api.Models.Exogenous;
using CryptoAgent.Api.Repositories;
using OpenAI.Chat;
using Serilog;

namespace CryptoAgent.Api.Services.Exogenous;

public interface IExogenousClassifier
{
    Task<ExogenousClassifierResult> ClassifyAsync(ExogenousItemEntity item, CancellationToken ct);
}

public class ExogenousClassificationService : IExogenousClassifier
{
    private readonly ChatClient _chatClient;
    private readonly ExogenousClassificationRepository _classificationRepository;
    private readonly ExogenousItemRepository _itemRepository;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ExogenousClassificationService(
        ChatClient chatClient,
        ExogenousClassificationRepository classificationRepository,
        ExogenousItemRepository itemRepository)
    {
        _chatClient = chatClient;
        _classificationRepository = classificationRepository;
        _itemRepository = itemRepository;
    }

    public async Task<ExogenousClassifierResult> ClassifyAsync(ExogenousItemEntity item, CancellationToken ct)
    {
        if (IsLowQuality(item))
        {
            await _itemRepository.UpdateStatusAsync(item.Id, "FAILED", "Filtered by low-quality heuristic");
            throw new InvalidOperationException("Filtered by low-quality heuristic");
        }

        var prompt = BuildPrompt(item);
        string response;
        try
        {
            response = await GetCompletionAsync(prompt, ct);
        }
        catch (Exception ex)
        {
            await _itemRepository.UpdateStatusAsync(item.Id, "FAILED", $"LLM failure: {ex.Message}");
            throw;
        }

        var parsed = ParseResult(response);
        if (parsed == null)
        {
            var retryPrompt = prompt + "\n\nFix JSON only. Return valid JSON that matches the schema.";
            try
            {
                var retryResponse = await GetCompletionAsync(retryPrompt, ct);
                parsed = ParseResult(retryResponse);
            }
            catch (Exception ex)
            {
                await _itemRepository.UpdateStatusAsync(item.Id, "FAILED", $"LLM failure: {ex.Message}");
                throw;
            }
        }

        if (parsed == null)
        {
            await _itemRepository.UpdateStatusAsync(item.Id, "FAILED", "Classification schema validation failed");
            throw new InvalidOperationException("Classification schema validation failed");
        }

        var classificationEntity = new ExogenousClassificationEntity
        {
            Id = Guid.NewGuid(),
            ItemId = item.Id,
            ThemeRelevance = parsed.ThemeRelevance.ToString(),
            ImpactHorizon = parsed.ImpactHorizon.ToString(),
            DirectionalBias = parsed.DirectionalBias.ToString(),
            ConfidenceScore = parsed.ConfidenceScore,
            NoveltyScore = parsed.NoveltyScore,
            SummaryBulletsJson = JsonSerializer.Serialize(parsed.SummaryBullets),
            KeyEntitiesJson = JsonSerializer.Serialize(parsed.KeyEntities),
            CreatedAt = DateTime.UtcNow
        };

        await _classificationRepository.AddAsync(classificationEntity);
        await _itemRepository.UpdateStatusAsync(item.Id, "CLASSIFIED", null);

        Log.Information(
            "Exogenous classification item={ItemId} theme={Theme} confidence={Confidence}",
            item.Id,
            parsed.ThemeRelevance,
            parsed.ConfidenceScore);

        return parsed;
    }

    private async Task<string> GetCompletionAsync(string prompt, CancellationToken ct)
    {
        var completion = await _chatClient.CompleteChatAsync(new List<ChatMessage>
        {
            new SystemChatMessage("You are a careful analyst classifying crypto-related news. Return only JSON."),
            new UserChatMessage(prompt)
        }, null, ct);

        return completion.Content[0].Text.Replace("```json", "").Replace("```", "").Trim();
    }

    private static string BuildPrompt(ExogenousItemEntity item)
    {
        return $@"Classify the following news item into exogenous themes.

Rules:
- Use only the text provided.
- No price predictions.
- Provide 3-6 concise bullet summaries.
- Provide 2-8 key entities.
- theme_relevance must be AI_COMPUTE, ETH_ECOSYSTEM, or NONE.
- impact_horizon must be NOISE, TRANSITIONAL, or STRUCTURAL.
- directional_bias must be SUPPORTIVE, ADVERSE, or NEUTRAL.
- confidence_score must be 0 to 1.
- cite what in the text implies the classification.

Return JSON only matching this schema:
{{
  "theme_relevance": "AI_COMPUTE",
  "impact_horizon": "STRUCTURAL",
  "directional_bias": "SUPPORTIVE",
  "confidence_score": 0.74,
  "summary_bullets": ["..."],
  "key_entities": ["..."]
}}

TITLE: {item.Title}
EXCERPT: {item.RawExcerpt}
SOURCE_ID: {item.SourceId}";
    }

    private static ExogenousClassifierResult? ParseResult(string payload)
    {
        try
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(payload, SerializerOptions);
            if (doc.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            var themeValue = doc.GetProperty("theme_relevance").GetString();
            var horizonValue = doc.GetProperty("impact_horizon").GetString();
            var biasValue = doc.GetProperty("directional_bias").GetString();
            var confidence = doc.GetProperty("confidence_score").GetDecimal();
            var bullets = doc.GetProperty("summary_bullets").EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var entities = doc.GetProperty("key_entities").EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            if (!Enum.TryParse(themeValue, true, out ExogenousTheme theme))
            {
                return null;
            }

            if (!Enum.TryParse(horizonValue, true, out ExogenousImpactHorizon horizon))
            {
                return null;
            }

            if (!Enum.TryParse(biasValue, true, out ExogenousDirectionalBias bias))
            {
                return null;
            }

            if (confidence < 0 || confidence > 1)
            {
                return null;
            }

            if (bullets.Count == 0 || bullets.Count > 6)
            {
                return null;
            }

            if (entities.Count == 0)
            {
                return null;
            }

            decimal? novelty = null;
            if (doc.TryGetProperty("novelty_score", out var noveltyElement) && noveltyElement.ValueKind == JsonValueKind.Number)
            {
                novelty = noveltyElement.GetDecimal();
            }

            return new ExogenousClassifierResult
            {
                ThemeRelevance = theme,
                ImpactHorizon = horizon,
                DirectionalBias = bias,
                ConfidenceScore = confidence,
                NoveltyScore = novelty,
                SummaryBullets = bullets,
                KeyEntities = entities
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool IsLowQuality(ExogenousItemEntity item)
    {
        var text = $"{item.Title} {item.RawExcerpt}";
        if (text.Length < 80)
        {
            return true;
        }

        var lower = text.ToLowerInvariant();
        return lower.Contains("sponsored") || lower.Contains("advert") || lower.Contains("pr newswire");
    }
}
