# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY BackOffice.slnx .
COPY BackOffice.Api/*.csproj BackOffice.Api/
COPY BackOffice.Application/*.csproj BackOffice.Application/
COPY BackOffice.Domain/*.csproj BackOffice.Domain/
COPY BackOffice.Infrastructure/*.csproj BackOffice.Infrastructure/
COPY BackOffice.IntegrationTests/*.csproj BackOffice.IntegrationTests/
COPY BackOffice.UnitTests/*.csproj BackOffice.UnitTests/
RUN dotnet restore
COPY . .
RUN dotnet publish BackOffice.Api/BackOffice.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BackOffice.Api.dll"]
