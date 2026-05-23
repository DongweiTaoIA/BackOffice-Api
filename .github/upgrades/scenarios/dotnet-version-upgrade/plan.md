# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade all projects from net9.0 to net10.0
**Scope**: 6 projects — 1 web API, 3 class libraries, 2 test projects

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 6 projects, all on .NET 9, clear dependency structure.

## Tasks

### 01-prerequisites: Verify SDK and toolchain

Verify that the .NET 10 SDK is installed and compatible. Update global.json if present to allow the net10.0 SDK.

**Done when**: `dotnet --version` confirms .NET 10 SDK available; no global.json blocking the upgrade.

---

### 02-upgrade-all-projects: Upgrade TFMs, packages, and fix breaking changes

Update all 6 projects from net9.0 to net10.0. Bump all NuGet packages to their latest compatible versions. Fix any binary or source incompatibilities flagged in the assessment (Api project has Api.0001 and Api.0002 issues).

Projects: BackOffice.Api, BackOffice.Application, BackOffice.Domain, BackOffice.Infrastructure, BackOffice.IntegrationTests, BackOffice.UnitTests.

Assessment signals: BackOffice.Api has binary and source incompatible APIs plus package upgrades needed. BackOffice.IntegrationTests also needs package upgrades. The remaining 4 projects are straightforward TFM bumps.

**Done when**: All projects target net10.0, all packages updated to latest compatible versions, solution builds with zero errors and zero warnings, all tests pass.

---

### 03-final-validation: Full solution validation and cleanup

Run full solution build and test suite. Verify Scalar UI works. Ensure port and authentication configuration is intact per user preferences.

**Done when**: Solution builds clean, all tests pass, application starts and Scalar UI is accessible at configured port.
