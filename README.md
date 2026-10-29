# CosmicWiki

## Deployment configuration

- Set `FRONTEND_ORIGIN` to the exact browser origin (scheme, hostname, and optional port) used to access the frontend. The local Docker Compose default is `http://localhost:4200`; deployments outside Development must set `Cors__AllowedOrigins__0` (and further indexed entries if needed) to HTTPS origins. Both Gateway and Auth use this allowlist for credentialed CORS requests.
- Every API exposes an unauthenticated liveness probe at `/health/live`. Ordering, Payments, and Visit also expose `/health` with their existing database/message-broker dependency checks; use liveness for process probes and dependency checks where configured.
- Production must also provide the existing authentication signing keys, HTTPS password-reset URL, SMTP settings, Stripe secret and webhook keys, and service secrets through the deployment secret manager or environment; do not use local Compose defaults in production. For local Compose payment-provider testing, set `STRIPE_SECRET_KEY` in the ignored `.env` file.
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