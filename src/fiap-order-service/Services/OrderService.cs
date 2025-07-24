using fiap_order_service.Constants;
using fiap_order_service.Dtos;
using fiap_order_service.Infrastructure.EventBridge;
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
        private readonly IEventPublisher _eventBridgePublisher;
        private readonly ICustomerService _customerService;

        public OrderService(IOrderRepository orderRepository, ICatalogService catalogService, ILogger<OrderService> logger, ISqsClientService sqsClientService, IEventPublisher eventBridgePublisher, ICustomerService customerService)
        {
            _orderRepository = orderRepository;
            _catalogService = catalogService;
            _logger = logger;
            _sqsClientService = sqsClientService;
            _eventBridgePublisher = eventBridgePublisher;
            _customerService = customerService;
        }

        public async Task<Order> CreateOrderAsync(OrderDto orderDto)
        {
            try
            {
                _logger.LogInformation("Criando pedido com os dados: {@OrderDto}", orderDto);
                if (orderDto == null)
                    throw new ArgumentNullException(nameof(orderDto), "O pedido não pode ser nulo.");

                if (orderDto.Item == null)
                    throw new ArgumentNullException(nameof(orderDto), "O item do pedido não pode ser nulo.");

                var customer = await _customerService.GetCustomerByIdAsync(orderDto.Customer.Id);

                if (customer == null)
                {
                    _logger.LogInformation("Cliente não encontrado.");
                    throw new KeyNotFoundException("Cliente não encontrado.");
                }

                var vehicle = await _catalogService.GetVehicleByIdAsync(orderDto.Item.VehicleExternalId);
                if (vehicle == null)
                {
                    _logger.LogInformation("Veículo com ID {VehicleId} não encontrado.", orderDto.Item.VehicleExternalId);
                    throw new KeyNotFoundException("Veículo não encontrado.");
                }
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    Customer = customer,
                    Item = new ItemOrder
                    {
                        VehicleId = vehicle.Id,
                        Model = vehicle.Model,
                        Brand = vehicle.Brand,
                        Color = vehicle.Color,
                        Amount = orderDto.Item.Amount,
                        UnitPrice = vehicle.Price,
                        TotalPrice = vehicle.Price * orderDto.Item.Amount,
                        Year = vehicle.Year
                    },
                    TotalPrice = vehicle.Price * orderDto.Item.Amount,
                    Status = OrderStatus.Created,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                var createdOrder = await _orderRepository.CreateOrderAsync(order);

                if (createdOrder == null)
                    throw new InvalidOperationException("Falha ao criar o pedido");

                await _eventBridgePublisher.PublicarCompraRealizadaAsync(createdOrder.Id, createdOrder.Item!.VehicleId);

                _logger.LogInformation("Pedido criado com sucesso: {@Order}", createdOrder);

                return createdOrder;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Veículo não encontrado: {Message}", ex.Message);
                throw new KeyNotFoundException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido: {Message}", ex.Message);
                throw new InvalidOperationException();
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            if (orders == null || !orders.Any())
            {
                _logger.LogInformation("Nenhum pedido encontrado.");
                return new List<Order>();
            }
            return orders.OrderBy(x => x.TotalPrice).ToList();
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                _logger.LogInformation("Pedido com ID {Id} não encontrado.", id);
                return null;
            }

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

            if (status == OrderStatus.Canceled)
                await _eventBridgePublisher.PublicarCompraCanceladaAsync(updatedOrder.Id, updatedOrder.Item.VehicleId);

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
                    CustomerEmail = order.Customer.Email,
                    Amount = order.TotalPrice,
                    Description = $"Pedido {order.Id} - {order.Customer.FirstName} - {order.TotalPrice:C}"
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