using ERP.Application.Features.Authentication;
using FluentValidation.TestHelper;
using Xunit;

namespace ERP.Application.Tests;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new LoginCommand("admin", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyUsername_Fails()
    {
        var command = new LoginCommand("", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void WhitespaceUsername_Fails()
    {
        var command = new LoginCommand("   ", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void EmptyPassword_Fails()
    {
        var command = new LoginCommand("admin", "");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void WhitespacePassword_Fails()
    {
        var command = new LoginCommand("admin", "   ");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void UsernameTooLong_Fails()
    {
        var command = new LoginCommand(new string('a', 51), "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void UsernameMaxLength_Passes()
    {
        var command = new LoginCommand(new string('a', 50), "password123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void UsernameMinLength_Passes()
    {
        var command = new LoginCommand("abc", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void UsernameTooShort_Fails()
    {
        var command = new LoginCommand("ab", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void UsernameWithSpecialChars_Fails()
    {
        var command = new LoginCommand("admin@123", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void UsernameWithSpaces_Fails()
    {
        var command = new LoginCommand("admin user", "password123");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void SingleCharPassword_Passes()
    {
        var command = new LoginCommand("admin", "x");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
