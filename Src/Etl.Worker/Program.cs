using Etl.Core;
using Etl.Infrastructure;
using Etl.Infrastructure.Bcb;
using Etl.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<BcbOptions>(builder.Configuration.GetSection("Bcb"));

builder.Services
    .AddHttpClient<IExtractor<BcbRawPoint>, BcbSgsExtractor>(c => c.BaseAddress = new Uri("https://api.bcb.gov.br/"))
    .AddStandardResilienceHandler(); // retries, circuit breaker, timeouts (Polly under the hood)

builder.Services.AddSingleton<ITransformer<BcbRawPoint, EconomicIndicator>, BcbTransformer>();
builder.Services.AddSingleton<ILoader<EconomicIndicator>, LoggingLoader<EconomicIndicator>>();
builder.Services.AddTransient<PipelineRunner<BcbRawPoint, EconomicIndicator>>();
builder.Services.AddHostedService<EtlWorker>();

await builder.Build().RunAsync();
