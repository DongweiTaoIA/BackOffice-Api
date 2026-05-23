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
