using fiap_order_service.Constants;
using fiap_order_service.Dtos;
using fiap_order_service.Infrastructure.HttpClients;
using fiap_order_service.Messaging;
using fiap_order_service.Models;
using fiap_order_service.Repositories;
using fiap_order_service.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace fiap_order_service_tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<ICatalogService> _catalogServiceMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly Mock<ISqsClientService> _sqsClientServiceMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _catalogServiceMock = new Mock<ICatalogService>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _sqsClientServiceMock = new Mock<ISqsClientService>();
            _orderService = new OrderService(_orderRepositoryMock.Object, _catalogServiceMock.Object, _loggerMock.Object, _sqsClientServiceMock.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateOrder_WhenOrderDtoIsValid()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var orderDto = new OrderDto
            {
                CustomerDocument = "12345678900",
                CustomerName = "John Doe",
                CustomerEmail = "john@doe.com",
                Itens = new List<ItemOrderDto>
                {
                    new ItemOrderDto { VehicleExternalId = vehicleId, Amount = 2 }
                }
            };

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Model = "Model X",
                Brand = "Tesla",
                Color = "Black",
                Year = 2022,
                Price = 100000m
            };

            _catalogServiceMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicle);
            _orderRepositoryMock.Setup(x => x.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync((Order o) => o);

            // Act
            var result = await _orderService.CreateOrderAsync(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderDto.CustomerDocument, result.CustomerDocument);
            Assert.Single(result.Itens);
            Assert.Equal(vehicleId, result.Itens[0].VehicleId);
            Assert.Equal(200000m, result.TotalPrice);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowArgumentNullException_WhenOrderDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _orderService.CreateOrderAsync(null!));
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowKeyNotFoundException_WhenVehicleNotFound()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var orderDto = new OrderDto
            {
                CustomerDocument = "12345678900",
                CustomerName = "John Doe",
                CustomerEmail = "john@doe.com",
                Itens = new List<ItemOrderDto>
                {
                    new ItemOrderDto { VehicleExternalId = vehicleId, Amount = 1 }
                }
            };

            _catalogServiceMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync((Vehicle?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.CreateOrderAsync(orderDto));
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowInvalidOperationException_WhenRepositoryReturnsNull()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var orderDto = new OrderDto
            {
                CustomerDocument = "12345678900",
                CustomerName = "John Doe",
                CustomerEmail = "john@doe.com",
                Itens = new List<ItemOrderDto>
                {
                    new ItemOrderDto { VehicleExternalId = vehicleId, Amount = 1 }
                }
            };

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Model = "Model S",
                Brand = "Tesla",
                Color = "White",
                Year = 2021,
                Price = 80000m
            };

            _catalogServiceMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync(vehicle);
            _orderRepositoryMock.Setup(x => x.CreateOrderAsync(It.IsAny<Order>())).ReturnsAsync((Order?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(orderDto));
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnOrderedOrders_WhenOrdersExist()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = Guid.NewGuid(), CustomerDocument = "1", CustomerName = "A", CustomerEmail = "a@a.com", Status = "Created", TotalPrice = 300, Itens = new List<ItemOrder>(), CreatedDate = DateTime.UtcNow },
                new Order { Id = Guid.NewGuid(), CustomerDocument = "2", CustomerName = "B", CustomerEmail = "b@b.com", Status = "Created", TotalPrice = 100, Itens = new List<ItemOrder>(), CreatedDate = DateTime.UtcNow }
            };
            _orderRepositoryMock.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(100, result[0].TotalPrice);
            Assert.Equal(300, result[1].TotalPrice);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            _orderRepositoryMock.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(new List<Order>());

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                CustomerDocument = "1",
                CustomerName = "A",
                CustomerEmail = "a@a.com",
                Status = "Created",
                TotalPrice = 100,
                Itens = new List<ItemOrder>(),
                CreatedDate = DateTime.UtcNow
            };
            _orderRepositoryMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ShouldReturnsNull_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderRepositoryMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.Null(order);
        }

        [Fact]
        public async Task UpdateStatusOrderAsync_ShouldUpdateStatus_WhenOrderExists()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                CustomerDocument = "1",
                CustomerName = "A",
                CustomerEmail = "a@a.com",
                Status = "Created",
                TotalPrice = 100,
                Itens = new List<ItemOrder>(),
                CreatedDate = DateTime.UtcNow
            };
            _orderRepositoryMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _orderRepositoryMock.Setup(x => x.UpdateStatusOrderAsync(orderId, It.IsAny<Order>())).ReturnsAsync((Guid id, Order o) => o);

            // Act
            var result = await _orderService.UpdateStatusOrderAsync(orderId, "Completed");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Completed", result.Status);
        }

        [Fact]
        public async Task UpdateStatusOrderAsync_ShouldThrowArgumentNullException_WhenIdIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _orderService.UpdateStatusOrderAsync(Guid.Empty, "Completed"));
        }

        [Fact]
        public async Task UpdateStatusOrderAsync_ShouldThrowArgumentNullException_WhenStatusIsNullOrEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _orderService.UpdateStatusOrderAsync(Guid.NewGuid(), null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _orderService.UpdateStatusOrderAsync(Guid.NewGuid(), ""));
        }

        [Fact]
        public async Task UpdateStatusOrderAsync_ShouldThrowKeyNotFoundException_WhenOrderNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderRepositoryMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync((Order?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.UpdateStatusOrderAsync(orderId, "Completed"));
        }

        [Fact]
        public async Task SendOrderToPaymentQueue_ShouldUpdateStatusAndSendMessage_WhenOrderIsValid()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerDocument = "12345678900",
                CustomerName = "John Doe",
                CustomerEmail = "john@doe.com",
                Status = OrderStatus.Created,
                TotalPrice = 50000m,
                Itens = new List<ItemOrder>(),
                CreatedDate = DateTime.UtcNow
            };

            _orderRepositoryMock
                .Setup(x => x.UpdateStatusOrderAsync(order.Id, It.IsAny<Order>()))
                .ReturnsAsync((Guid id, Order o) => o);

            _sqsClientServiceMock
                .Setup(x => x.SendMessageAsync(It.IsAny<PaymentPayLoad>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _orderService.SendOrderToPaymentQueue(order);

            // Assert
            _orderRepositoryMock.Verify(x => x.UpdateStatusOrderAsync(order.Id, It.Is<Order>(o => o.Status == OrderStatus.PendingPayment)), Times.Once);
            _sqsClientServiceMock.Verify(x => x.SendMessageAsync(It.Is<PaymentPayLoad>(p =>
                p.OrderId == order.Id &&
                p.PaymentMethod == PaymentMethod.CreditCard &&
                p.CustomerEmail == order.CustomerEmail &&
                p.Amount == order.TotalPrice &&
                p.Description.Contains(order.CustomerName)
            )), Times.Once);
        }

        [Fact]
        public async Task SendOrderToPaymentQueue_ShouldThrowInvalidOperationException_WhenRepositoryThrows()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerDocument = "12345678900",
                CustomerName = "John Doe",
                CustomerEmail = "john@doe.com",
                Status = OrderStatus.Created,
                TotalPrice = 50000m,
                Itens = new List<ItemOrder>(),
                CreatedDate = DateTime.UtcNow
            };

            _orderRepositoryMock
                .Setup(x => x.UpdateStatusOrderAsync(order.Id, It.IsAny<Order>()))
                .ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.SendOrderToPaymentQueue(order));
        }

        [Fact]
        public async Task SendOrderToPaymentQueue_ShouldThrowInvalidOperationException_WhenSqsClientThrows()
        {
            // Arrange
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerDocument = "12345678900",
                CustomerName = "John Doe",
                CustomerEmail = "john@doe.com",
                Status = OrderStatus.Created,
                TotalPrice = 50000m,
                Itens = new List<ItemOrder>(),
                CreatedDate = DateTime.UtcNow
            };

            _orderRepositoryMock
                .Setup(x => x.UpdateStatusOrderAsync(order.Id, It.IsAny<Order>()))
                .ReturnsAsync(order);

            _sqsClientServiceMock
                .Setup(x => x.SendMessageAsync(It.IsAny<PaymentPayLoad>()))
                .ThrowsAsync(new Exception("SQS error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.SendOrderToPaymentQueue(order));
        }
    }
}
