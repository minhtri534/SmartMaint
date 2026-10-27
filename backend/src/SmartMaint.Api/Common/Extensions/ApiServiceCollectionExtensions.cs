using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SmartMaint.Api.Common.Errors;
using SmartMaint.Api.Common.Security;
using SmartMaint.Api.Common.Validation;
using SmartMaint.Api.Modules.Auth;

namespace SmartMaint.Api.Common.Extensions;

/// <summary>HTTP + security wiring (controllers, JSON, errors, CORS, JWT, OpenAPI). Program.cs only calls these methods.</summary>
internal static class ApiServiceCollectionExtensions
{
    public const string CorsPolicy = "web";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddOptions<AuthOptions>().Bind(configuration.GetSection(AuthOptions.SectionName));
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services
            .AddControllers(options =>
            {
                options.Filters.Add<PasswordChangeRequiredFilter>();
                options.Filters.Add<ValidationFilter>();
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(EnumConverter()));
        // The OpenAPI generator reads these options, so the schema shows the same string enums.
        services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(EnumConverter()));

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddCors(options => options.AddPolicy(CorsPolicy, policy => policy
            .WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()));

        services.AddJwtAuthentication();
        services.AddAuthorizationBuilder()
            // Secure by default: every endpoint needs a signed-in user unless marked [AllowAnonymous].
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "SmartMaint API";
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            };
            document.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = [],
                },
            ];
            return Task.CompletedTask;
        }));

        return services;
    }

    // Enums travel as the same snake_case text stored in the DB: "admin", "customer", ...
    private static JsonStringEnumConverter EnumConverter() => new(JsonNamingPolicy.SnakeCaseLower);

    private static void AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    NameClaimType = AppClaims.Name,
                    RoleClaimType = AppClaims.Role,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
                bearer.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        // AUTH-05: the client shows the "session expired" overlay on this code.
                        var expired = context.AuthenticateFailure is SecurityTokenExpiredException;
                        return ProblemFactory.WriteAsync(
                            context.HttpContext, StatusCodes.Status401Unauthorized,
                            expired ? "auth.session_expired" : "auth.unauthenticated",
                            expired ? "Your session has expired. Please sign in again." : "Please sign in.");
                    },
                    OnForbidden = context =>
                        // ERR-01: the role is not allowed to use this endpoint.
                        ProblemFactory.WriteAsync(
                            context.HttpContext, StatusCodes.Status403Forbidden, "auth.forbidden",
                            "You do not have permission to perform this action."),
                };
            });
    }
}
