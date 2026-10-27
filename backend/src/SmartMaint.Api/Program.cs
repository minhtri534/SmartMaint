using SmartMaint.Api.Common.Configuration;
using SmartMaint.Api.Common.Extensions;
using SmartMaint.Api.Data.Seed;
using SmartMaint.Api.Modules;

DotEnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Railway injects PORT; locally the port comes from launchSettings.json.
if (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services
    .AddModules()
    .AddDatabase(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi().AllowAnonymous();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "SmartMaint API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors(ApiServiceCollectionExtensions.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();
