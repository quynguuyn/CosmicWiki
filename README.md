# CosmicWiki

## Account recovery and delivery

- Outside Development, configure `PASSWORD_RESET_EMAIL_ENABLED=true`, SMTP settings (`ORDER_EMAIL_SMTP_HOST`, `ORDER_EMAIL_SMTP_USERNAME`, `ORDER_EMAIL_SMTP_PASSWORD`, `ORDER_EMAIL_FROM_ADDRESS`), and the public HTTPS `PASSWORD_RESET_FRONTEND_URL`. For local testing without SMTP, explicitly enable `PASSWORD_RESET_EXPOSE_DEVELOPMENT_LINK=true`; this option is ignored outside Development.
- Create a normal account, then use Admin → Accounts → User to assign the `Delivery` role. A delivery account can claim paid orders from `/delivery`; location sharing requires browser geolocation permission and HTTPS (or localhost). Customers see the latest reported position on their order page.