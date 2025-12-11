using CryptoAgent.Api.Data;
using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Config
var riskConfig = builder.Configuration.GetSection("RiskConfig").Get<RiskConfig>() ?? new RiskConfig();
var appConfig = builder.Configuration.GetSection("AppConfig").Get<AppConfig>() ?? new AppConfig();
var feeConfig = builder.Configuration.GetSection("FeeConfig").Get<FeeConfig>() ?? new FeeConfig();

builder.Services.AddSingleton(riskConfig);
builder.Services.AddSingleton(appConfig);
builder.Services.AddSingleton(feeConfig);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Database
var configuredConnection = builder.Configuration.GetConnectionString("CryptoAgentDb")
    ?? throw new InvalidOperationException("Connection string 'CryptoAgentDb' is not configured.");

// Ensure the SQLite file path is anchored to the content root so migrations and runtime
// use the same database file instead of creating separate copies per working directory.
const string dataSourcePrefix = "Data Source=";
var resolvedConnection = configuredConnection;
if (configuredConnection.StartsWith(dataSourcePrefix, StringComparison.OrdinalIgnoreCase))
{
    var dataSource = configuredConnection[dataSourcePrefix.Length..].Trim();
    if (!Path.IsPathRooted(dataSource))
    {
        var absolutePath = Path.Combine(builder.Environment.ContentRootPath, dataSource);
        resolvedConnection = $"{dataSourcePrefix}{absolutePath}";
    }
}

builder.Services.AddDbContext<CryptoAgentDbContext>(options =>
    options.UseSqlite(resolvedConnection));

// Services
builder.Services.AddScoped<PortfolioRepository>();
builder.Services.AddScoped<PerformanceRepository>();
builder.Services.AddHttpClient("coingecko", client =>
{
    client.BaseAddress = new Uri("https://api.coingecko.com");
    client.DefaultRequestHeaders.Add("User-Agent", "CryptoAgentPOC/1.0");
});
builder.Services.AddSingleton<MarketDataService>();
builder.Services.AddSingleton<RiskEngine>();
builder.Services.AddSingleton<AgentService>();

if (appConfig.Mode == AgentMode.Paper)
{
    builder.Services.AddScoped<IExchangeClient, PaperExchangeClient>();
}
else
{
    builder.Services.AddScoped<IExchangeClient, CryptoComExchangeClient>();
}

builder.Services.AddHostedService<AgentWorker>();

// OpenAI
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];
    var model = config["OpenAI:Model"] ?? "gpt-4o";
    return new OpenAI.Chat.ChatClient(model, new System.ClientModel.ApiKeyCredential(apiKey));
});

var app = builder.Build();

// Ensure database is created and up to date
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CryptoAgentDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors();

// Endpoints

app.MapGet("/api/dashboard", async (PortfolioRepository portfolioRepository, MarketDataService marketDataService, AgentService agentService) =>
{
    var portfolio = await portfolioRepository.GetAsync();
    var market = await marketDataService.GetSnapshotAsync();
    var recentTrades = await portfolioRepository.GetRecentTradesAsync(20);

    var response = new DashboardResponse
    {
        Portfolio = portfolio.ToDto(market),
        Market = market,
        LastDecision = agentService.LastDecision,
        RecentTrades = recentTrades
    };

    return Results.Ok(response);
})
.WithName("GetDashboard");
//.WithOpenApi();

app.MapPost("/api/agent/run-once", async (AgentService agentService, PortfolioRepository portfolioRepository, MarketDataService marketDataService) =>
{
    await agentService.RunOnceAsync();

    // Return updated dashboard
    var portfolio = await portfolioRepository.GetAsync();
    var market = await marketDataService.GetSnapshotAsync();
    var recentTrades = await portfolioRepository.GetRecentTradesAsync(20);

    var response = new DashboardResponse
    {
        Portfolio = portfolio.ToDto(market),
        Market = market,
        LastDecision = agentService.LastDecision,
        RecentTrades = recentTrades
    };

    return Results.Ok(response);
})
.WithName("RunAgent");
//.WithOpenApi();

app.MapGet("/api/performance/monthly", async (PerformanceRepository performanceRepository) =>
{
    var all = await performanceRepository.GetAllAsync();

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

    return Results.Ok(groups);
})
.WithName("GetMonthlyPerformance");
//.WithOpenApi();

app.Run();
