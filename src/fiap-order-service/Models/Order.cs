using Amazon.DynamoDBv2.DataModel;

namespace fiap_order_service.Models
{
    [DynamoDBTable("Orders")]
    public class Order
    {
        public Guid Id { get; set; }
        public required Customer Customer { get; set; }
        public required string Status { get; set; }
        public decimal TotalPrice { get; set; }
        public required ItemOrder Item { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}