// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Models;
using System.Text;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace fiap_order_payment_consumer;
public class Function
{
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        foreach (var record in sqsEvent.Records)
        {
            try
            {
                context.Logger.LogInformation($"Processing message ID: {record.MessageId}");

                var messageBody = record.Body;

                // Extrai apenas o campo "detail" do envelope JSON
                var envelope = JsonDocument.Parse(messageBody);
                if (!envelope.RootElement.TryGetProperty("detail", out var detailElement))
                {
                    context.Logger.LogError($"Message ID: {record.MessageId} is missing 'detail' property.");
                    continue;
                }

                var detailJson = detailElement.GetRawText();
                
                var orderUpdateStatusDto = JsonSerializer.Deserialize<UpdateOrderDto>(detailJson);

                if (orderUpdateStatusDto == null)
                {
                    context.Logger.LogError($"Failed to deserialize 'detail' for message ID: {record.MessageId}");
                    continue;
                }

                var requestUri = $"https://rld8zb3bja.execute-api.us-east-1.amazonaws.com/orders/{orderUpdateStatusDto.OrderId}?status={orderUpdateStatusDto.Status}";

                var response = await _httpClient.PutAsync(requestUri, null);

                if (!response.IsSuccessStatusCode)
                {
                    context.Logger.LogError($"Failed to process update order status for message ID: {record.MessageId}, Status Code: {response.StatusCode}");
                    continue;
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Error processing message ID: {record.MessageId}, Error: {ex.Message}");
                // Aqui você pode enviar para uma DLQ ou outro mecanismo de fallback
            }
        }
    }
}