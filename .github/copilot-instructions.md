# Copilot Instructions

## Project Guidelines
- BackOffice-Api should use the same port and Scalar (OpenAPI UI) configuration as C:\repos\git\TeamPnC-ToolBox\TeamPnC-Toolbox-Api
- The main production database connection for BackOffice-Api is: sql_van_test_accp.iap.iafg.net,49826. The testing database name is DPACCP on this server (Note: This is a testing database, not production).
- A local PostgreSQL database running in Docker is used to store logs, chat history, and other runtime/operational data for BackOffice-Api. The SQL Server database (DPACCP on sql_van_test_accp.iap.iafg.net,49826) is for business/testing data, while PostgreSQL is for operational data. 
- Local PostgreSQL (Docker) connection string for BackOffice-Api operational data: Host=localhost;Port=5432;Database=backoffice;Username=admin;Password=YOUR_PASSWORD