using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using BackOffice.Api.Hubs;
using BackOffice.Domain.Interfaces;
using BackOffice.Application.Services;
using BackOffice.Application.Services.Intents;
using BackOffice.Infrastructure.Services;
using BackOffice.Infrastructure.BackOfficeAdminData;
using BackOffice.Infrastructure.UnifiData;
using MongoDB.Driver;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using NpgsqlTypes;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
var postgresConnectionString = builder.Configuration.GetConnectionString("PostgreSQL");

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "message", new RenderedMessageColumnWriter() },
    { "message_template", new MessageTemplateColumnWriter() },
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    { "timestamp", new TimestampColumnWriter() },
    { "exception", new ExceptionColumnWriter() },
    { "log_event", new LogEventSerializedColumnWriter() },
    { "properties", new PropertiesColumnWriter() }
};

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: postgresConnectionString!,
        tableName: "logs",
        columnOptions: columnWriters,
        needAutoCreateTable: true)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Azure AD authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Configure user ID claim for SignalR
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters.ValidateAudience = false;
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Add CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "https://localhost:5173", "http://localhost:5174", "https://localhost:5174"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register services
builder.Services.AddDbContext<BackOfficeAdminDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));
builder.Services.AddDbContext<UnifiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UnifiDb")));
builder.Services.AddSingleton<IProductRegistry, ProductRegistry>();
builder.Services.AddSingleton<IDealerService, MockDealerService>();

// Chat intent handlers � register new capabilities here. FallbackIntentHandler must be last.
builder.Services.AddSingleton<IChatIntentHandler, EligibilityIntentHandler>();
builder.Services.AddSingleton<IChatIntentHandler, FallbackIntentHandler>();

builder.Services.AddSingleton<IChatService, ChatService>();
builder.Services.AddScoped<IChatStore, ChatStore>();

// MongoDB for Support Tickets
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "backoffice";
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabaseName));
builder.Services.AddSingleton<ISupportTicketStore, SupportTicketStore>();

var app = builder.Build();

// Ensure PostgreSQL database and tables are created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BackOfficeAdminDbContext>();
    db.Database.EnsureCreated();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
