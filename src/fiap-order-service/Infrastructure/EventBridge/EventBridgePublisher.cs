using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using System.Text.Json;

namespace fiap_order_service.Infrastructure.EventBridge
{
    public class EventBridgePublisher : IEventPublisher
    {
        private readonly IAmazonEventBridge _eventBridge;
        private const string EventBusName = "saga-event-bus";

        public EventBridgePublisher(IAmazonEventBridge eventBridge)
        {
            _eventBridge = eventBridge;
        }

        public async Task PublicarCompraCanceladaAsync(Guid orderId, Guid vehicleId)
        {
            var detail = JsonSerializer.Serialize(new
            {
                eventType = "CompraCancelada",
                orderId,
                vehicleId,             
                timestamp = DateTime.UtcNow
            });

            var request = new PutEventsRequest
            {
                Entries = new List<PutEventsRequestEntry>
            {
                new()
                {
                    Detail = detail,
                    DetailType = "CompraCancelada",
                    Source = "ms.compras",
                    EventBusName = EventBusName
                }
            }
            };

            var response = await _eventBridge.PutEventsAsync(request);

            if (response.FailedEntryCount > 0)
            {
                throw new Exception("Falha ao publicar evento CompraCancelada no EventBridge.");
            }
        }

        public async Task PublicarCompraRealizadaAsync(Guid orderId, Guid vehicleId)
        {
            var detail = JsonSerializer.Serialize(new
            {
                eventType = "CompraRealizada",
                orderId,
                vehicleId,
                timestamp = DateTime.UtcNow
            });
            var request = new PutEventsRequest
            {
                Entries = new List<PutEventsRequestEntry>
            {
                new()
                {
                    Detail = detail,
                    DetailType = "CompraRealizada",
                    Source = "ms.compras",
                    EventBusName = EventBusName
                }
            }
            };
            var response = await _eventBridge.PutEventsAsync(request);
            if (response.FailedEntryCount > 0)
            {
                throw new Exception("Falha ao publicar evento CompraRealizada no EventBridge.");
            }
        }
    }
}