using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.EventBridge;
using Amazon.SQS;
using fiap_order_service.Configurations;
using fiap_order_service.Endpoints;
using fiap_order_service.Infrastructure.EventBridge;
using fiap_order_service.Infrastructure.HttpClients;
using fiap_order_service.Messaging;
using fiap_order_service.Repositories;
using fiap_order_service.Services;
using fiap_order_service.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<ISqsClientService, SqsClientService>();
builder.Services.AddScoped<IEventPublisher, EventBridgePublisher>();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddValidatorsFromAssemblyContaining<OrderDtoValidator>();
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

// Add AWS Lambda support. When running the application as an AWS Serverless application, Kestrel is replaced
// with a Lambda function contained in the Amazon.Lambda.AspNetCoreServer package, which marshals the request into the ASP.NET Core hosting framework.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    return new AmazonDynamoDBClient();
});

builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    return new AmazonSQSClient();
});

builder.Services.AddSingleton<IAmazonEventBridge>(sp =>
{
    return new AmazonEventBridgeClient();
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