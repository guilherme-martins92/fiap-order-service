﻿using fiap_order_service.Models;
using System.Diagnostics.CodeAnalysis;

namespace fiap_order_service.Infrastructure.HttpClients
{
    [ExcludeFromCodeCoverage]
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly string _catalogApiUrl;
        private readonly ILogger<CatalogService> _logger;

        public CatalogService(IConfiguration configuration, ILogger<CatalogService> logger)
        {
            _httpClient = new HttpClient();
            _catalogApiUrl = Environment.GetEnvironmentVariable("CatalogApiUrl") ?? configuration.GetSection("ApiSettings")["CatalogApiUrl"];
            _logger = logger;
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Chamando API de catálogo em: {Url}", _catalogApiUrl);

                var response = await _httpClient.GetAsync($"{_catalogApiUrl}/vehicles/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var vehicle = await response.Content.ReadFromJsonAsync<Vehicle>();
                    return vehicle;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch vehicle from catalog service. Status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching vehicle from catalog service: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching vehicle from catalog service: {Message}", ex.Message);
            }
            return null;
        }
    }
}
