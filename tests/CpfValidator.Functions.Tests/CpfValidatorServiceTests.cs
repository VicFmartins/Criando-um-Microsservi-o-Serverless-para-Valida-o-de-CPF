using CpfValidator.Functions.Services;
using Xunit;

namespace CpfValidator.Functions.Tests;

public sealed class CpfValidatorServiceTests
{
    private readonly CpfValidatorService _service = new();

    [Theory]
    [InlineData("11144477735")]
    [InlineData("111.444.777-35")]
    [InlineData("935.411.347-80")]
    public void IsValid_WhenCpfIsValid_ReturnsTrue(string cpf)
    {
        var result = _service.IsValid(cpf);
        Assert.True(result);
    }

    [Theory]
    [InlineData("12345678901")]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")]
    public void IsValid_WhenCpfIsInvalid_ReturnsFalse(string? cpf)
    {
        var result = _service.IsValid(cpf);
        Assert.False(result);
    }

    [Fact]
    public void Normalize_RemovesFormattingCharacters()
    {
        var normalized = _service.Normalize("111.444.777-35");
        Assert.Equal("11144477735", normalized);
    }
}
