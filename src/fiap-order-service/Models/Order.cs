namespace fiap_order_service.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public required string CustomerDocument { get; set; }
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public OrderStatus Status { get; set; }
        public List<ItemOrder> Itens { get; set; } = new List<ItemOrder>();
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public enum OrderStatus
        {
            Created = 1,
            Processing = 2,
            Completed = 3,
            Canceled = 4
        }
    }
}
