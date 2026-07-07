using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ERP.API.Endpoints;
using ERP.Application;
using ERP.Application.Abstractions;
using ERP.Application.Security;
using ERP.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured. Set it in appsettings.json or user-secrets.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ERP.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ERP.WinForms";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrWhiteSpace(jti))
                {
                    context.Fail("Token session identifier is missing.");
                    return;
                }

                var revocationRepository = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenRevocationRepository>();

                if (await revocationRepository.IsTokenRevokedAsync(jti, context.HttpContext.RequestAborted))
                {
                    context.Fail("Token has been revoked.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var role in RoleModuleMapping.GetKnownRoles())
    {
        options.AddPolicy(role, policy => policy.RequireRole(role));
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { name = "ERP.API", status = "bootstrap" }))
    .AllowAnonymous();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .AllowAnonymous();

app.MapAuthEndpoints();

app.Run();
