using System.Text.Json.Serialization;
using SchoolSaaS.Api.Api.V1;
using SchoolSaaS.Api.Health;
using SchoolSaaS.Api.DependencyInjection;
using SchoolSaaS.Api.Infrastructure;
using SchoolSaaS.Api.Middleware;
using SchoolSaaS.Application;
using SchoolSaaS.Infrastructure;
using SchoolSaaS.Infrastructure.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddApiVersioningServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment()
    && !app.Configuration.GetValue<bool>("Testing:SkipDataSeed"))
{
    await IdentityDataSeeder.SeedAsync(app.Services);
}

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSwaggerDocumentation();

app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

app.MapGet("/", () => "School SaaS API");
app.MapHealthEndpoints();
app.MapV1Endpoints();

app.Run();

public partial class Program;
