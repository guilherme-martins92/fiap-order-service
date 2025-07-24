using fiap_order_service.Models;

namespace fiap_order_service.Infrastructure.HttpClients
{
    public interface ICustomerService
    {
        Task<Customer?> GetCustomerByIdAsync(Guid customerId);
    }
}
