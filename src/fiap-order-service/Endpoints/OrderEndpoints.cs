using fiap_order_service.Dtos;
using fiap_order_service.Services;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace fiap_order_service.Endpoints
{
    [ExcludeFromCodeCoverage]
    public class OrderEndpoints
    {
        private readonly ILogger<OrderEndpoints> _logger;
        private readonly IOrderService _orderService;

        public OrderEndpoints(ILogger<OrderEndpoints> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        public void MapEndpoints(WebApplication app)
        {
            app.MapGet("/orders", async () =>
            {
                var orders = await _orderService.GetAllOrdersAsync();

                if (orders == null || !orders.Any())
                {
                    return Results.NotFound("Nenhum pedido encontrado.");
                }

                return Results.Ok(orders);
            })
            .WithName("GetAllOrders")
            .WithOpenApi();

            app.MapGet("/orders/{id}", async (Guid id) =>
            {
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                    return Results.NotFound("Pedido não encontrado.");

                return Results.Ok(order);
            })
            .WithName("GetOrderById")
            .WithOpenApi();

            app.MapPost("/orders", async (OrderDto order, IValidator<OrderDto> validator) =>
            {
                try
                {
                    var result = await validator.ValidateAsync(order);
                    if (!result.IsValid)
                    {
                        _logger.LogWarning("Erro de validação: {@Errors}", result.Errors);
                        return Results.ValidationProblem(result.ToDictionary());
                    }

                    var createdOrder = await _orderService.CreateOrderAsync(order);
                    return Results.Created($"/orders/{createdOrder.Id}", createdOrder);
                }
                catch (KeyNotFoundException)
                {
                    return Results.NotFound("Veículo não encontrado.");
                }
                catch (Exception)
                {
                    return Results.Problem(title: "Ocorreu um erro interno.");
                }
            })
            .WithName("CreateOrder")
            .WithOpenApi();

            app.MapPut("/orders/{id}", async (Guid id, string status) =>
            {
                _logger.LogInformation("Atualizando compra com ID: {Id}", id);
                var updatedOrder = await _orderService.UpdateStatusOrderAsync(id, status);

                if (updatedOrder == null)
                    return Results.NotFound();

                return Results.Ok(updatedOrder);
            })
            .WithName("UpdateOrder")
            .WithOpenApi();
        }
    }
}
