namespace Models
{
    public class UpdateOrderDto
    {
        public required Guid OrderId { get; set; }
        public required string Status { get; set; }
    }
}