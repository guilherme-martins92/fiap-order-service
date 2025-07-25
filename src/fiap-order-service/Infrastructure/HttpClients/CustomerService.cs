using fiap_order_service.Models;
using System.Diagnostics.CodeAnalysis;

namespace fiap_order_service.Infrastructure.HttpClients
{
    [ExcludeFromCodeCoverage]
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _customerApiUrl;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IConfiguration configuration, ILogger<CustomerService> logger)
        {
            _httpClient = new HttpClient();
            _customerApiUrl = "https://hf8pq28mw9.execute-api.us-east-1.amazonaws.com/";
            _logger = logger;
        }

        public async Task<Customer?> GetCustomerByIdAsync(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Chamando API de clientes em: {Url}", _customerApiUrl);

                var response = await _httpClient.GetAsync($"{_customerApiUrl}/customers/{customerId}");
                if (response.IsSuccessStatusCode)
                {
                    var customer = await response.Content.ReadFromJsonAsync<Customer>();
                    return customer;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch customer from customer service. Status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching customer from customer service: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching customer from customer service: {Message}", ex.Message);
            }
            return null;
        }
    }
}