using Amazon.DynamoDBv2.DataModel;

namespace fiap_order_service.Models
{
    [DynamoDBTable("Orders")]
    public class Order
    {
        public Guid Id { get; set; }
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
