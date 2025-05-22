using fiap_order_service.Models;

namespace fiap_order_service.Repositories
{
    public interface IVehicleRepository
    {
        Task<Vehicle?> GetVehicleByExternalIdAsync(Guid externalId);
    }
}
