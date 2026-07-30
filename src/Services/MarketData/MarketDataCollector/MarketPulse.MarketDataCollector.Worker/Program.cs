using MarketPulse.MarketDataCollector.Worker;
using MarketPulse.MarketDataCollector.Worker.Configuration;
using MarketPulse.MarketDataCollector.Worker.Events;
using MarketPulse.MarketDataCollector.Worker.MarketData;
using MarketPulse.MarketDataCollector.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<KafkaOptions>()
    .Bind(builder.Configuration.GetSection(KafkaOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.BootstrapServers),
        "Kafka BootstrapServers must be configured.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ClientId),
        "Kafka ClientId must be configured.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.MarketPriceTopic),
        "Kafka MarketPriceTopic must be configured.")
    .ValidateOnStart();

builder.Services
    .AddOptions<EventOptions>()
    .Bind(builder.Configuration.GetSection(EventOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Producer),
        "Event producer must be configured.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddSingleton<IPriceGenerator, FakePriceGenerator>();
builder.Services.AddSingleton<IEventFactory, EventFactory>();
builder.Services.AddSingleton<IEventProducer, KafkaEventProducer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await host.RunAsync();