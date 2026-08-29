using YaeaY.Account.Infrastructure;
using YaeaY.Account.EventProcessing.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddReadModelWorkerInfrastructure(builder.Configuration);
builder.Services.AddHostedService<YaeaY.Account.Infrastructure.Messaging.RabbitMq.RabbitMqEventConsumer>();
builder.Services.AddHostedService<ReadModelRebuildHostedService>();

var host = builder.Build();
await host.RunAsync();
