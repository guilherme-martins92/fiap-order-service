using System.Net;
using fiap_payment_processor;
using fiap_payment_processor.Models;
using Moq;
using Moq.Protected;

namespace fiap_payment_processor_tests;
public class FunctionTests
{
    private PaymentRequest GetSamplePaymentRequest()
    {
        return new PaymentRequest
        {
            OrderId = "12345",
            Amount = 100.00m,
            PaymentMethod = "credit_card",
            Currency = "USD",
            CustomerEmail = "test@example.com",
            Description = "Test payment"
        };
    }

    private HttpClient GetMockHttpClient(HttpStatusCode postStatus, HttpStatusCode putStatus)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("process-payment")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = postStatus,
                Content = new StringContent("")
            });

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri!.ToString().Contains("/status")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = putStatus,
                Content = new StringContent("")
            });

        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_SuccessfulPayment_ReturnsSuccessMessage()
    {
        // Arrange
        var paymentRequest = GetSamplePaymentRequest();
        var httpClient = GetMockHttpClient(HttpStatusCode.OK, HttpStatusCode.OK);
        var function = new FunctionTestable(httpClient);

        // Act
        var result = await function.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.Equal("Payment processed successfully.", result);
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_SuccessfulUpdate_ReturnsSuccessMessage()
    {
        // Arrange
        var orderId = "12345";
        var status = "PAGO";
        var httpClient = GetMockHttpClient(HttpStatusCode.OK, HttpStatusCode.OK);
        var function = new FunctionTestable(httpClient);

        // Act
        var result = await function.UpdatePaymentStatusAsync(orderId, status);

        // Assert
        Assert.Equal("Payment status updated successfully.", result);
    }

    [Fact]
    public async Task UpdatePaymentStatusAsync_FailedUpdate_ReturnsFailureMessage()
    {
        // Arrange
        var orderId = "12345";
        var status = "PAGO";
        var httpClient = GetMockHttpClient(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        var function = new FunctionTestable(httpClient);

        // Act
        var result = await function.UpdatePaymentStatusAsync(orderId, status);

        // Assert
        Assert.StartsWith("Failed to update payment status:", result);
    }

    // Helper class to inject HttpClient
    private class FunctionTestable : Function
    {
        public FunctionTestable(HttpClient httpClient)
        {
            typeof(Function)
                .GetField("_httpClient", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(null, httpClient);
        }
    }
}