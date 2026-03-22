namespace CpfValidator.Functions.Services;

public interface ICpfValidatorService
{
    bool IsValid(string? cpf);
    string Normalize(string? cpf);
}
