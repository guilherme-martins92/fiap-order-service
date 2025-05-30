namespace fiap_order_service.Messaging
{
    public interface ISqsClientService
    {
        Task SendMessageAsync(string message);
    }
}
