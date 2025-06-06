using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using fiap_payment_processor.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace fiap_payment_processor;
public class Function
{
    private static HttpClient _httpClient = new HttpClient();

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
            }
        }
    }

    public async Task<string> ProcessPaymentAsync(PaymentRequest paymentRequest)
    {
        await UpdatePaymentStatusAsync(paymentRequest.OrderId, "PAGO");
        return "Pagamento processado com sucesso!";
    }

    public async Task<string> UpdatePaymentStatusAsync(string orderId, string status)
    {
        var updateContent = new StringContent(JsonSerializer.Serialize(new { OrderId = orderId, Status = status }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"https://3usbkhj94a.execute-api.us-east-1.amazonaws.com/orders/{orderId}/status", updateContent);
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