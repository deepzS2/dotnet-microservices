using Play.Common.MongoDB;
using Play.Common.MassTransit;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo().AddMongoRepository<InventoryItem>("inventoryitems").AddMongoRepository<CatalogItem>("catalogitems").AddMassTransitWithRabbitMQ();

AddCatalogCient(builder);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogCient(WebApplicationBuilder builder)
{
  builder.Services.AddHttpClient<CatalogClient>(client =>
  {
    client.BaseAddress = new Uri("https://localhost:7083");
  })
  .AddTransientHttpErrorPolicy(httpBuilder => httpBuilder.Or<TimeoutRejectedException>().WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), onRetry: (outcome, timespan, retryAttempt) =>
  {
    var serviceProvider = builder.Services.BuildServiceProvider();
    serviceProvider.GetService<ILogger<CatalogClient>>()?.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
  }))
  .AddTransientHttpErrorPolicy(httpBuilder => httpBuilder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
    3,
    TimeSpan.FromSeconds(15),
    onBreak: (outcome, timespan) =>
    {
      var serviceProvider = builder.Services.BuildServiceProvider();
      serviceProvider.GetService<ILogger<CatalogClient>>()?.LogWarning($"Opening the circuit for {timespan.TotalSeconds}");
    },
    onReset: () =>
    {
      var serviceProvider = builder.Services.BuildServiceProvider();
      serviceProvider.GetService<ILogger<CatalogClient>>()?.LogWarning($"Closing the circuit...");
    }
  ))
  .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}