namespace CpfValidator.Functions.Models;

public sealed class ErrorResponse
{
    public string Error { get; init; } = string.Empty;
    public DateTime TimestampUtc { get; init; }
}
