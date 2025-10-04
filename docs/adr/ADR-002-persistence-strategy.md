# ADR-002: Persistence Strategy (Neon + EF Core)

- Status: Accepted
- Date: 2025-10-03

## Context
We need a reliable, low‑maintenance relational database for the Enrollment API with:
- Managed hosting and easy scaling for low-cost hobby use and future growth.
- Strong Postgres support from .NET and mature tooling.
- Developer-friendly workflows (branch/preview environments, migrations, local dev).

The runtime is deployed on Fly.io. We want the database to be accessible from Fly with minimal ops overhead and a clear local development story.

## Decision
- Use Neon (https://neon.tech) as the managed Postgres provider.
  - Serverless, auto‑scaling Postgres with a generous free tier.
  - Branching support for preview databases mapped to app feature branches if needed.
  - Native Postgres; compatible with Npgsql and EF Core.
- Use Entity Framework Core with the Npgsql provider for data access and schema migrations.
  - Apply automatic migrations on app startup (see Program.cs) for simplified deployment.
  - Prefer additive migrations; avoid destructive changes without an explicit migration plan.

Configuration:
- Connection string is read from:
  1) ConnectionStrings:DefaultConnection, or
  2) DATABASE_URL environment variable (Neon connection URL)
- Production (Fly.io): set a Fly secret with the Neon connection string, for example:
  - fly secrets set DATABASE_URL="Host=...;Username=...;Password=...;Database=...;Ssl Mode=Require;Trust Server Certificate=true"
- Local development: use a local Postgres or a Neon branch connection string in user-secrets.

## Alternatives Considered
- Roll-your-own Postgres on a VM: More control but higher ops burden, backups, patching.
- SQLite: Simple, but insufficient for concurrency, migrations in prod, and cloud scaling.
- Cosmos/NoSQL: Not required; relational model fits enrollments well and EF Core integration is better with Postgres.

## Consequences
- Developer Experience
  - Simple: EF Core migrations + Neon branches enable fast iteration.
  - Clear environment separation via different connection strings.
- Operations
  - Minimal DB management; Neon handles scaling and storage.
  - Startup auto-migrate reduces manual steps but requires care in versioning.
- Risks
  - Auto-migration on startup can surprise if incompatible changes are deployed; mitigate via review and tests.
  - Neon cold starts on free tier may add initial latency; acceptable for this project.

## Links
- Fly app config: src/Enrollment.Api/fly.toml (app: easv-enrollment)
- EF Core setup: src/Enrollment.Api/Program.cs
- DbContext model: src/Enrollment.Api/EnrollmentDbContext.cs
