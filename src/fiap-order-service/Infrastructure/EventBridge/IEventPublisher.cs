namespace fiap_order_service.Infrastructure.EventBridge
{
    public interface IEventPublisher
    {
        Task PublicarCompraCanceladaAsync(Guid orderId, Guid vehicleId);
        Task PublicarCompraRealizadaAsync(Guid orderId, Guid vehicleId);
    }
}