using Microsoft.EntityFrameworkCore;
using MarketPulse.MarketDataStorage.Worker;
using MarketPulse.MarketDataStorage.Worker.Configuration;
using MarketPulse.MarketDataStorage.Worker.Consumers;
using MarketPulse.MarketDataStorage.Worker.Persistence;
using MarketPulse.MarketDataStorage.Worker.Processing;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<KafkaOptions>()
    .Bind(
        builder.Configuration.GetSection(
            KafkaOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.BootstrapServers),
        "Kafka BootstrapServers must be configured.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.GroupId),
        "Kafka GroupId must be configured.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.MarketPriceTopic),
        "Kafka MarketPriceTopic must be configured.")
    .ValidateOnStart();

var connectionString =
    builder.Configuration.GetConnectionString("MarketData")
    ?? throw new InvalidOperationException(
        "Connection string 'MarketData' was not configured.");

builder.Services.AddDbContext<MarketDataDbContext>(
    options => options.UseNpgsql(connectionString));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<KafkaMarketPriceConsumer>();

builder.Services.AddScoped<
    IMarketPriceProcessor,
    MarketPriceProcessor>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();
