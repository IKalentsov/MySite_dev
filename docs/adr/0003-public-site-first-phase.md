# The first phase is a public site without authentication

_Superseded by [ADR-0006](0006-identity-stack-removed.md): the identity code that this ADR was
about keeping dormant no longer exists; authentication will be written from scratch when the
admin panel's first write scenario arrives._

The site is being built locally, for its owner, and nothing about it is published yet. The first
phase therefore ships with every page public: no sign-in, no per-request validation, no
authorisation checks. Authentication arrives later, together with the administration panel the
owner will use to manage the site's content — that panel is the first thing on the site that
genuinely needs an identity, which is why the two are planned together.

## Considered options

- **Wire authentication now and put the public pages behind it** — rejected: it would place a
  login in front of content that has no reason to be protected, and slow down the work that
  actually defines the site.
- **Delete the identity code and write it again with the panel** — rejected: `JwtProvider`,
  `PasswordHasher`, `UserRight` and the user table already exist and are the natural seed of the
  panel.
- **Keep the identity code in place, unwired, and ship public pages** — chosen.

## Consequences

The codebase holds identity pieces that nothing calls: `Program.cs` has no `AddAuthentication`, no
`AddJwtBearer` and no `UseAuthentication`, `JwtOptions` is not bound to configuration, and the
register and login endpoints that exist are not part of this phase. A reader will meet that
dormant code, and this record is why it is there — it is not a half-finished feature to complete
on the way past.

While nobody is signed in, no rule that needs a current user is active, and nothing on the public
site may depend on one.
