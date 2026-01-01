using System.Text.Json;
using CryptoAgent.Api.Models.Exogenous;
using Microsoft.AspNetCore.Hosting;

namespace CryptoAgent.Api.Services.Exogenous;

public class ExogenousSourceRegistry
{
    private readonly string _registryPath;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private List<ExogenousSourceDefinition>? _sources;

    public ExogenousSourceRegistry(IWebHostEnvironment environment)
    {
        _registryPath = Path.Combine(environment.ContentRootPath, "exogenous.sources.json");
    }

    public IReadOnlyList<ExogenousSourceDefinition> GetSources()
    {
        if (_sources != null)
        {
            return _sources;
        }

        if (!File.Exists(_registryPath))
        {
            _sources = new List<ExogenousSourceDefinition>();
            return _sources;
        }

        var json = File.ReadAllText(_registryPath);
        _sources = JsonSerializer.Deserialize<List<ExogenousSourceDefinition>>(json, _serializerOptions)
                   ?? new List<ExogenousSourceDefinition>();

        return _sources;
    }
}
