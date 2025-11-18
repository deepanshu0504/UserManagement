using LoginandRegisterMVC.Services;
using NUnit.Framework;

namespace LoginandRegisterMVC.UnitTests.Services;

[TestFixture]
public class PasswordHashServiceTests
{
    private PasswordHashService _passwordHashService = null!;

    [SetUp]
    public void Setup()
    {
        _passwordHashService = new PasswordHashService();
    }

    [Test]
    public void HashPassword_WithValidPassword_ReturnsHashedString()
    {
        // Arrange
        const string password = "TestPassword123";

        // Act
        var hashedPassword = _passwordHashService.HashPassword(password);

        // Assert
        Assert.That(hashedPassword, Is.Not.Null);
        Assert.That(hashedPassword, Is.Not.Empty);
        Assert.That(hashedPassword, Is.Not.EqualTo(password));
    }

    [Test]
    public void HashPassword_WithSamePassword_ReturnsConsistentHash()
    {
        // Arrange
        const string password = "TestPassword123";

        // Act
        var hash1 = _passwordHashService.HashPassword(password);
        var hash2 = _passwordHashService.HashPassword(password);

        // Assert
        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public void HashPassword_WithDifferentPasswords_ReturnsDifferentHashes()
    {
        // Arrange
        const string password1 = "TestPassword123";
        const string password2 = "DifferentPassword456";

        // Act
        var hash1 = _passwordHashService.HashPassword(password1);
        var hash2 = _passwordHashService.HashPassword(password2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }
}
