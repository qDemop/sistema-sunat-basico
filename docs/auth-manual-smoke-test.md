# Authentication Manual Smoke Test

This procedure is for local development only. It verifies the Sprint 1 Authentication backend with a manually seeded unsafe local user.

## Preconditions

- Use a local PostgreSQL database only. Do not run the dev seed against production, staging, QA, demos, or shared data.
- Apply the canonical database bootstrap first, in this order: `database/schema.sql`, `database/indexes.sql`, `database/functions.sql`, `database/procedures.sql`, `database/seeds.sql`, then `database/security.sql`.
- Intentionally and manually run `database/dev-seeds.example.sql` only for the local database that will be used by the API.
- Configure `ConnectionStrings:Postgres` outside git, for example with environment variables or .NET user-secrets.
- Configure `Jwt:Key` outside git, for example with environment variables or .NET user-secrets. Do not use production JWT keys for this smoke test.
- Run the API locally after configuration is in place.
- Use the local/dev-only smoke-test credentials:
  - Username: `devadmin`
  - Password: `ChangeMe.DevOnly.2026!`
- Never use this user, password, or BCrypt hash in production.

Example manual seed execution:

```bash
psql "$ERP_DEV_DATABASE_URL" -f database/dev-seeds.example.sql
```

Set the local API base URL used by the examples:

```bash
API_BASE_URL="http://localhost:5000"
```

## Login

Send a login request with the local/dev-only credentials:

```bash
curl -i -X POST "$API_BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: smoke-login-001" \
  -d '{"username":"devadmin","password":"ChangeMe.DevOnly.2026!"}'
```

Expected result:

- HTTP `200 OK`.
- Response contains `token`, `expiresAt`, `user`, `modules`, and `correlationId`.
- `user.rol` is `Administrador Sistema`.
- `modules` includes the modules available to `Administrador Sistema`.

Save the returned JWT as `TOKEN` for the next steps.

## /me

Call the authenticated session endpoint with the saved token:

```bash
curl -i -X GET "$API_BASE_URL/api/auth/me" \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Correlation-ID: smoke-me-001"
```

Expected result:

- HTTP `200 OK`.
- Response contains the current user session and visible modules.
- `user.rol` is `Administrador Sistema`.

## Logout

Revoke the current JWT `jti`:

```bash
curl -i -X POST "$API_BASE_URL/api/auth/logout" \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Correlation-ID: smoke-logout-001"
```

Expected result:

- HTTP `200 OK`.
- Response contains `revoked: true`, `expiresAt`, and `correlationId`.

## Revoked token check

Use the same token again after logout:

```bash
curl -i -X GET "$API_BASE_URL/api/auth/me" \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Correlation-ID: smoke-revoked-001"
```

Expected result:

- HTTP `401 Unauthorized`.
- The revoked token must not reach the protected `/api/auth/me` use case.

## Troubleshooting

- Wrong JWT key: if authenticated requests return `401`, confirm the API uses the same local `Jwt:Key` for issuing and validating tokens, and that the key is configured outside git.
- Missing DB connection string: if the API fails at startup or login cannot reach PostgreSQL, confirm `ConnectionStrings:Postgres` is configured outside git for the API process.
- Wrong script order: if the dev seed raises the missing role error, rerun the canonical bootstrap order from the Preconditions section before running `database/dev-seeds.example.sql`.
- Dev user locked after repeated wrong passwords: rerun `database/dev-seeds.example.sql` locally to reset `intentos_fallidos` and `bloqueado_hasta` for `devadmin`.
- `401 Unauthorized` on login: confirm the dev seed was run against the same local database used by the API, and confirm the password is exactly `ChangeMe.DevOnly.2026!`.
- `/me` returns `401`: confirm the `Authorization` header is exactly `Bearer <token>` and the token has not expired or already been revoked.
- Token revoked but still accepted: confirm logout used the same token and that the API is connected to the database containing the `identity.token_revocation` row; if it still returns `200`, review JWT revocation check configuration.
- Correlation troubleshooting: use a unique `X-Correlation-ID` per request and inspect API/database logs for that value.
