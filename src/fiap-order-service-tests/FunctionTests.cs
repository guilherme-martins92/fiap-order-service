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
            OrderId = Guid.NewGuid(),
            Amount = 100.00m,
            PaymentMethod = "credit_card",          
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

    //[Fact]
    //public async Task ProcessPaymentAsync_SuccessfulPayment_ReturnsSuccessMessage()
    //{
    //    // Arrange
    //    var paymentRequest = GetSamplePaymentRequest();
    //    var httpClient = GetMockHttpClient(HttpStatusCode.OK, HttpStatusCode.OK);
    //    var function = new FunctionTestable(httpClient);

    //    // Act
    //    var result = await Function.ProcessPaymentAsync(paymentRequest);

    //    // Assert
    //    Assert.Equal("Pagamento processado com sucesso!", result);
    //}

    //[Fact]
    //public async Task UpdatePaymentStatusAsync_SuccessfulUpdate_ReturnsSuccessMessage()
    //{
    //    // Arrange
    //    var orderId = Guid.NewGuid();
    //    var status = "PAGO";
    //    var httpClient = GetMockHttpClient(HttpStatusCode.OK, HttpStatusCode.OK);
    //    var function = new FunctionTestable(httpClient);

    //    // Act
    //    var result = await Function.UpdatePaymentStatusAsync(orderId, status);

    //    // Assert
    //    Assert.Equal("Status de pagamento atualizado com sucesso.", result);
    //}

    //[Fact]
    //public async Task UpdatePaymentStatusAsync_FailedUpdate_ReturnsFailureMessage()
    //{
    //    // Arrange
    //    var orderId = Guid.NewGuid();
    //    var status = "PAGO";
    //    var httpClient = GetMockHttpClient(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    //    var function = new FunctionTestable(httpClient);

    //    // Act
    //    var result = await Function.UpdatePaymentStatusAsync(orderId, status);

    //    // Assert
    //    Assert.StartsWith("Falha ao tentar atualizar o status de pagamento:", result);
    //}

    //[Fact]
    //public async Task FunctionHandler_ValidPaymentRequest_ProcessesPayment()
    //{
    //    // Arrange
    //    var paymentRequest = GetSamplePaymentRequest();
    //    var sqsEvent = new Amazon.Lambda.SQSEvents.SQSEvent
    //    {
    //        Records = new List<Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage>
    //        {
    //            new Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage
    //            {
    //                Body = System.Text.Json.JsonSerializer.Serialize(paymentRequest)
    //            }
    //        }
    //    };
    //    var httpClient = GetMockHttpClient(HttpStatusCode.OK, HttpStatusCode.OK);
    //    var function = new FunctionTestable(httpClient);

    //    var loggerMock = new Mock<Amazon.Lambda.Core.ILambdaLogger>();
    //    var contextMock = new Mock<Amazon.Lambda.Core.ILambdaContext>();
    //    contextMock.SetupGet(c => c.Logger).Returns(loggerMock.Object);

    //    // Act
    //    await function.FunctionHandler(sqsEvent, contextMock.Object);

    //    // Assert
    //    loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Pagamento processado: Pagamento processado com sucesso!"))), Times.Once);
    //}

    //[Fact]
    //public async Task FunctionHandler_InvalidPaymentRequest_LogsInvalidRequest()
    //{
    //    // Arrange
    //    var sqsEvent = new Amazon.Lambda.SQSEvents.SQSEvent
    //    {
    //        Records = new List<Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage>
    //        {
    //            new Amazon.Lambda.SQSEvents.SQSEvent.SQSMessage
    //            {
    //                Body = "not a valid json"
    //            }
    //        }
    //    };
    //    var httpClient = GetMockHttpClient(HttpStatusCode.OK, HttpStatusCode.OK);
    //    var function = new FunctionTestable(httpClient);

    //    var loggerMock = new Mock<Amazon.Lambda.Core.ILambdaLogger>();
    //    var contextMock = new Mock<Amazon.Lambda.Core.ILambdaContext>();
    //    contextMock.SetupGet(c => c.Logger).Returns(loggerMock.Object);

    //    // Act
    //    await function.FunctionHandler(sqsEvent, contextMock.Object);

    //    // Assert
    //    loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.StartsWith("Error processing record:"))), Times.Once);
    //}
}