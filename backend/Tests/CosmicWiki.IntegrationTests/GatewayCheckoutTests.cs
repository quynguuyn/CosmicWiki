using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace CosmicWiki.IntegrationTests;

public sealed class GatewayCheckoutTests : IAsyncLifetime
{
    private static readonly Uri GatewayUri = new(
        Environment.GetEnvironmentVariable("COSMICWIKI_GATEWAY_URL")
        ?? "http://localhost:5000");

    private static readonly string WebhookSecret =
        Environment.GetEnvironmentVariable("COSMICWIKI_WEBHOOK_SECRET")
        ?? "smoke-secret";

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync()
    {
        using var client = new HttpClient
        {
            BaseAddress = GatewayUri,
            Timeout = TimeSpan.FromSeconds(5)
        };

        var deadline = DateTime.UtcNow.AddSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            if (await IsGatewayReadyAsync(client))
                return;

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        throw new InvalidOperationException(
            $"Gateway dependencies were not ready at {GatewayUri}.");
    }

    [Fact]
    public async Task SuccessWebhook_is_idempotent_through_gateway()
    {
        using var client = new HttpClient { BaseAddress = GatewayUri };
        var checkout = await CreateCheckoutAsync(client);

        var first = await SendWebhookAsync(client, checkout, "succeeded");
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var duplicate = await SendWebhookAsync(client, checkout, "succeeded", checkout.EventId);
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        var duplicateBody = await duplicate.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(duplicateBody.GetProperty("duplicate").GetBoolean());
    }

    [Fact]
    public async Task FailedWebhook_is_accepted_through_gateway()
    {
        using var client = new HttpClient { BaseAddress = GatewayUri };
        var checkout = await CreateCheckoutAsync(client);

        var response = await SendWebhookAsync(client, checkout, "failed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Same_idempotency_key_returns_the_existing_order()
    {
        using var client = new HttpClient { BaseAddress = GatewayUri };
        var checkout = await CreateCheckoutAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(new
            {
                customerId = checkout.CustomerId,
                currency = checkout.Currency,
                lines = new[] { new { productId = checkout.ProductId, quantity = 1 } }
            })
        };
        request.Headers.Add("Idempotency-Key", checkout.IdempotencyKey);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var existing = await response.Content.ReadFromJsonAsync<Order>();

        Assert.NotNull(existing);
        Assert.Equal(checkout.OrderId, existing.OrderId);
        Assert.Equal(checkout.PaymentIntentId, existing.PaymentIntentId);
    }

    [Fact]
    public async Task Reusing_idempotency_key_for_different_request_is_rejected()
    {
        using var client = new HttpClient { BaseAddress = GatewayUri };
        var checkout = await CreateCheckoutAsync(client);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(new
            {
                customerId = checkout.CustomerId,
                currency = checkout.Currency,
                lines = new[] { new { productId = checkout.ProductId, quantity = 2 } }
            })
        };
        request.Headers.Add("Idempotency-Key", checkout.IdempotencyKey);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Webhook_with_invalid_signature_is_rejected()
    {
        using var client = new HttpClient { BaseAddress = GatewayUri };
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/payments/webhooks")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Payment-Timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        request.Headers.Add("X-Payment-Signature", "00");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Malformed_payment_event_is_published_to_dead_letter_topic()
    {
        var bootstrapServers =
            Environment.GetEnvironmentVariable("COSMICWIKI_KAFKA_BOOTSTRAP_SERVERS")
            ?? "localhost:9092";
        var marker = $"integration-dlq-{Guid.NewGuid():N}";

        using var consumer = new ConsumerBuilder<string, string>(
                new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = $"integration-dlq-{Guid.NewGuid():N}",
                    AutoOffsetReset = AutoOffsetReset.Latest,
                    EnableAutoCommit = false
                })
            .Build();
        consumer.Subscribe("cosmicwiki.payments.status-changed.dlq");
        consumer.Consume(TimeSpan.FromSeconds(2));

        using (var producer = new ProducerBuilder<string, string>(
                   new ProducerConfig { BootstrapServers = bootstrapServers }).Build())
        {
            await producer.ProduceAsync(
                "cosmicwiki.payments.status-changed",
                new Message<string, string>
                {
                    Key = marker,
                    Value = JsonSerializer.Serialize(new
                    {
                        eventId = "not-a-guid",
                        status = "unsupported",
                        marker
                    })
                });
        }

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
            if (result?.Message.Value.Contains(marker, StringComparison.Ordinal) == true)
            {
                var deadLetter = JsonSerializer.Deserialize<JsonElement>(
                    result.Message.Value);
                Assert.Equal("JsonException", deadLetter.GetProperty("errorType").GetString());
                return;
            }
        }

        Assert.Fail("Malformed payment event was not published to the DLQ.");
    }

