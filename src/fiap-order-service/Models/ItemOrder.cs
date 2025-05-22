namespace fiap_order_service.Models
{
    public class ItemOrder
    {
        public int ItemOrderId { get; set; }
        public Guid VehicleId { get; set; }
        public int Amount { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
