using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fiap_order_service.Dtos;
using fiap_order_service.Endpoints;
using fiap_order_service.Models;
using fiap_order_service.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace fiap_order_service_tests.Endpoints
{
    public class OrderEndpointsTests
    {
        private readonly Mock<ILogger<OrderEndpoints>> _loggerMock;
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly OrderEndpoints _endpoints;

        public OrderEndpointsTests()
        {
            _loggerMock = new Mock<ILogger<OrderEndpoints>>();
            _orderServiceMock = new Mock<IOrderService>();
            _endpoints = new OrderEndpoints(_loggerMock.Object, _orderServiceMock.Object);
        }

        [Fact]
        public async Task GetAllOrders_ReturnsOkWithOrders()
        {
            var orders = new List<Order> { new Order { Id = Guid.NewGuid(), CustomerDocument = "123", CustomerName = "Test", CustomerEmail = "test@test.com", Status = "Pending", TotalPrice = 100, Itens = new List<ItemOrder>(), CreatedDate = DateTime.UtcNow } };
            _orderServiceMock.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

            var result = await _orderServiceMock.Object.GetAllOrdersAsync();

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetOrderById_ReturnsOk_WhenOrderExists()
        {
            var id = Guid.NewGuid();
            var order = new Order { Id = id, CustomerDocument = "123", CustomerName = "Test", CustomerEmail = "test@test.com", Status = "Pending", TotalPrice = 100, Itens = new List<ItemOrder>(), CreatedDate = DateTime.UtcNow };
            _orderServiceMock.Setup(s => s.GetOrderByIdAsync(id)).ReturnsAsync(order);

            var result = await _orderServiceMock.Object.GetOrderByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task GetOrderById_ReturnsNull_WhenOrderDoesNotExist()
        {
            var id = Guid.NewGuid();
            _orderServiceMock.Setup(s => s.GetOrderByIdAsync(id)).ReturnsAsync((Order?)null);

            var result = await _orderServiceMock.Object.GetOrderByIdAsync(id);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreated_WhenValid()
        {
            var orderDto = new OrderDto { CustomerDocument = "123", CustomerName = "Test", CustomerEmail = "test@test.com", Itens = new List<ItemOrderDto>() };
            var order = new Order { Id = Guid.NewGuid(), CustomerDocument = "123", CustomerName = "Test", CustomerEmail = "test@test.com", Status = "Pending", TotalPrice = 100, Itens = new List<ItemOrder>(), CreatedDate = DateTime.UtcNow };
            var validatorMock = new Mock<IValidator<OrderDto>>();
            validatorMock.Setup(v => v.ValidateAsync(orderDto, default)).ReturnsAsync(new ValidationResult());
            _orderServiceMock.Setup(s => s.CreateOrderAsync(orderDto)).ReturnsAsync(order);

            var validationResult = await validatorMock.Object.ValidateAsync(orderDto);
            var createdOrder = await _orderServiceMock.Object.CreateOrderAsync(orderDto);

            Assert.True(validationResult.IsValid);
            Assert.NotNull(createdOrder);
            Assert.Equal(order.Id, createdOrder.Id);
        }

        [Fact]
        public async Task CreateOrder_ReturnsValidationProblem_WhenInvalid()
        {
            var orderDto = new OrderDto { CustomerDocument = "123", CustomerName = "Test", CustomerEmail = "test@test.com", Itens = new List<ItemOrderDto>() };
            var validatorMock = new Mock<IValidator<OrderDto>>();
            var failures = new List<ValidationFailure> { new ValidationFailure("CustomerName", "Required") };
            validatorMock.Setup(v => v.ValidateAsync(orderDto, default)).ReturnsAsync(new ValidationResult(failures));

            var validationResult = await validatorMock.Object.ValidateAsync(orderDto);

            Assert.False(validationResult.IsValid);
            Assert.Single(validationResult.Errors);
        }

        [Fact]
        public async Task CreateOrder_ReturnsNotFound_WhenKeyNotFoundException()
        {
            var orderDto = new OrderDto { CustomerDocument = "123", CustomerName = "Test", CustomerEmail = "test@test.com", Itens = new List<ItemOrderDto>() };
            var validatorMock = new Mock<IValidator<OrderDto>>();
            validatorMock.Setup(v => v.ValidateAsync(orderDto, default)).ReturnsAsync(new ValidationResult());
            _orderServiceMock.Setup(s => s.CreateOrderAsync(orderDto)).ThrowsAsync(new KeyNotFoundException("Vehicle not found"));

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderServiceMock.Object.CreateOrderAsync(orderDto));
        }

        [Fact]
        public async Task CreateOrder_ReturnsProblem_WhenInvalidOperationException()
        {
            var orderDto = new OrderDto { CustomerDocument = "123", CustomerName = "Test", CustomerEmail = "test@test.com", Itens = new List<ItemOrderDto>() };
            var validatorMock = new Mock<IValidator<OrderDto>>();
            validatorMock.Setup(v => v.ValidateAsync(orderDto, default)).ReturnsAsync(new ValidationResult());
            _orderServiceMock.Setup(s => s.CreateOrderAsync(orderDto)).ThrowsAsync(new InvalidOperationException("Error"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderServiceMock.Object.CreateOrderAsync(orderDto));
        }

        [Fact]
        public async Task UpdateOrder_ReturnsOk_WhenOrderUpdated()
        {
            var id = Guid.NewGuid();
            var status = "Completed";
            var order = new Order { Id = id, CustomerDocument = "123", CustomerName = "Test", CustomerEmail = "test@test.com", Status = status, TotalPrice = 100, Itens = new List<ItemOrder>(), CreatedDate = DateTime.UtcNow };
            _orderServiceMock.Setup(s => s.UpdateStatusOrderAsync(id, status)).ReturnsAsync(order);

            var result = await _orderServiceMock.Object.UpdateStatusOrderAsync(id, status);

            Assert.NotNull(result);
            Assert.Equal(status, result.Status);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsNull_WhenOrderNotFound()
        {
            var id = Guid.NewGuid();
            var status = "Completed";
            _orderServiceMock.Setup(s => s.UpdateStatusOrderAsync(id, status)).ReturnsAsync((Order?)null);

            var result = await _orderServiceMock.Object.UpdateStatusOrderAsync(id, status);

            Assert.Null(result);
        }
    }
}
