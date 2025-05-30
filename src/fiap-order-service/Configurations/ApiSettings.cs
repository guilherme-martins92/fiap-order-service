using System.Diagnostics.CodeAnalysis;

namespace fiap_order_service.Configurations
{
    [ExcludeFromCodeCoverage]
    public class ApiSettings
    {
        public required string CatalogApiUrl { get; set; }
    }
}
