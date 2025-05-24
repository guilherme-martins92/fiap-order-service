using fiap_order_service.Dtos;
using fiap_order_service.Models;

namespace fiap_order_service.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<Order> CreateOrderAsync(OrderDto orderDto);
        Task<Order?> UpdateStatusOrderAsync(Guid id, string status);
    }
}
