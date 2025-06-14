using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using fiap_payment_processor.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace fiap_payment_processor;

[ExcludeFromCodeCoverage]
public class Function
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly string _baseAddres = Environment.GetEnvironmentVariable("ORDER_API_URL") ?? "http://localhost:5000/api";

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var record in sqsEvent.Records)
        {
            try
            {
                var paymentRequest = JsonSerializer.Deserialize<PaymentRequest>(record.Body);
                if (paymentRequest == null)
                {
                    context.Logger.LogLine("Invalid payment request received.");
                    continue;
                }

                var response = await ProcessPaymentAsync(paymentRequest);

                context.Logger.LogInformation($"Pagamento processado: {response}");
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error processing record: {ex.Message}");
                throw;
            }
        }
    }

    public async Task<string> ProcessPaymentAsync(PaymentRequest paymentRequest)
    {
        string teste = string.Empty;
        try
        {
            teste = await UpdatePaymentStatusAsync(paymentRequest.OrderId, "PAGO");
            return "Pagamento processado com sucesso!" + teste;
        }
        catch (Exception ex)
        {
            return $"Erro ao processar pagamento: {ex.Message}" + teste;
        }
    }

    public async Task<string> UpdatePaymentStatusAsync(Guid orderId, string status)
    {
        string endpoint = _baseAddres + $"/orders/{orderId}?status={status}";

        var response = await _httpClient.PutAsync(endpoint, null);

        if (response.IsSuccessStatusCode)
        {
            return "Status de pagamento atualizado com sucesso.";
        }
        else
        {
            return $"Falha ao tentar atualizar o status de pagamento: {response.ReasonPhrase}";
        }
    }
}