using fiap_order_service.Models;

namespace fiap_order_service.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public async Task<Order> CreateOrderAsync(Order order)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Order?> UpdateOrderAsync(int id, Order order)
        {
            throw new NotImplementedException();
        }
    }
}
