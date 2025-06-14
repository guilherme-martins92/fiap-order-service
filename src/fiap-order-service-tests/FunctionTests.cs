using fiap_payment_processor;
using fiap_payment_processor.Models;
using Moq;
using Moq.Protected;
using System.Net;

namespace fiap_payment_processor_tests;
public class FunctionTests
{
    [Fact]
    public async Task ProcessPaymentAsync_ReturnsSuccess_WhenUpdateSucceeds()
    {
        // Arrange
        var paymentRequest = new PaymentRequest
        {
            OrderId = Guid.NewGuid(),
            Amount = 100,
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            Description = "Test"
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var function = new FunctionTestable(httpClient);

        // Act
        var result = await function.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.Contains("Pagamento processado com sucesso!", result);
        Assert.Contains("Status de pagamento atualizado com sucesso.", result);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ReturnsError_WhenUpdateFails()
    {
        // Arrange
        var paymentRequest = new PaymentRequest
        {
            OrderId = Guid.NewGuid(),
            Amount = 100,
            PaymentMethod = "CreditCard",
            CustomerEmail = "test@example.com",
            Description = "Test"
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Network error"));

        var httpClient = new HttpClient(handlerMock.Object);
        var function = new FunctionTestable(httpClient);

        // Act
        var result = await function.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.Contains("Erro ao processar pagamento: Network error", result);
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_ReturnsSuccess_WhenApiReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var function = new FunctionTestable(httpClient);

        // Act
        var result = await function.UpdatePaymentStatusAsync(orderId, "PAGO");

        // Assert
        Assert.Equal("Status de pagamento atualizado com sucesso.", result);
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_ReturnsFailure_WhenApiReturnsError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                ReasonPhrase = "Bad Request"
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var function = new FunctionTestable(httpClient);

        // Act
        var result = await function.UpdatePaymentStatusAsync(orderId, "PAGO");

        // Assert
        Assert.Equal("Falha ao tentar atualizar o status de pagamento: Bad Request", result);
    }

    // Helper class to inject HttpClient
    private class FunctionTestable : Function
    {
        public FunctionTestable(HttpClient httpClient)
        {
            typeof(Function)
                .GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, httpClient);
        }
    }
}