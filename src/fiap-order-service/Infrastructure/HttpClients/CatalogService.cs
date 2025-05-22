using fiap_order_service.Models;

namespace fiap_order_service.Infrastructure.HttpClients
{
    public class CatalogService : ICatalogService
    {
        private readonly List<Vehicle> _vehicles = new()
        {
            new Vehicle { ExternalId = Guid.Parse("6f2b0b8d-33f5-4ea0-8e2f-f03b27e4a731"), Model = "Model S", Brand = "Tesla", Year = 2020, Color = "Red", Price = 20000},
            new Vehicle { ExternalId = Guid.Parse("a63f0975-dcbe-44b5-b813-91ed144ba4f5"), Model = "Mustang", Brand = "Ford", Year = 2021, Color = "Blue", Price = 30000},
            new Vehicle { ExternalId = Guid.Parse("82d9c3e8-75bd-4a44-9df1-61b2a78653c4"), Model = "Civic", Brand = "Honda", Year = 2019, Color = "Black", Price = 25000 }
        };
        public Task<Vehicle?> GetVehicleByIdAsync(Guid id)
        {
            var vehicle = _vehicles.FirstOrDefault(v => v.ExternalId == id);
            return Task.FromResult(vehicle);
        }
    }
}
