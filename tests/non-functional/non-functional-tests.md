# Non-Functional Test Specifications

## Performance

| Requirement | Test Specification | Pass Criteria |
|---|---|---|
| RNF-001 | Calculate payroll for 100 active employees. | Results visible in <= 30 seconds. |
| RNF-ELI-01 | Execute API read requests under normal load. | 95th percentile response <= 500 ms, excluding network. |
| RNF-ELI-04 | Simulate 50 concurrent users. | Average response <= 800 ms. |
| SUNAT book performance | Generate book for 1,000 vouchers. | Results visible in <= 60 seconds. |
| UI response | Click, navigate, and change selections in WinForms. | User interactions respond in < 200 ms without window freeze. |

## Security

| Requirement | Test Specification | Pass Criteria |
|---|---|---|
| JWT protection | Call protected endpoint without token. | API returns 401. |
| Expired token | Call protected endpoint with expired token. | API returns 401. |
| Revoked token | Log out and reuse the same unexpired JWT `jti`. | Revocation row remains until expiration and API returns 401. |
| Persistent lockout | Produce three consecutive failures for one username across separate requests/processes. | Counter/attempts persist and account remains blocked for 15 minutes; success later resets the counter. |
| Role restriction | User opens unauthorized module. | Module is hidden or access is denied. |
| Role policy coverage | Compare every protected OpenAPI `x-roles` value with the canonical four-role matrix. | No custom role, missing command restriction, RRHH report access, or Gerente mutation is present. |
| Password hashing | Inspect stored password field. | No plaintext password is stored. |
| Sensitive transport | Connect to database from application. | Connection uses SSL/TLS where configured. |
| Audit actor integrity | Submit a sensitive command while attempting to spoof actor data. | Actor user/role come from JWT, correlation is preserved, and the request DTO has no actor fields. |
| Database least privilege | Connect as `erp_api`, `erp_readonly`, and `erp_migrator`; exercise permitted/denied operations from `database/security.sql`. | Each role can perform only its documented grants and cannot assume another role. |

## Usability and Accessibility

| Requirement | Test Specification | Pass Criteria |
|---|---|---|
| Frequent tasks | Register employee, calculate payroll, view report from dashboard. | Each task takes <= 3 interactions from main screen. |
| Resolution support | Run UI at 1280x720 and 1920x1080. | Text and controls remain readable and usable. |
| Keyboard navigation | Navigate login and core forms using keyboard. | All controls are reachable and actionable. |
| Accessible labels | Inspect controls. | Inputs and actions have accessible labels. |

## Maintainability

| Requirement | Test Specification | Pass Criteria |
|---|---|---|
| Modular architecture | Review module dependencies. | Module changes do not require unrelated module changes. |
| API documentation | Parse every module OpenAPI contract and shared components. | Every operation has unique `operationId`, typed success/error responses, `x-roles`, request schema when data is required, and list pagination metadata. |
| Error handling | Trigger validation and server errors. | Responses include status, message, and correlation ID. |
| API registry parity | Compare `docs/api-contracts.md` method/path pairs with all module OpenAPI files. | Sets are identical; no duplicate or undocumented operation exists. |
| Correlation header coverage | Inspect every protected OpenAPI operation including inherited path parameters. | Every protected operation accepts optional `X-Correlation-ID`; only public login may omit inherited security context. |
| Missing-ID errors | Inspect every operation whose path contains `{id}`. | Each declares typed 404 response. |
| Workflow traceability | Inspect `tests/traceability/workflow-traceability.md`. | WF-001 through WF-064 occur exactly once and each in-scope row references WF-AT acceptance evidence. |

## Compatibility

| Requirement | Test Specification | Pass Criteria |
|---|---|---|
| Windows support | Run on Windows 10/11 with .NET 10 Desktop/ASP.NET Core Runtime. | Application starts and core flows execute. |
| Database support | Connect to PostgreSQL 16. | Application can perform required read/write operations. |
