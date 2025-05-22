using fiap_order_service.Models;

namespace fiap_order_service.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public async Task<Order> CreateOrderAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order), "Order cannot be null");
            // Simulate async database operation
            await Task.Delay(100);

            // Here you would typically add the order to your database context and save changes
            // For this example, we will just return the order as if it was saved successfully
            return order;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            //simulate a lista return from a database
            var orders = new List<Order>
            {
                new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerDocument = "12345678900",
                    CustomerName = "John Doe",
                    CustomerEmail = "teste@teste.com",
                    Status = Order.OrderStatus.Created,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    Itens = new List<ItemOrder>
                    {
                        new ItemOrder
                        {
                            VehicleId = Guid.NewGuid(),
                            UnitPrice = 10000,
                            Amount = 1,
                            TotalPrice = 10000,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        },
                        new ItemOrder
                        {
                            VehicleId = Guid.NewGuid(),
                            UnitPrice = 20000,
                            Amount = 2,
                            TotalPrice = 40000,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        }
                    }
                }
            };
            return orders;
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<Order?> UpdateOrderAsync(int id, Order order)
        {
            throw new NotImplementedException();
        }
    }
}
