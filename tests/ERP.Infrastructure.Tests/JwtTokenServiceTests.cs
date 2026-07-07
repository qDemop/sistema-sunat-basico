using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using ERP.Infrastructure.Configuration;
using ERP.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ERP.Infrastructure.Tests;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(JwtOptions options)
    {
        return new JwtTokenService(Options.Create(options));
    }

    private static readonly JwtOptions TestOptions = new()
    {
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        Key = "this-is-a-test-key-that-is-at-least-32-characters-long",
        ExpirationMinutes = 60
    };

    [Fact]
    public void GenerateToken_ReturnsTokenAndExpiresAt()
    {
        var service = CreateService(TestOptions);

        var (token, expiresAt) = service.GenerateToken(1, "Test User", "Administrador RRHH", "test-jti");

        Assert.NotEmpty(token);
        Assert.True(expiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_TokenCanBeValidated()
    {
        var service = CreateService(TestOptions);
        var (token, _) = service.GenerateToken(1, "Test User", "Administrador RRHH", "test-jti");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestOptions.Issuer,
            ValidAudience = TestOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(TestOptions.Key))
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        Assert.NotNull(principal);
        Assert.IsType<JwtSecurityToken>(validatedToken);
    }

    [Fact]
    public void GenerateToken_WithExplicitClaimMapping_PreservesSessionClaimsAndRoleAuthorization()
    {
        var service = CreateService(TestOptions);
        var (token, _) = service.GenerateToken(42, "Jane Doe", "Contador", "test-jti");

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = TestOptions.Issuer,
            ValidAudience = TestOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(TestOptions.Key)),
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role"
        };

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(token, validationParameters, out _);

        Assert.Equal("42", principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
        Assert.Equal("Jane Doe", principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value);
        Assert.Equal("test-jti", principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value);
        Assert.Equal("Contador", principal.FindFirst("role")?.Value);
        Assert.Equal("Jane Doe", principal.Identity?.Name);
        Assert.True(principal.IsInRole("Contador"));
    }

    [Fact]
    public void GenerateToken_ContainsUserIdAndNameAndRole()
    {
        var service = CreateService(TestOptions);
        var (token, _) = service.GenerateToken(42, "Jane Doe", "Contador", "test-jti");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var userIdClaim = jwt.Claims.First(c => c.Type == "userId");
        var nameClaim = jwt.Claims.First(c => c.Type == "name");
        var roleClaim = jwt.Claims.First(c => c.Type == "role");

        Assert.Equal("42", userIdClaim.Value);
        Assert.Equal("Jane Doe", nameClaim.Value);
        Assert.Equal("Contador", roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_ContainsJti()
    {
        var service = CreateService(TestOptions);
        var (token, _) = service.GenerateToken(1, "Test User", "Administrador RRHH", "unique-jti-123");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var jtiClaim = jwt.Claims.First(c => c.Type == "jti");
        Assert.Equal("unique-jti-123", jtiClaim.Value);
    }

    [Fact]
    public void GenerateToken_HasExpiration()
    {
        var service = CreateService(TestOptions);
        var (token, expectedExpires) = service.GenerateToken(1, "Test User", "Administrador RRHH", "test-jti");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_UsesConfiguredIssuerAndAudience()
    {
        var service = CreateService(TestOptions);
        var (token, _) = service.GenerateToken(1, "Test User", "Administrador RRHH", "test-jti");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var issuerClaim = jwt.Claims.First(c => c.Type == "iss");
        var audienceClaim = jwt.Claims.First(c => c.Type == "aud");

        Assert.Equal(TestOptions.Issuer, issuerClaim.Value);
        Assert.Equal(TestOptions.Audience, audienceClaim.Value);
    }

    [Fact]
    public void GenerateToken_ThrowsWhenKeyIsEmpty()
    {
        var options = new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = string.Empty,
            ExpirationMinutes = 60
        };
        var service = CreateService(options);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.GenerateToken(1, "Test User", "Administrador RRHH", "test-jti"));

        Assert.Contains("signing key is not configured", ex.Message);
    }

    [Fact]
    public void GenerateToken_ContainsSubClaim()
    {
        var service = CreateService(TestOptions);
        var (token, _) = service.GenerateToken(99, "Test User", "Administrador RRHH", "test-jti");

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var subClaim = jwt.Claims.First(c => c.Type == "sub");
        Assert.Equal("99", subClaim.Value);
    }
}
