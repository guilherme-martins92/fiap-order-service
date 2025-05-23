namespace fiap_order_service.Models
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public required string CustomerDocument { get; set; }
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public required string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public List<ItemOrder> Itens { get; set; } = new List<ItemOrder>();
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
