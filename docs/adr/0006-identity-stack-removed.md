# Identity stack removed from the codebase

The identity code (`JwtProvider`, `PasswordHasher`, `UsersService`, `UserController`, all
identity request/response DTOs, the `User` entity, and the `UserRight` enum) was deleted in
ticket 01. The migration was regenerated without the `users` table; `appsettings.json` now holds
an empty placeholder for `ConnectionStrings:DefaultConnection` with a startup check that stops the
application if the string is not provided.

This decision supersedes [ADR-0003](0003-public-site-first-phase.md): the rationale for keeping
dormant identity code as the seed of the administration panel no longer applies because there is
nothing dormant to keep. Authentication will be written from scratch when the first write scenario
for the admin panel is implemented — owner sign-in is the expected trigger.

## Consequences

`Program.cs` contains no authentication middleware, no `AddAuthentication`, and no `AddJwtBearer`.
The DI composition lives in layer-specific extensions (`AddProgramDependencies` chains
`AddWebDependencies` and `AddInfrastructure`), and health checks are split by tags into `/health/live`
and `/health/ready`. The deviation recorded in [ADR-0001](0001-single-infrastructure-project.md) about
identity adapters living inside the Postgres infrastructure project is resolved — there are no more
identity adapters to locate.