    [Fact]
    public async Task Unprocessable_payment_event_is_retried_then_published_to_dead_letter_topic()
    {
        var bootstrapServers =
            Environment.GetEnvironmentVariable("COSMICWIKI_KAFKA_BOOTSTRAP_SERVERS")
            ?? "localhost:9092";
        var marker = Guid.NewGuid();
        var unknownOrderId = Guid.NewGuid();

        using var consumer = new ConsumerBuilder<string, string>(
                new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = $"integration-retry-dlq-{Guid.NewGuid():N}",
                    AutoOffsetReset = AutoOffsetReset.Latest,
                    EnableAutoCommit = false
                })
            .Build();
        consumer.Subscribe("cosmicwiki.payments.status-changed.dlq");
        consumer.Consume(TimeSpan.FromSeconds(2));

        using (var producer = new ProducerBuilder<string, string>(
                   new ProducerConfig { BootstrapServers = bootstrapServers }).Build())
        {
            await producer.ProduceAsync(
                "cosmicwiki.payments.status-changed",
                new Message<string, string>
                {
                    Key = marker.ToString(),
                    Value = JsonSerializer.Serialize(new
                    {
                        eventId = marker,
                        version = 1,
                        occurredAtUtc = DateTime.UtcNow,
                        aggregateId = marker,
                        orderId = unknownOrderId,
                        paymentIntentId = $"retry_{marker:N}",
                        amount = 1,
                        currency = "VND",
                        status = "succeeded"
                    })
                });
        }

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
            if (result?.Message.Value.Contains(marker.ToString(), StringComparison.OrdinalIgnoreCase) != true)
                continue;

