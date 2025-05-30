namespace fiap_order_service.Models
{
    public class PaymentPayLoad
    {
        public required Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public required string PaymentMethod { get; set; }
        public required string CustomerEmail { get; set; }
        public required string Description { get; set; }
    }
}
