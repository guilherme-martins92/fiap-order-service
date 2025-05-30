using fiap_order_service.Models;

namespace fiap_order_service.Messaging
{
    public interface ISqsClientService
    {
        Task SendMessageAsync(PaymentPayLoad paymentPayLoad);
    }
}
