# C-Sweet Compute

C-Sweet Headquarters and Office consume these .NET 10 packages independently from NuGet.

| Package | Contents |
| --- | --- |
| `CSweet.Compute.Contracts` | Compute specifications, protocol messages, JSON encoding, and canonical signing payloads. |
| `CSweet.Compute.Provider` | Dispatch verification, capacity reservations, durable replay and receipts, lease expiry, and guest communication. Depends on `CSweet.Compute.Contracts`. |

Headquarters owns scheduling and business authorization. Office owns the privileged virtualization service, its Windows identity, and Hyper-V operations. The shared libraries preserve the existing compute v1 encoding and on-disk replay format.

## Build and release

Run `dotnet test CSweet.Compute.slnx -c Release` to build both libraries and run the protocol and provider tests. Run `dotnet pack CSweet.Compute.slnx -c Release -o artifacts/packages` to produce both packages.

Both packages take their version from `Directory.Build.props`. Bump it for each release. A change to that file on `main` runs `.github/workflows/publish.yml`. The workflow also accepts a manual dispatch on `main` or a matching `vMAJOR.MINOR.PATCH` tag. It tests the solution and pushes Contracts before Provider.

Configure NuGet trusted publishing for the owning NuGet account, GitHub repository `CrosswiredStudios/CSweet.Compute`, and workflow filename `publish.yml`. Set the GitHub Actions secret `NUGET_USER` to that account's NuGet username. Both package IDs must be publishable by that account. Update the pins in each consumer after the packages are available.
