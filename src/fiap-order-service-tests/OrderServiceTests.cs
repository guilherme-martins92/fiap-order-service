using fiap_order_service.Constants;
using fiap_order_service.Dtos;
using fiap_order_service.Infrastructure.EventBridge;
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
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly OrderService _orderService;
        private readonly Mock<ICustomerService> _customerServiceMock;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _catalogServiceMock = new Mock<ICatalogService>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _customerServiceMock = new Mock<ICustomerService>();

            _orderService = new OrderService(_orderRepositoryMock.Object, _catalogServiceMock.Object, _loggerMock.Object, _eventPublisherMock.Object, _customerServiceMock.Object);
        }

        private Customer BuildCustomer()
        {
            return new Customer
            {
                Id = Guid.NewGuid(),
                DocumentNumber = "12345678900",
                FirstName = "John",
                LastName = "Doe",
                Email = "test@test.com",
                Street = "123 Main St",
                HouseNumber = "456",
                City = "Test City",
                State = "TS",
                PostalCode = "12345-678",
                Country = "Test Country",
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                PhoneNumber = "1234567890"
            };
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldCreateOrder_WhenOrderDtoIsValid()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customer = BuildCustomer();

            var orderDto = new OrderDto
            {
                Customer = new CustomerDto
                {
                    Id = customer.Id,
                },
                Item = new ItemOrderDto
                {
                    VehicleExternalId = vehicleId,
                    Amount = 2
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
            _customerServiceMock.Setup(x => x.GetCustomerByIdAsync(orderDto.Customer.Id)).ReturnsAsync(customer);

            // Act
            var result = await _orderService.CreateOrderAsync(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderDto.Customer.Id, result.Customer.Id);
            Assert.Equal(customer.FirstName, customer.FirstName);
            Assert.Equal(customer.LastName, customer.LastName);
            Assert.Equal(customer.Email, customer.Email);
            Assert.Equal(customer.DateOfBirth, customer.DateOfBirth);
            Assert.Equal(customer.PhoneNumber, customer.PhoneNumber);
            Assert.Equal(customer.Street, customer.Street);
            Assert.Equal(customer.HouseNumber, customer.HouseNumber);
            Assert.Equal(customer.City, customer.City);
            Assert.Equal(customer.State, customer.State);
            Assert.Equal(customer.PostalCode, customer.PostalCode);
            Assert.Equal(customer.Country, customer.Country);

            Assert.Equal(OrderStatus.Created, result.Status);
            Assert.NotNull(result.Item);
            Assert.Equal(vehicleId, result.Item.VehicleId);
            Assert.Equal(200000m, result.TotalPrice);
            _eventPublisherMock.Verify(x => x.PublicarCompraRealizadaAsync(result.Id, vehicleId), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowKeyNotFoundException_WhenVehicleNotFound()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var orderDto = new OrderDto
            {
                Customer = new CustomerDto
                {
                    Id = Guid.NewGuid()
                },
                Item = new ItemOrderDto
                {
                    VehicleExternalId = vehicleId,
                    Amount = 1
                }
            };

            _catalogServiceMock.Setup(x => x.GetVehicleByIdAsync(vehicleId)).ReturnsAsync((Vehicle?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.CreateOrderAsync(orderDto));
        }

        [Fact]
        public async Task CreateOrderAsync_ShouldThrowKeyNotFoundException_WhenCustomerNotFound()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var orderDto = new OrderDto
            {
                Customer = new CustomerDto
                {
                    Id = Guid.NewGuid()
                },
                Item = new ItemOrderDto
                {
                    VehicleExternalId = vehicleId,
                    Amount = 1
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
            _customerServiceMock.Setup(x => x.GetCustomerByIdAsync(orderDto.Customer.Id)).ReturnsAsync((Customer?)null);

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
                Customer = new CustomerDto
                {
                    Id = Guid.NewGuid()
                },
                Item = new ItemOrderDto
                {
                    VehicleExternalId = vehicleId,
                    Amount = 1
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
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.CreateOrderAsync(orderDto));
        }

        [Fact]
        public async Task GetAllOrdersAsync_ShouldReturnOrderedOrders_WhenOrdersExist()
        {
            // Arrange
            var ItemOrder1 = new ItemOrder
            {
                VehicleId = Guid.NewGuid(),
                Model = "Model Y",
                Brand = "Tesla",
                Color = "Red",
                Year = 2023,
                Amount = 1,
                UnitPrice = 150000m,
                TotalPrice = 150000m
            };

            var ItemOrder2 = new ItemOrder
            {
                VehicleId = Guid.NewGuid(),
                Model = "Model 3",
                Brand = "Tesla",
                Color = "Blue",
                Year = 2022,
                Amount = 1,
                UnitPrice = 100000m,
                TotalPrice = 100000m
            };

            var customer = BuildCustomer();

            var orders = new List<Order>
            {
                new Order { Customer = customer, Status = "Created", TotalPrice = 300,Item = ItemOrder1, CreatedDate = DateTime.UtcNow },
                new Order { Customer = customer, Status = "Created", TotalPrice = 100, Item = ItemOrder2, CreatedDate = DateTime.UtcNow }
            };

            _orderRepositoryMock.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(100, result[0].TotalPrice);
            Assert.Equal(300, result[1].TotalPrice);
            Assert.Equal(customer.Id, result[0].Customer.Id);
            Assert.Equal(customer.DocumentNumber, result[0].Customer.DocumentNumber);
            Assert.Equal(customer.FirstName, result[0].Customer.FirstName);
            Assert.Equal(customer.LastName, result[0].Customer.LastName);
            Assert.Equal(customer.Email, result[0].Customer.Email);
            Assert.Equal(customer.DateOfBirth, result[0].Customer.DateOfBirth);
            Assert.Equal(customer.PhoneNumber, result[0].Customer.PhoneNumber);
            Assert.Equal(customer.Street, result[0].Customer.Street);
            Assert.Equal(customer.HouseNumber, result[0].Customer.HouseNumber);
            Assert.Equal(customer.City, result[0].Customer.City);
            Assert.Equal(customer.State, result[0].Customer.State);
            Assert.Equal(customer.PostalCode, result[0].Customer.PostalCode);
            Assert.Equal(customer.Country, result[0].Customer.Country);
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
            var customer = BuildCustomer();

            var order = new Order
            {
                Id = orderId,
                Customer = customer,
                Status = "Created",
                TotalPrice = 100,
                Item = new ItemOrder
                {
                    VehicleId = Guid.NewGuid(),
                    Model = "Model X",
                    Brand = "Tesla",
                    Color = "Black",
                    Year = 2022,
                    Amount = 1,
                    UnitPrice = 100000m,
                    TotalPrice = 100000m
                },
                CreatedDate = DateTime.UtcNow
            };

            _orderRepositoryMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Equal(customer.Id, result.Customer.Id);
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
            var customer = BuildCustomer();
            var order = new Order
            {
                Id = orderId,
                Customer = customer,
                Status = "Created",
                TotalPrice = 100,
                Item = new ItemOrder
                {
                    VehicleId = Guid.NewGuid(),
                    Model = "Model Y",
                    Brand = "Tesla",
                    Color = "Red",
                    Year = 2023,
                    Amount = 1,
                    UnitPrice = 100000m,
                    TotalPrice = 100000m
                },
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
        public async Task UpdateStatusOrderAsync_ShouldPublicEvent_WhenStatusIsCanceled()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var customer = BuildCustomer();
            var order = new Order
            {
                Id = orderId,
                Customer = customer,
                Status = "CANCELADO",
                TotalPrice = 100,
                Item = new ItemOrder
                {
                    VehicleId = Guid.NewGuid(),
                    Model = "Model Y",
                    Brand = "Tesla",
                    Color = "Red",
                    Year = 2023,
                    Amount = 1,
                    UnitPrice = 100000m,
                    TotalPrice = 100000m
                },
                CreatedDate = DateTime.UtcNow
            };

            _orderRepositoryMock.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(order);
            _orderRepositoryMock.Setup(x => x.UpdateStatusOrderAsync(orderId, It.IsAny<Order>())).ReturnsAsync((Guid id, Order o) => o);

            // Act
            var result = await _orderService.UpdateStatusOrderAsync(orderId, "CANCELADO");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CANCELADO", result.Status);
            _eventPublisherMock.Verify(x => x.PublicarCompraCanceladaAsync(orderId, order.Item!.VehicleId), Times.Once);
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
    }
}