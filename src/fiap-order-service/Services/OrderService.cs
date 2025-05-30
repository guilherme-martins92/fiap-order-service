using fiap_order_service.Constants;
using fiap_order_service.Dtos;
using fiap_order_service.Infrastructure.HttpClients;
using fiap_order_service.Messaging;
using fiap_order_service.Models;
using fiap_order_service.Repositories;

namespace fiap_order_service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICatalogService _catalogService;
        private readonly ILogger<OrderService> _logger;
        private readonly ISqsClientService _sqsClientService;

        public OrderService(IOrderRepository orderRepository, ICatalogService catalogService, ILogger<OrderService> logger, ISqsClientService sqsClientService)
        {
            _orderRepository = orderRepository;
            _catalogService = catalogService;
            _logger = logger;
            _sqsClientService = sqsClientService;
        }

        public async Task<Order> CreateOrderAsync(OrderDto orderDto)
        {
            try
            {
                _logger.LogInformation("Criando pedido com os dados: {@OrderDto}", orderDto);

                if (orderDto == null)
                    throw new ArgumentNullException(nameof(orderDto), "O pedido não pode ser null.");

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    CustomerDocument = orderDto.CustomerDocument,
                    CustomerName = orderDto.CustomerName,
                    CustomerEmail = orderDto.CustomerEmail,
                    Status = OrderStatus.Created,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                foreach (var item in orderDto.Itens)
                {
                    var vehicle = await _catalogService.GetVehicleByIdAsync(item.VehicleExternalId);

                    if (vehicle == null)
                        throw new KeyNotFoundException($"Veiculo com ID {item.VehicleExternalId} não encontrado.");

                    order.Itens.Add(new ItemOrder
                    {
                        VehicleId = vehicle.Id,
                        UnitPrice = vehicle.Price,
                        Amount = item.Amount,
                        TotalPrice = vehicle.Price * item.Amount,
                        Model = vehicle.Model,
                        Brand = vehicle.Brand,
                        Color = vehicle.Color,
                        Year = vehicle.Year
                    });
                }

                order.TotalPrice = order.Itens.Sum(i => i.TotalPrice);

                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                if (createdOrder == null)
                    throw new InvalidOperationException("Falha ao criar o pedido");

                await SendOrderToPaymentQueue(createdOrder);

                _logger.LogInformation("Pedido criado com sucesso: {@Order}", createdOrder);

                return createdOrder;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Veículo não encontrado: {Message}", ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido: {Message}", ex.Message);
                throw new InvalidOperationException("Erro ao criar o pedido", ex);
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();

            if (orders == null || !orders.Any())
                throw new InvalidOperationException("Nenhum pedido encontrado");

            return orders.OrderBy(x => x.TotalPrice).ToList();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id), "O ID do pedido não pode ser vazio.");

            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
                throw new KeyNotFoundException($"Pedido com ID {id} não encontrado");

            return order;
        }

        public async Task<Order?> UpdateStatusOrderAsync(Guid id, string status)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id), "O ID do pedido não pode ser vazio.");
            if (string.IsNullOrEmpty(status))
                throw new ArgumentNullException(nameof(status), "O pedido não pode ser null.");

            var existingOrder = await _orderRepository.GetOrderByIdAsync(id);

            if (existingOrder == null)
                throw new KeyNotFoundException($"Pedido com ID {id} não encontrado");

            existingOrder.Status = status;
            existingOrder.UpdatedDate = DateTime.UtcNow;

            var updatedOrder = await _orderRepository.UpdateStatusOrderAsync(id, existingOrder);

            if (updatedOrder == null)
                throw new InvalidOperationException("Falha ao atualizar o pedido");

            return updatedOrder;
        }

        public async Task SendOrderToPaymentQueue(Order order)
        {
            try
            {
                _logger.LogInformation("Enviando pedido para a fila de pagamento: {@Order}", order);

                order.Status = OrderStatus.PendingPayment;
                await _orderRepository.UpdateStatusOrderAsync(order.Id, order);

                var paymentPayLoad = new PaymentPayLoad
                {
                    OrderId = order.Id,
                    PaymentMethod = PaymentMethod.CreditCard,
                    CustomerEmail = order.CustomerEmail,
                    Amount = order.TotalPrice,
                    Description = $"Pedido {order.Id} - {order.CustomerName} - {order.TotalPrice:C}"
                };

                await _sqsClientService.SendMessageAsync(paymentPayLoad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar pedido para a fila de pagamento: {Message}", ex.Message);
                throw new InvalidOperationException("Erro ao enviar o pedido para a fila de pagamento", ex);
            }
        }
    }
}