using System.Net;
using System.Text.Json;
using CpfValidator.Functions.Models;
using CpfValidator.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CpfValidator.Functions.Functions;

public sealed class ValidateCpfFunction
{
    private readonly ILogger<ValidateCpfFunction> _logger;
    private readonly ICpfValidatorService _cpfValidatorService;

    public ValidateCpfFunction(
        ILogger<ValidateCpfFunction> logger,
        ICpfValidatorService cpfValidatorService)
    {
        _logger = logger;
        _cpfValidatorService = cpfValidatorService;
    }

    [Function("ValidateCpf")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "cpf/validate")] HttpRequestData request)
    {
        _logger.LogInformation("Recebida requisicao de validacao de CPF.");

        string requestBody;
        using (var reader = new StreamReader(request.Body))
        {
            requestBody = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return await CreateErrorResponse(request, HttpStatusCode.BadRequest, "Body da requisicao nao pode estar vazio.");
        }

        CpfRequest? payload;
        try
        {
            payload = JsonSerializer.Deserialize<CpfRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return await CreateErrorResponse(request, HttpStatusCode.BadRequest, "JSON invalido.");
        }

        if (string.IsNullOrWhiteSpace(payload?.Cpf))
        {
            return await CreateErrorResponse(request, HttpStatusCode.BadRequest, "CPF nao informado.");
        }

        var normalizedCpf = _cpfValidatorService.Normalize(payload.Cpf);
        var isValid = _cpfValidatorService.IsValid(payload.Cpf);

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new CpfValidationResponse
        {
            Cpf = payload.Cpf,
            NormalizedCpf = normalizedCpf,
            IsValid = isValid,
            Message = isValid ? "CPF valido." : "CPF invalido.",
            TimestampUtc = DateTime.UtcNow
        });

        return response;
    }

    [Function("Health")]
    public HttpResponseData Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData request)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        response.WriteString(JsonSerializer.Serialize(new
        {
            status = "ok",
            service = "cpf-validator",
            timestampUtc = DateTime.UtcNow
        }));
        return response;
    }

    private static async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData request,
        HttpStatusCode statusCode,
        string errorMessage)
    {
        var response = request.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new ErrorResponse
        {
            Error = errorMessage,
            TimestampUtc = DateTime.UtcNow
        });
        return response;
    }
}
