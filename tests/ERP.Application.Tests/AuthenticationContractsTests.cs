using System;
using System.Collections.Generic;
using System.Linq;
using ERP.Application.Abstractions;
using ERP.Application.Features.Authentication;
using Xunit;

namespace ERP.Application.Tests;

public class AuthenticationContractsTests
{
    [Fact]
    public void UserAuthenticationData_HasAllProperties()
    {
        var now = DateTime.UtcNow;
        var data = new UserAuthenticationData(
            Id: 1,
            Username: "admin",
            PasswordHash: "$2a$11$...",
            NombreCompleto: "Admin User",
            Rol: "Administrador Sistema",
            Activo: true,
            IntentosFallidos: 0,
            BloqueadoHasta: now);

        Assert.Equal(1, data.Id);
        Assert.Equal("admin", data.Username);
        Assert.Equal("$2a$11$...", data.PasswordHash);
        Assert.Equal("Admin User", data.NombreCompleto);
        Assert.Equal("Administrador Sistema", data.Rol);
        Assert.True(data.Activo);
        Assert.Equal(0, data.IntentosFallidos);
        Assert.Equal(now, data.BloqueadoHasta);
    }

    [Fact]
    public void UserAuthenticationData_AllowsNullBloqueadoHasta()
    {
        var data = new UserAuthenticationData(
            Id: 1,
            Username: "admin",
            PasswordHash: "hash",
            NombreCompleto: "Admin",
            Rol: "Administrador Sistema",
            Activo: true,
            IntentosFallidos: 0,
            BloqueadoHasta: null);

        Assert.Null(data.BloqueadoHasta);
    }

    [Fact]
    public void LoginAttemptRecord_HasAllProperties()
    {
        var record = new LoginAttemptRecord(
            UsuarioId: 1,
            Username: "admin",
            Exitoso: true,
            IpOrigen: "127.0.0.1",
            CorrelationId: "corr-123");

        Assert.Equal(1, record.UsuarioId);
        Assert.Equal("admin", record.Username);
        Assert.True(record.Exitoso);
        Assert.Equal("127.0.0.1", record.IpOrigen);
        Assert.Equal("corr-123", record.CorrelationId);
    }

    [Fact]
    public void LoginAttemptRecord_AllowsNullUsuarioId()
    {
        var record = new LoginAttemptRecord(
            UsuarioId: null,
            Username: "unknown",
            Exitoso: false,
            IpOrigen: null,
            CorrelationId: "corr-456");

        Assert.Null(record.UsuarioId);
        Assert.Null(record.IpOrigen);
    }

    [Fact]
    public void AuditEventRecord_HasAllProperties()
    {
        var datos = new Dictionary<string, object> { { "key", "value" } };
        var record = new AuditEventRecord(
            UsuarioId: 1,
            RolActor: "Administrador Sistema",
            Modulo: "Authentication",
            Accion: "Login",
            Entidad: "Usuario",
            EntidadId: "1",
            Resultado: "Success",
            Datos: datos,
            CorrelationId: "corr-789");

        Assert.Equal(1, record.UsuarioId);
        Assert.Equal("Administrador Sistema", record.RolActor);
        Assert.Equal("Authentication", record.Modulo);
        Assert.Equal("Login", record.Accion);
        Assert.Equal("Usuario", record.Entidad);
        Assert.Equal("1", record.EntidadId);
        Assert.Equal("Success", record.Resultado);
        Assert.Equal("corr-789", record.CorrelationId);
        Assert.True(record.FechaEvento <= DateTime.UtcNow);
    }

    [Fact]
    public void AuditEventRecord_AllowsNullUsuarioIdAndRolActor()
    {
        var record = new AuditEventRecord(
            UsuarioId: null,
            RolActor: null,
            Modulo: "Authentication",
            Accion: "LoginAttempt",
            Entidad: "Usuario",
            EntidadId: null,
            Resultado: "Failure",
            Datos: new Dictionary<string, object>(),
            CorrelationId: "corr-000");

        Assert.Null(record.UsuarioId);
        Assert.Null(record.RolActor);
        Assert.Null(record.EntidadId);
    }

    [Fact]
    public void AllAuthenticationInterfaces_ExistInApplicationAssembly()
    {
        var assembly = typeof(IJwtTokenService).Assembly;
        var interfaces = new[]
        {
            typeof(IAuthenticationRepository),
            typeof(ITokenRevocationRepository),
            typeof(IAuditWriter),
            typeof(IPasswordHasher)
        };

        foreach (var interfaceType in interfaces)
        {
            var found = assembly.GetTypes().Any(t => t == interfaceType);
            Assert.True(found, $"Interface {interfaceType.Name} not found in Application assembly");
        }
    }

    [Fact]
    public void AllAuthenticationRecords_ExistInApplicationAssembly()
    {
        var assembly = typeof(IJwtTokenService).Assembly;
        var records = new[]
        {
            typeof(UserAuthenticationData),
            typeof(LoginAttemptRecord),
            typeof(AuditEventRecord),
            typeof(AuthStateUpdateResult)
        };

        foreach (var recordType in records)
        {
            var found = assembly.GetTypes().Any(t => t == recordType);
            Assert.True(found, $"Record {recordType.Name} not found in Application assembly");
        }
    }
}
