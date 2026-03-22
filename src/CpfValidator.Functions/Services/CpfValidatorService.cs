using System.Text.RegularExpressions;

namespace CpfValidator.Functions.Services;

public sealed class CpfValidatorService : ICpfValidatorService
{
    private static readonly Regex NonDigitRegex = new(@"\D", RegexOptions.Compiled);

    public string Normalize(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return string.Empty;
        }

        return NonDigitRegex.Replace(cpf, string.Empty);
    }

    public bool IsValid(string? cpf)
    {
        var normalized = Normalize(cpf);

        if (normalized.Length != 11)
        {
            return false;
        }

        if (normalized.All(digit => digit == normalized[0]))
        {
            return false;
        }

        var firstDigit = CalculateVerifierDigit(normalized[..9], 10);
        if (normalized[9] - '0' != firstDigit)
        {
            return false;
        }

        var secondDigit = CalculateVerifierDigit(normalized[..10], 11);
        return normalized[10] - '0' == secondDigit;
    }

    private static int CalculateVerifierDigit(string baseCpf, int initialWeight)
    {
        var sum = 0;

        for (var index = 0; index < baseCpf.Length; index++)
        {
            sum += (baseCpf[index] - '0') * (initialWeight - index);
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
