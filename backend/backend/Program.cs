using CryptoAgent.Api.Models;
using CryptoAgent.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Config
builder.Services.AddSingleton(new RiskConfig());
builder.Services.AddSingleton(new AppConfig());

// Services
builder.Services.AddSingleton<PortfolioStore>();
builder.Services.AddHttpClient("coingecko", client =>
{
    client.BaseAddress = new Uri("https://api.coingecko.com");
    client.DefaultRequestHeaders.Add("User-Agent", "CryptoAgentPOC/1.0");
});
builder.Services.AddSingleton<MarketDataService>();
builder.Services.AddSingleton<PerformanceStore>();
builder.Services.AddSingleton<RiskEngine>();
builder.Services.AddSingleton<AgentService>();

// OpenAI
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];
    var model = config["OpenAI:Model"] ?? "gpt-4o";
    return new OpenAI.Chat.ChatClient(model, new System.ClientModel.ApiKeyCredential(apiKey));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors();

// Endpoints

app.MapGet("/api/dashboard", async (PortfolioStore portfolioStore, MarketDataService marketDataService, AgentService agentService) =>
{
    var portfolio = await portfolioStore.GetAsync();
    var market = await marketDataService.GetSnapshotAsync();
    var recentTrades = await portfolioStore.GetRecentTradesAsync(20);

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

app.MapPost("/api/agent/run-once", async (AgentService agentService, PortfolioStore portfolioStore, MarketDataService marketDataService) =>
{
    await agentService.RunOnceAsync();

    // Return updated dashboard
    var portfolio = await portfolioStore.GetAsync();
    var market = await marketDataService.GetSnapshotAsync();
    var recentTrades = await portfolioStore.GetRecentTradesAsync(20);

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

app.MapGet("/api/performance/monthly", async (PerformanceStore performanceStore) =>
{
    var all = await performanceStore.GetAllAsync();
    
    var groups = all.GroupBy(x => new { x.DateUtc.Year, x.DateUtc.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => 
                    {
                        var first = g.First();
                        var last = g.Last();
                        var pnl = last.PortfolioValueGbp - first.PortfolioValueGbp;
                        var aiCost = last.CumulatedAiCostGbp - first.CumulatedAiCostGbp;
                        
                        return new 
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            StartValue = first.PortfolioValueGbp,
                            EndValue = last.PortfolioValueGbp,
                            PnlGbp = pnl,
                            AiCostGbp = aiCost,
                            NetAfterAiGbp = pnl - aiCost,
                            VaultEndGbp = last.VaultGbp
                        };
                    })
                    .ToList();

    return Results.Ok(groups);
})
.WithName("GetMonthlyPerformance");
//.WithOpenApi();

app.Run();
