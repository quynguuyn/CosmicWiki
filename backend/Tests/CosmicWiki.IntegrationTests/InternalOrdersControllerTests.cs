using CosmicWiki.Ordering.Api.Controllers;
using CosmicWiki.Ordering.Api.Internal;
using CosmicWiki.Ordering.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CosmicWiki.IntegrationTests;

public sealed class InternalOrdersControllerTests
{
    [Fact]
    public async Task Duplicate_service_key_headers_are_rejected_without_calling_validator()
    {
        var validator = new RecordingPaymentIntentValidator();
        var controller = CreateController(validator);
        var orderId = Guid.NewGuid();
        controller.Request.Headers.Append("X-Internal-Service-Key", "local-key");
        controller.Request.Headers.Append("X-Internal-Service-Key", "local-key");

        var result = await controller.ValidatePaymentIntent(
            orderId,
            "pi_test",
            10,
            "VND",
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
        Assert.False(validator.WasCalled);
    }

    [Fact]
    public async Task Valid_request_disables_caching_and_calls_validator()
    {
        var validator = new RecordingPaymentIntentValidator { Result = true };
        var controller = CreateController(validator);
        var orderId = Guid.NewGuid();
        controller.Request.Headers.Append("X-Internal-Service-Key", "local-key");

        var result = await controller.ValidatePaymentIntent(
            orderId,
            "pi_test",
            10,
            "VND",
            CancellationToken.None);

        Assert.IsType<OkResult>(result);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
        Assert.Equal("no-cache", controller.Response.Headers.Pragma.ToString());
        Assert.True(validator.WasCalled);
        Assert.Equal(orderId, validator.OrderId);
    }

    [Fact]
    public async Task Invalid_input_is_rejected_after_authentication()
    {
        var validator = new RecordingPaymentIntentValidator();
        var controller = CreateController(validator);
        controller.Request.Headers.Append("X-Internal-Service-Key", "local-key");

        var result = await controller.ValidatePaymentIntent(
            Guid.NewGuid(),
            "pi_test",
            10,
            "VN1",
            CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
        Assert.False(validator.WasCalled);
    }

    private static InternalOrdersController CreateController(
        RecordingPaymentIntentValidator validator)
    {
        var controller = new InternalOrdersController(
            validator,
            Options.Create(new InternalOrderingOptions
            {
                ServiceKey = "local-key"
            }));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    private sealed class RecordingPaymentIntentValidator
        : IOrderPaymentIntentValidator
    {
        public bool Result { get; init; }
        public bool WasCalled { get; private set; }
        public Guid OrderId { get; private set; }

        public Task<bool> IsPaymentIntentValidAsync(
            Guid orderId,
            string paymentIntentId,
            decimal amount,
            string currency,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            OrderId = orderId;
            return Task.FromResult(Result);
        }
    }
}
