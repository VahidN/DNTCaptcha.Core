# Backend regression checks

Run `dotnet run --project tests/DNTCaptcha.BackendTests` from the repository root.
This executable suite starts a local Kestrel server on a dynamically assigned
loopback port and exits nonzero on failure. It requires .NET 10 and no test
framework packages.

Checks cover WAV headers and PCM ordering for both voices, the optional assembly
resource boundary, invalid inputs, repeat playback without consuming the answer,
browser binding, tampering, expiration, challenge invalidation on refresh,
successful validation after playback, existing visual modes, and HTTP rate limits.
