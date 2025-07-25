namespace fiap_order_service.Dtos
{
    public class OrderDto
    {
        public required CustomerDto Customer { get; set; }
        public required ItemOrderDto Item { get; set; }
    }
}