            var deadLetter = JsonSerializer.Deserialize<JsonElement>(
                result.Message.Value);
            Assert.Equal("KeyNotFoundException", deadLetter.GetProperty("errorType").GetString());
            Assert.Equal(3, deadLetter.GetProperty("attempts").GetInt32());
            return;
        }

        Assert.Fail("Unprocessable payment event was not retried and published to the DLQ.");
    }

    [Fact]
    public async Task Outbox_recovers_order_after_kafka_outage()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("COSMICWIKI_RUN_RECOVERY_TEST"),
                "true",
                StringComparison.OrdinalIgnoreCase))
            return;

        using var client = new HttpClient { BaseAddress = GatewayUri };
        var checkout = await CreateCheckoutAsync(client);
        var kafkaContainer = Environment.GetEnvironmentVariable(
            "COSMICWIKI_KAFKA_CONTAINER") ?? "cosmicwiki-kafka";

        try
        {
            RunDocker("stop", kafkaContainer);
            using var webhook = await SendWebhookAsync(client, checkout, "succeeded");
            Assert.Equal(HttpStatusCode.OK, webhook.StatusCode);
        }
        finally
        {
            RunDocker("start", kafkaContainer);
        }

        var deadline = DateTime.UtcNow.AddSeconds(
            int.TryParse(
                Environment.GetEnvironmentVariable("COSMICWIKI_RECONCILIATION_WAIT_SECONDS"),
                out var waitSeconds)
                ? waitSeconds
                : 45);
        while (DateTime.UtcNow < deadline)
        {
            using var response = await client.GetAsync($"/api/orders/{checkout.OrderId}");
            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadFromJsonAsync<Order>();
                if (order?.Status == "Paid")
                    return;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        Assert.Fail("Outbox publishing did not restore the order to Paid.");
    }

    private static async Task<Checkout> CreateCheckoutAsync(HttpClient client)
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                username = $"integration_{suffix}",
                email = $"integration_{suffix}@example.com",
                password = "SmokeTest@123"
            });
        registerResponse.EnsureSuccessStatusCode();

        var registration = await registerResponse.Content.ReadFromJsonAsync<Registration>();
        Assert.NotNull(registration);

        var loginResponse = await PostLoginWithRateLimitRetryAsync(
            client,
            $"integration_{suffix}");
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<Login>();
        Assert.NotNull(login);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var products = await client.GetFromJsonAsync<ProductPage>(
            "/api/product/products?page=1&pageSize=10");
        var product = products?.Items.FirstOrDefault(
            x => x.Status == 2 && x.StockQuantity > 0);
        Assert.NotNull(product);

        using var orderRequest = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(new
            {
                customerId = registration.UserId,
                currency = "VND",
                lines = new[] { new { productId = product.Id, quantity = 1 } }
            })
        };
        var idempotencyKey = $"integration-{Guid.NewGuid():N}";
        orderRequest.Headers.Add("Idempotency-Key", idempotencyKey);
        var orderResponse = await client.SendAsync(orderRequest);
        orderResponse.EnsureSuccessStatusCode();

        var order = await orderResponse.Content.ReadFromJsonAsync<Order>();
        Assert.NotNull(order);
        Assert.Equal("PendingPayment", order.Status);

        return new Checkout(
            registration.UserId,
            product.Id,
            order.OrderId,
            order.PaymentIntentId,
            order.TotalAmount,
            order.Currency,
            idempotencyKey,
            $"evt_{Guid.NewGuid():N}");
    }

    private static async Task<bool> IsGatewayReadyAsync(HttpClient client)
    {
        foreach (var path in new[]
                 {
                     "/api/auth/login",
                     "/api/product/products",
                     "/api/orders",
                     "/api/payments/webhooks"
                 })
        {
            try
            {
                using var response = await client.GetAsync(path);
                if (response.StatusCode is
                    HttpStatusCode.BadGateway or
                    HttpStatusCode.ServiceUnavailable or
                    HttpStatusCode.GatewayTimeout)
                    return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        return true;
    }

    private static void RunDocker(params string[] arguments)
    {
        using var process = System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = string.Join(" ", arguments.Select(QuoteArgument)),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }) ?? throw new InvalidOperationException("Unable to start Docker.");

        if (!process.WaitForExit(15_000))
            throw new TimeoutException("Docker command timed out.");
        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"Docker command failed: {process.StandardError.ReadToEnd()}");
    }

    private static string QuoteArgument(string argument) =>
        argument.Contains(' ') ? $"\"{argument.Replace("\"", "\\\"")}\"" : argument;

    private static async Task<HttpResponseMessage> PostLoginWithRateLimitRetryAsync(
        HttpClient client,
        string username)
    {
        for (var attempt = 1; ; attempt++)
        {
            var response = await client.PostAsJsonAsync(
                "/api/auth/login",
                new { username, password = "SmokeTest@123" });
            if (response.StatusCode != HttpStatusCode.TooManyRequests ||
                attempt >= 4)
                return response;

            var delay = response.Headers.RetryAfter?.Delta
                ?? TimeSpan.FromSeconds(2);
            response.Dispose();
            await Task.Delay(delay);
        }
    }

    private static async Task<HttpResponseMessage> SendWebhookAsync(
        HttpClient client,
        Checkout checkout,
        string status,
        string? eventId = null)
    {
        var payload = JsonSerializer.Serialize(new
        {
            providerEventId = eventId ?? checkout.EventId,
            orderId = checkout.OrderId,
            paymentIntentId = checkout.PaymentIntentId,
            amount = checkout.TotalAmount,
            currency = checkout.Currency,
            status
        });

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(WebhookSecret));
        var signature = Convert.ToHexString(
                hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{payload}")))
            .ToLowerInvariant();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/payments/webhooks")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Payment-Timestamp", timestamp);
        request.Headers.Add("X-Payment-Signature", signature);
        return await client.SendAsync(request);
    }

    private sealed record Registration(Guid UserId);
    private sealed record Login(string AccessToken);
    private sealed record ProductPage(IReadOnlyList<Product> Items);
    private sealed record Product(Guid Id, int Status, int StockQuantity);
    private sealed record Order(
        Guid OrderId,
        string PaymentIntentId,
        decimal TotalAmount,
        string Currency,
        string Status);
    private sealed record Checkout(
        Guid CustomerId,
        Guid ProductId,
        Guid OrderId,
        string PaymentIntentId,
        decimal TotalAmount,
        string Currency,
        string IdempotencyKey,
        string EventId);
}
