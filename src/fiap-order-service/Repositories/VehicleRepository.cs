using fiap_order_service.Models;

namespace fiap_order_service.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        public Task<Vehicle?> GetVehicleByExternalIdAsync(Guid externalId)
        {
            throw new NotImplementedException();
        }
    }
}
