namespace fiap_order_service.Dtos
{
    public class ItemOrderDto
    {
        public Guid VehicleExternalId { get; set; }
        public int Amount { get; set; }
    }
}
