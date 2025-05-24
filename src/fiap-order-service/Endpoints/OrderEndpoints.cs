using fiap_order_service.Dtos;
using fiap_order_service.Models;
using fiap_order_service.Services;
using FluentValidation;

namespace fiap_order_service.Endpoints
{
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
                _logger.LogInformation("Buscando todas as compras");
                var orders = await _orderService.GetAllOrdersAsync();
                return Results.Ok(orders);
            })
            .WithName("GetAllOrders")
            .WithOpenApi();

            app.MapGet("/orders/{id}", async (Guid id) =>
            {
                _logger.LogInformation("Buscando compra com ID: {Id}", id);
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(order);
            })
            .WithName("GetOrderById")
            .WithOpenApi();

            app.MapPost("/orders", async (OrderDto order, IValidator<OrderDto> validator) =>
            {
                try
                {
                    _logger.LogInformation("Criando nova compra");

                    var result = await validator.ValidateAsync(order);
                    if (!result.IsValid)
                    {
                        _logger.LogWarning("Erro de validação: {@Errors}", result.Errors);
                        return Results.ValidationProblem(result.ToDictionary());
                    }

                    var createdOrder = await _orderService.CreateOrderAsync(order);
                    return Results.Created($"/orders/{createdOrder.Id}", createdOrder);
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogError(ex, "Veículo não encontrado");
                    return Results.NotFound(ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Erro ao criar compra");
                    return Results.Problem(title: "Erro ao criar compra", detail: ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar veículos");
                    return Results.Problem(title: "Erro interno");
                }
            })
            .WithName("CreateOrder")
            .WithOpenApi();

            app.MapPut("/orders/{id}", async (Guid id, string status) =>
            {
                _logger.LogInformation("Atualizando compra com ID: {Id}", id);
                var updatedOrder = await _orderService.UpdateStatusOrderAsync(id, status);
                if (updatedOrder == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(updatedOrder);
            })
            .WithName("UpdateOrder")
            .WithOpenApi();
        }
    }
}
