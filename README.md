# CosmicWiki

## Deployment configuration

- Set `FRONTEND_ORIGIN` to the exact browser origin (scheme, hostname, and optional port) used to access the frontend. The local Docker Compose default is `http://localhost:4200`; deployments outside Development must set `Cors__AllowedOrigins__0` (and further indexed entries if needed) to HTTPS origins. Both Gateway and Auth use this allowlist for credentialed CORS requests.
- Every API exposes an unauthenticated liveness probe at `/health/live`. Ordering, Payments, and Visit also expose `/health` with their existing database/message-broker dependency checks; use liveness for process probes and dependency checks where configured.
- Production must also provide the existing authentication signing keys, HTTPS password-reset URL, SMTP settings, Stripe secret and webhook keys, and service secrets through the deployment secret manager or environment; do not use local Compose defaults in production. For local Compose payment-provider testing, set `STRIPE_SECRET_KEY` in the ignored `.env` file.
- The Stories chat widget uses a self-hosted Ollama model (`qwen2.5:1.5b` by default), so prompts are sent to the Ollama container in the CosmicWiki network instead of an external AI API. To enable it with Docker Compose, start the `ai` profile using `docker compose --profile ai up -d ollama`, then download the model once with `docker compose exec ollama ollama pull qwen2.5:1.5b`. Set `AI_CHAT_MODEL` to use another Ollama model. Stories also reads currently published, in-stock products from the internal Product API to ground recommendations; the model is instructed not to invent products, prices, or stock. AI chat history remains in the browser and is not saved to the Stories database. Running a model requires additional host memory and CPU; the 1.5B model is intended as a low-resource starting point, not a model trained from scratch.
- The APIs emit structured JSON logs to stdout/stderr. Configure the container platform to collect these streams and create alerts from `/health` checks and service error logs; this repository does not provision an external log or alerting service.
- The frontend shows an online/offline banner and does not cache authenticated API responses or checkout data for offline use. Avoid adding offline transaction support unless a secure synchronization and conflict policy is defined.

## Oracle backup and restore

- The Compose Oracle volume persists data across container recreation, but it is not a backup. Take an Oracle Data Pump full export regularly and copy the dump off the Docker host. Choose unique dump/log filenames for each backup:

  ```powershell
  docker compose exec oracle expdp system@FREEPDB1 full=y directory=DATA_PUMP_DIR dumpfile=cosmicwiki_20261029_%U.dmp logfile=cosmicwiki_20261029.log filesize=2G
  ```

  Enter the Oracle `SYSTEM` password when prompted. Query `SELECT directory_path FROM dba_directories WHERE directory_name = 'DATA_PUMP_DIR';` in `sqlplus system@FREEPDB1`, then copy every matching dump piece and the log from that container path with `docker cp cosmicwiki-oracle:<directory-path>/<filename> .\backups\`. Keep backups encrypted and off-host.
- Restore only into a clean, isolated Oracle Free instance/PDB. Copy all dump pieces into its Data Pump directory, then run `impdp system@FREEPDB1 full=y directory=DATA_PUMP_DIR dumpfile=cosmicwiki_20261029_%U.dmp logfile=cosmicwiki_restore.log`. Confirm the imported schemas and application migrations before directing traffic to the restored instance. Never run a full import against the active database.
- Schedule and periodically rehearse both export and isolated restore; verify that the dump set contains all pieces and record recovery-point and recovery-time results. Docker volume persistence alone does not provide point-in-time recovery.

## Local stack verification

After starting Docker Desktop, build and start the complete stack with `docker compose up --build -d`, wait for the Oracle/Kafka/Redis health checks, and confirm the APIs and browser UI are ready. Run the backend regression suite with `dotnet test backend\CosmicWiki.sln`; checkout integration tests require the local Compose services at their configured ports. Stop the stack with `docker compose down` (without `-v` to retain local data).

## Account recovery and delivery

- Outside Development, configure `PASSWORD_RESET_EMAIL_ENABLED=true`, SMTP settings (`ORDER_EMAIL_SMTP_HOST`, `ORDER_EMAIL_SMTP_USERNAME`, `ORDER_EMAIL_SMTP_PASSWORD`, `ORDER_EMAIL_FROM_ADDRESS`), and the public HTTPS `PASSWORD_RESET_FRONTEND_URL`. For local testing without SMTP, explicitly enable `PASSWORD_RESET_EXPOSE_DEVELOPMENT_LINK=true`; this option is ignored outside Development.
- Create a normal account, then use Admin → Accounts → User to assign the `Delivery` role. A delivery account can claim paid orders from `/delivery`; location sharing requires browser geolocation permission and HTTPS (or localhost). Customers see the latest reported position on their order page.

## Ordering and Product service boundary

- Ordering reads product snapshots and reserves, confirms, releases, and expires stock through Product's `/internal/product` API using the shared `PRODUCT_SERVICE_KEY` secret. These routes are not part of the public `/api/product` gateway route.
- Each stock operation is transactional and idempotent within Product, keyed by order ID. Ordering and Product do not share a distributed transaction: Product commits the reservation separately from Ordering's order/payment persistence, and Ordering compensates with an idempotent release when its create flow fails. A process failure between service commits can leave a reservation temporarily active; Product's 15-minute expiry and Ordering's reaper release expired reservations.