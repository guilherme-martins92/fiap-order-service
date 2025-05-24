using Amazon.DynamoDBv2.DataModel;
using fiap_order_service.Models;

namespace fiap_order_service.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDynamoDBContext _context;

        public OrderRepository(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<Order?> CreateOrderAsync(Order order)
        {
            await _context.SaveAsync(order);
            return order;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = await _context.ScanAsync<Order>(null).GetRemainingAsync();
            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(Guid id)
        {
            var order = await _context.LoadAsync<Order>(id);
            return order;
        }

        public async Task<Order?> UpdateStatusOrderAsync(Guid id, Order order)
        {
            var existingOrder = await _context.LoadAsync<Order>(id);
            if (existingOrder == null)
            {
                return null;
            }
            existingOrder.Status = order.Status;
            existingOrder.UpdatedDate = DateTime.UtcNow;
            await _context.SaveAsync(existingOrder);
            return existingOrder;
        }
    }
}
