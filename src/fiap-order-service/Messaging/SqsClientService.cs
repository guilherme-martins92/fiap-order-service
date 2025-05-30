using Amazon.SQS;
using Amazon.SQS.Model;
using fiap_order_service.Models;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace fiap_order_service.Messaging
{
    [ExcludeFromCodeCoverage]
    public class SqsClientService : ISqsClientService
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly string _queueUrl = "https://sqs.us-east-1.amazonaws.com/891377307312/PaymentProcessingQueue";
        private readonly ILogger<SqsClientService> _logger;

        public SqsClientService(IAmazonSQS sqsClient, ILogger<SqsClientService> logger)
        {
            _sqsClient = sqsClient;
            _logger = logger;
        }

        public async Task SendMessageAsync(PaymentPayLoad paymentPayLoad)
        {
            var jsonMessage = JsonSerializer.Serialize(paymentPayLoad);

            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = jsonMessage
            };

            await _sqsClient.SendMessageAsync(request);
            _logger.LogInformation("Mensagem enviada para SQS com sucesso!");
        }
    }
}
