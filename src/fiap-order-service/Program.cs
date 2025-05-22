using fiap_order_service.Endpoints;
using fiap_order_service.Infrastructure.HttpClients;
using fiap_order_service.Repositories;
using fiap_order_service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderEndpoints>>();

    var orderEndpoints = new OrderEndpoints(logger, orderService);
    orderEndpoints.MapEndpoints(app);
}

await app.RunAsync();


