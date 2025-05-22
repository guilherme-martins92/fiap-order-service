using fiap_order_service.Dtos;
using fiap_order_service.Infrastructure.HttpClients;
using fiap_order_service.Models;
using fiap_order_service.Repositories;

namespace fiap_order_service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICatalogService _catalogService;

        public OrderService(IOrderRepository orderRepository, ICatalogService catalogService)
        {
            _orderRepository = orderRepository;
            _catalogService = catalogService;
        }

        public async Task<Order> CreateOrderAsync(OrderDto orderDto)
        {
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto), "Order cannot be null");

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                CustomerDocument = orderDto.CustomerDocument,
                CustomerName = orderDto.CustomerName,
                CustomerEmail = orderDto.CustomerEmail,
                Status = Order.OrderStatus.Created,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            foreach (var item in orderDto.Itens)
            {
                var vehicle = await _catalogService.GetVehicleByIdAsync(item.VehicleExternalId);

                if (vehicle == null)
                    throw new KeyNotFoundException($"Vehicle with ID {item.VehicleExternalId} not found");

                order.Itens.Add(new ItemOrder
                {
                    VehicleId = vehicle.ExternalId,
                    UnitPrice = vehicle.Price,
                    Amount = item.Amount,
                    TotalPrice = vehicle.Price * item.Amount,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            if (createdOrder == null)
                throw new InvalidOperationException("Failed to create order");

            return createdOrder;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            // Call the repository to get all orders
            var orders = await _orderRepository.GetAllOrdersAsync();
            // Check if the list is null or empty
            if (orders == null || !orders.Any())
            {
                throw new InvalidOperationException("No orders found");
            }
            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            // Validate the order ID
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Order ID must be greater than zero");
            }
            // Call the repository to get the order by ID
            var order = await _orderRepository.GetOrderByIdAsync(id);
            // Check if the order is null
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {id} not found");
            }
            return order;
        }

        public async Task<Order?> UpdateOrderAsync(int id, Order order)
        {
            throw new NotImplementedException();
        }
    }
}
