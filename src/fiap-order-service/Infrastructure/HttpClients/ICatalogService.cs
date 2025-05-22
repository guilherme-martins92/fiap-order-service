using fiap_order_service.Models;

namespace fiap_order_service.Infrastructure.HttpClients
{
    public interface ICatalogService
    {
        Task<Vehicle?> GetVehicleByIdAsync(Guid id);
    }
}
