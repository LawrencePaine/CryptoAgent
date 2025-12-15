using CryptoAgent.Api.Data;
using CryptoAgent.Api.Models;
using CryptoAgent.Api.Repositories;
using CryptoAgent.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: @"C:\CryptoAgentData\logs\cryptoagent-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true)
    .CreateLogger();

// Config
var riskConfig = builder.Configuration.GetSection("RiskConfig").Get<RiskConfig>() ?? new RiskConfig();
var appConfig = builder.Configuration.GetSection("AppConfig").Get<AppConfig>() ?? new AppConfig();
var feeConfig = builder.Configuration.GetSection("FeeConfig").Get<FeeConfig>() ?? new FeeConfig();
var regimeConfig = builder.Configuration.GetSection("RegimeConfig").Get<RegimeConfig>() ?? new RegimeConfig();
var workerConfig = builder.Configuration.GetSection("Worker").Get<WorkerConfig>() ?? new WorkerConfig();

builder.Host.UseSerilog();

builder.Services.AddSingleton(riskConfig);
builder.Services.AddSingleton(appConfig);
builder.Services.AddSingleton(feeConfig);
builder.Services.AddSingleton(regimeConfig);
builder.Services.AddSingleton(workerConfig);

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

// Anchor the SQLite file to the compiled output folder so both `dotnet ef` design-time
// tools and the running application write to the exact same database file regardless of
// the working directory used to launch the process.
//const string dataSourcePrefix = "Data Source=";
//var resolvedConnection = configuredConnection;
//if (configuredConnection.StartsWith(dataSourcePrefix, StringComparison.OrdinalIgnoreCase))
//{
//    var dataSource = configuredConnection[dataSourcePrefix.Length..].Trim();
//    if (!Path.IsPathRooted(dataSource))
//    {
//        var absolutePath = Path.Combine(AppContext.BaseDirectory, dataSource);
//        resolvedConnection = $"{dataSourcePrefix}{absolutePath}";
//    }
//}

//builder.Services.AddDbContext<CryptoAgentDbContext>(options =>
//    options.UseSqlite(resolvedConnection));
builder.Services.AddDbContext<CryptoAgentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CryptoAgentDb")));

// Services
builder.Services.AddScoped<PortfolioRepository>();
builder.Services.AddScoped<PerformanceRepository>();
builder.Services.AddScoped<DecisionRepository>();
builder.Services.AddScoped<HourlyCandleRepository>();
builder.Services.AddScoped<HourlyFeatureRepository>();
builder.Services.AddScoped<RegimeStateRepository>();
builder.Services.AddScoped<StrategySignalRepository>();
builder.Services.AddScoped<LlmStateBuilder>();
builder.Services.AddHttpClient("coingecko", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["CoinGecko:ApiKey"];

    client.BaseAddress = new Uri("https://api.coingecko.com");
    client.DefaultRequestHeaders.Add("User-Agent", "CryptoAgentPOC/1.0");

    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("x-cg-demo-api-key", apiKey);
    }
});
builder.Services.AddSingleton<MarketDataService>();
builder.Services.AddSingleton<RiskEngine>();
builder.Services.AddSingleton<AgentService>();
builder.Services.AddSingleton<HourlyFeatureCalculator>();
builder.Services.AddSingleton<RegimeClassifier>();
builder.Services.AddSingleton<IStrategyModule, CryptoAgent.Api.Services.Strategies.DcaAccumulateStrategy>();
builder.Services.AddSingleton<IStrategyModule, CryptoAgent.Api.Services.Strategies.MeanReversionStrategy>();
builder.Services.AddSingleton<IStrategyModule, CryptoAgent.Api.Services.Strategies.RiskOffTrimStrategy>();

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

app.MapControllers();
app.Lifetime.ApplicationStarted.Register(() =>
{
    var server = app.Services.GetRequiredService<IServer>();
    var addressFeature = server.Features.Get<IServerAddressesFeature>();

    if (addressFeature != null)
    {
        foreach (var address in addressFeature.Addresses)
        {
            Log.Information("CryptoAgent running on: {Address}", address);
        }
    }
});

Log.Information("CryptoAgent starting up...");
app.Run();
