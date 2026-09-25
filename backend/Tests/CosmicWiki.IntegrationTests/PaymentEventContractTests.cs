using System.Text.Json;
using CosmicWiki.Messaging.Contracts.Payment;

namespace CosmicWiki.IntegrationTests;

public sealed class PaymentEventContractTests
{
    [Fact]
    public void Payment_status_event_round_trips_with_web_defaults()
    {
        var expected = new PaymentStatusChangedIntegrationEvent(
            Guid.NewGuid(),
            1,
            DateTime.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "pi_test",
            125.50m,
            "VND",
            "succeeded");

        var payload = JsonSerializer.Serialize(
            expected,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var actual = JsonSerializer.Deserialize<PaymentStatusChangedIntegrationEvent>(
            payload,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Payment_status_event_preserves_decimal_amount_and_currency()
    {
        var expected = new PaymentStatusChangedIntegrationEvent(
            Guid.NewGuid(),
            1,
            DateTime.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "pi_decimal",
            0.01m,
            "usd",
            "failed");

        var payload = JsonSerializer.Serialize(expected);
        var actual = JsonSerializer.Deserialize<PaymentStatusChangedIntegrationEvent>(payload);

        Assert.NotNull(actual);
        Assert.Equal(expected.Amount, actual.Amount);
        Assert.Equal(expected.Currency, actual.Currency);
        Assert.Equal(expected.Status, actual.Status);
    }
}
