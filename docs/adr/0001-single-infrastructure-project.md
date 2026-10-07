# One infrastructure project holds both data access and identity adapters

_Superseded by [ADR-0006](0006-identity-stack-removed.md): the identity stack was removed entirely,
so there are no identity adapters left to locate inside this project._

`MySite.Infrastructure.Postgres` contains the EF Core context, its configurations and the
repositories — and also `JwtProvider`, `PasswordHasher` and `JwtOptions`, which have nothing to do
with PostgreSQL. The harness rules say `Infrastructure.*` is "split by technology", so the name
promises one technology while the assembly holds two. A single infrastructure project is kept on
purpose while the codebase is this small: two classes do not earn a third project, and the
adapters already sat next to data access in the original code.

## Considered options

- **Split `MySite.Infrastructure.Identity` out** — the literal reading of the rule, and the way
  back when a second identity concern appears. Rejected for now as one more project for two
  classes.
- **Rename the project to `MySite.Infrastructure`** — accurate and the cheapest way to close the
  gap; not chosen because the name was fixed when the project was laid out.
- **Keep one project under the Postgres name** — chosen, with the mismatch written down here so
  it reads as deliberate rather than as an oversight.

## Consequences

Someone looking for the JWT code under an `Infrastructure.Identity` name will not find it. The
deviation is listed in `ARCHITECTURE.md`, and it should be closed by the rename or the split
before a third technology adapter lands in this assembly.
