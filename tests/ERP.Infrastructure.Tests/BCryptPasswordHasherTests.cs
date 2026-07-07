using ERP.Infrastructure.Services;
using Xunit;

namespace ERP.Infrastructure.Tests;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyString()
    {
        var hash = _hasher.HashPassword("password123");

        Assert.NotEmpty(hash);
        Assert.StartsWith("$2", hash);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        var hash = _hasher.HashPassword("correctPassword");

        var isValid = _hasher.VerifyPassword("correctPassword", hash);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var hash = _hasher.HashPassword("correctPassword");

        var isValid = _hasher.VerifyPassword("wrongPassword", hash);

        Assert.False(isValid);
    }

    [Fact]
    public void HashPassword_ProducesDifferentHashesForSameInput()
    {
        var hash1 = _hasher.HashPassword("samePassword");
        var hash2 = _hasher.HashPassword("samePassword");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForEmptyPassword()
    {
        var hash = _hasher.HashPassword("notEmpty");

        var isValid = _hasher.VerifyPassword("", hash);

        Assert.False(isValid);
    }

    [Fact]
    public void HashPassword_ThrowsForNullPassword()
    {
        Assert.Throws<ArgumentNullException>(() => _hasher.HashPassword(null!));
    }

    [Fact]
    public void VerifyPassword_ThrowsForNullPassword()
    {
        var hash = _hasher.HashPassword("test");

        Assert.Throws<ArgumentNullException>(() => _hasher.VerifyPassword(null!, hash));
    }

    [Fact]
    public void VerifyPassword_ThrowsForNullHash()
    {
        Assert.Throws<ArgumentNullException>(() => _hasher.VerifyPassword("password", null!));
    }
}
