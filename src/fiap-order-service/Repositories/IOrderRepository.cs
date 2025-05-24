using fiap_order_service.Models;

namespace fiap_order_service.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(Guid id);
        Task<Order?> CreateOrderAsync(Order order);
        Task<Order?> UpdateStatusOrderAsync(Guid id, Order order);
    }
}
