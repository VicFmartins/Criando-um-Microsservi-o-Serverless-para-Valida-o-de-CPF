namespace CpfValidator.Functions.Models;

public sealed class CpfValidationResponse
{
    public string Cpf { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime TimestampUtc { get; init; }
    public string NormalizedCpf { get; init; } = string.Empty;
}
