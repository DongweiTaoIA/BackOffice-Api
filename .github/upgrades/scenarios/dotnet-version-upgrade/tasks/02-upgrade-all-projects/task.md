# 02-upgrade-all-projects: Upgrade TFMs, packages, and fix breaking changes

Update all 6 projects from net9.0 to net10.0. Bump all NuGet packages to their latest compatible versions. Fix any binary or source incompatibilities flagged in the assessment (Api project has Api.0001 and Api.0002 issues).

Projects: BackOffice.Api, BackOffice.Application, BackOffice.Domain, BackOffice.Infrastructure, BackOffice.IntegrationTests, BackOffice.UnitTests.

Assessment signals: BackOffice.Api has binary and source incompatible APIs plus package upgrades needed. BackOffice.IntegrationTests also needs package upgrades. The remaining 4 projects are straightforward TFM bumps.

**Done when**: All projects target net10.0, all packages updated to latest compatible versions, solution builds with zero errors and zero warnings, all tests pass.
