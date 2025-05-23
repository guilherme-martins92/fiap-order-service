using fiap_order_service.Models;

namespace fiap_order_service.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new List<Order>();
        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "O pedido não pode ser null.");
            order.OrderId = Guid.NewGuid();
            order.CreatedDate = DateTime.UtcNow;
            order.UpdatedDate = DateTime.UtcNow;
            _orders.Add(order);
            return await Task.FromResult(order);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            if (_orders.Count == 0)
                throw new KeyNotFoundException("Nenhum pedido encontrado.");

            return _orders;
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            var order = _orders.FirstOrDefault(o => o.OrderId == id);

            return await Task.FromResult(order);
        }

        public async Task<Order?> UpdateStatusOrderAsync(Guid id, Order order)
        {
            var existingOrder = _orders.FirstOrDefault(o => o.OrderId == id);
            return await Task.FromResult(existingOrder);
        }
    }
}
