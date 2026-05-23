# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0
- **Scope**: Entire solution
- **Update NuGet Packages**: Yes — update all packages to latest compatible versions

## User Preferences
### Technical Preferences
- **Port & Scalar config**: Should match C:\repos\git\TeamPnC-ToolBox\TeamPnC-Toolbox-Api (same port, same Scalar/OpenAPI UI setup)
- **Authentication**: Same as TeamPnC-Toolbox-Api — Azure AD with Microsoft.Identity.Web + JwtBearer (`AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(config.GetSection("AzureAd"))`)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: All-at-Once

## Strategy
**Selected**: All-at-Once
**Rationale**: 6 projects all on net9.0, shallow dependency graph, simple TFM bump

### Execution Constraints
- Single atomic upgrade — all projects updated together
- Validate full solution build after upgrade
- Update all NuGet packages to latest compatible versions for net10.0
