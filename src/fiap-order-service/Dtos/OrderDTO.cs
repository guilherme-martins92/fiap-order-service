using fiap_order_service.Models;

namespace fiap_order_service.Dtos
{
    public class OrderDto
    {
        public required string CustomerDocument { get; set; }
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public List<ItemOrderDto> Itens { get; set; } = new List<ItemOrderDto>();
    }
}
