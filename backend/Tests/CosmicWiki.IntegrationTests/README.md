# CosmicWiki integration tests

These tests exercise the public checkout contract through the Gateway.

Start the local infrastructure first. The repository compose file only
manages Oracle and Kafka; the eight APIs run from their individual projects.
Set `ORACLE_PASSWORD`, `ORACLE_APP_USER_PASSWORD`, and any required schema
credentials in the shell before starting Compose:

```powershell
$env:ORACLE_PASSWORD = "..."
$env:ORACLE_APP_USER_PASSWORD = "..."
docker compose up -d oracle kafka
```

Then start Auth, Product, Ordering, Payments (`http://localhost:5007`),
Gateway, Catalog, Discovery, and Visit on ports `5000`-`5007`, and run:

```powershell
dotnet test Tests\CosmicWiki.IntegrationTests\CosmicWiki.IntegrationTests.csproj
```

Optional environment variables:

- `COSMICWIKI_GATEWAY_URL` (default: `http://localhost:5000`)
- `COSMICWIKI_WEBHOOK_SECRET` (default: `smoke-secret`)

Product read caching is disabled by default for local Development. To enable
Redis-backed product list/detail caching, set `Caching__Enabled=true` and
`Caching__Redis__ConnectionString=localhost:6379` for the Product API.

For non-development deployments, configure secrets outside `appsettings.json`:

- `JWT_PRIVATE_KEY_PEM`
- `ORDERING_SERVICE_KEY`
- `PaymentWebhook__SharedSecret`
- `StripeWebhook__SigningSecret`
- `Kafka__SaslUsername`
- `Kafka__SaslPassword`
- `Kafka__SslCaLocation`

Production startup rejects missing payment/JWT credentials, HTTP
Payments-to-Ordering URLs, and unauthenticated Kafka (`Plaintext`). Local
Development may use the checked-in placeholders and local Kafka listener.
