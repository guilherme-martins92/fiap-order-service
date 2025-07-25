﻿using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace fiap_order_service.Infrastructure.EventBridge
{
    [ExcludeFromCodeCoverage]
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
                EventType = "CompraCancelada",
                OrderId = orderId,
                VehicleId = vehicleId,
                Timestamp = DateTime.UtcNow
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
                throw new InvalidOperationException("Falha ao publicar evento CompraCancelada no EventBridge.");
            }
        }

        public async Task PublicarCompraRealizadaAsync(Guid orderId, Guid vehicleId)
        {
            var detail = JsonSerializer.Serialize(new
            {
                EventType = "CompraRealizada",
                OrderId = orderId,
                VehicleId = vehicleId,
                Timestamp = DateTime.UtcNow
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
                throw new InvalidOperationException("Falha ao publicar evento CompraRealizada no EventBridge.");
            }
        }
    }
}