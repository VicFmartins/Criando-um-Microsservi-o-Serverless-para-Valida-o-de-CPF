# Criando-um-Microsservi-o-Serverless-para-Valida-o-de-CPF

Microsserviço Serverless para Validação de CPF
 Descrição do Projeto
Este projeto tem como objetivo desenvolver um microsserviço eficiente, escalável e econômico para validação de CPFs, utilizando arquitetura serverless com Azure Functions. A aplicação será construída com base em princípios modernos de computação em nuvem, garantindo alta disponibilidade, baixo custo operacional e facilidade de manutenção.
Objetivos
•	Eficiência: Validação rápida e precisa de números de CPF
•	Escalabilidade: Capacidade de atender milhares de requisições simultâneas
•	Economia: Modelo de pagamento por uso (pay-per-use)
•	Disponibilidade: Serviço altamente disponível na nuvem Azure
•	Manutenibilidade: Código limpo e bem estruturado
 Arquitetura
Componentes Principais
•	Azure Functions: Serviço serverless para execução das funções
•	HTTP Trigger: Endpoint REST para receber requisições de validação
•	Azure Storage Account: Armazenamento necessário para o funcionamento das Functions
•	Application Insights: Monitoramento e telemetria da aplicação
Fluxo de Funcionamento
1.	Cliente envia requisição HTTP POST com o CPF
2.	Azure Functions processa a requisição através do HTTP Trigger
3.	Algoritmo de validação de CPF é executado
4.	Resposta JSON é retornada com o resultado da validação
🛠️ Ferramentas Necessárias
Ambiente de Desenvolvimento
•	Visual Studio Code ou Visual Studio 2022
•	.NET 8 SDK
•	Azure CLI
•	Azure Functions Core Tools
Recursos Azure
•	Conta Azure (gratuita ou paga)
•	Azure Functions App
•	Azure Storage Account
•	Application Insights (opcional, mas recomendado)
 Instalação das Ferramentas
1. Instalação do .NET 8 SDK
# Windows (via winget)
winget install Microsoft.DotNet.SDK.8

# macOS (via Homebrew)
brew install dotnet

# Linux (Ubuntu/Debian)
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0

2. Instalação da Azure CLI
# Windows (via winget)
winget install Microsoft.AzureCLI

# macOS (via Homebrew)
brew install azure-cli

# Linux (via curl)
curl -sL https://aka.ms/InstallAzureCLI | sudo bash

3. Instalação do Azure Functions Core Tools
# Windows (via npm)
npm i -g azure-functions-core-tools@4 --unsafe-perm true

# Windows (via winget)
winget install Microsoft.Azure.FunctionsCoreTools

# macOS (via Homebrew)
brew tap azure/functions
brew install azure-functions-core-tools@4

# Linux (Ubuntu/Debian)
curl https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.gpg
sudo mv microsoft.gpg /etc/apt/trusted.gpg.d/microsoft.gpg
sudo sh -c 'echo "deb [arch=amd64] https://packages.microsoft.com/repos/microsoft-ubuntu-$(lsb_release -cs)-prod $(lsb_release -cs) main" > /etc/apt/sources.list.d/dotnetdev.list'
sudo apt-get update
sudo apt-get install azure-functions-core-tools-4

 Passo a Passo Detalhado
Passo 1: Configuração do Ambiente Azure
1.1 Login na Azure
az login

1.2 Definir Variáveis de Ambiente
# Definir variáveis (substitua pelos seus valores)
RESOURCE_GROUP="rg-cpf-validator"
FUNCTION_APP_NAME="cpf-validator-func-app"
STORAGE_ACCOUNT_NAME="cpfvalidatorstorage"
LOCATION="East US"

1.3 Criar Resource Group
az group create --name $RESOURCE_GROUP --location "$LOCATION"

1.4 Criar Storage Account
az storage account create \
    --name $STORAGE_ACCOUNT_NAME \
    --location "$LOCATION" \
    --resource-group $RESOURCE_GROUP \
    --sku Standard_LRS \
    --kind StorageV2

Passo 2: Desenvolvimento Local
2.1 Criar o Projeto Local
# Criar diretório do projeto
mkdir cpf-validator-microservice
cd cpf-validator-microservice

# Inicializar projeto Azure Functions
func init . --worker-runtime dotnet-isolated --target-framework net8.0

2.2 Criar a Função HTTP
func new --name ValidateCpf --template "HTTP trigger" --authlevel "function"

2.3 Estrutura do Projeto
cpf-validator-microservice/
├── ValidateCpf.cs
├── Program.cs
├── host.json
├── local.settings.json
├── cpf-validator-microservice.csproj
└── .gitignore

Passo 3: Implementação do Código
3.1 Atualizar Program.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();

3.2 Implementar ValidateCpf.cs
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace CpfValidator
{
    public class ValidateCpf
    {
        private readonly ILogger _logger;

        public ValidateCpf(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ValidateCpf>();
        }

        [Function("ValidateCpf")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Iniciando validação de CPF");

            try
            {
                // Ler o body da requisição
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Body da requisição não pode estar vazio");
                }

                var data = JsonSerializer.Deserialize<CpfRequest>(requestBody);
                
                if (string.IsNullOrEmpty(data?.Cpf))
                {
                    return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "CPF não informado");
                }

                // Validar CPF
                bool isValid = ValidateCpfNumber(data.Cpf);
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                
                var result = new CpfValidationResponse
                {
                    Cpf = data.Cpf,
                    IsValid = isValid,
                    Message = isValid ? "CPF válido" : "CPF inválido",
                    Timestamp = DateTime.UtcNow
                };

                await response.WriteStringAsync(JsonSerializer.Serialize(result));
                
                _logger.LogInformation($"CPF {data.Cpf} validado. Resultado: {isValid}");
                
                return response;
            }
            catch (JsonException)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Formato JSON inválido");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno durante validação de CPF");
                return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Erro interno do servidor");
            }
        }

        private static bool ValidateCpfNumber(string cpf)
        {
            // Remove caracteres não numéricos
            cpf = cpf.Replace(".", "").Replace("-", "").Replace(" ", "");
            
            // Verifica se tem 11 dígitos
            if (cpf.Length != 11)
                return false;
            
            // Verifica se todos os dígitos são iguais
            if (cpf.All(c => c == cpf[0]))
                return false;
            
            // Calcula primeiro dígito verificador
            int sum = 0;
            for (int i = 0; i < 9; i++)
                sum += int.Parse(cpf[i].ToString()) * (10 - i);
            
            int remainder = sum % 11;
            int firstDigit = remainder < 2 ? 0 : 11 - remainder;
            
            if (int.Parse(cpf[9].ToString()) != firstDigit)
                return false;
            
            // Calcula segundo dígito verificador
            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += int.Parse(cpf[i].ToString()) * (11 - i);
            
            remainder = sum % 11;
            int secondDigit = remainder < 2 ? 0 : 11 - remainder;
            
            return int.Parse(cpf[10].ToString()) == secondDigit;
        }

        private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json");
            
            var errorResponse = new ErrorResponse
            {
                Error = message,
                Timestamp = DateTime.UtcNow
            };
            
            await response.WriteStringAsync(JsonSerializer.Serialize(errorResponse));
            return response;
        }
    }

    public class CpfRequest
    {
        public string Cpf { get; set; } = string.Empty;
    }

    public class CpfValidationResponse
    {
        public string Cpf { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}

3.3 Configurar local.settings.json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}

3.4 Configurar host.json
{
  "version": "2.0",
  "functionTimeout": "00:05:00",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    }
  }
}

Passo 4: Testes Locais
4.1 Executar Localmente
# Restaurar dependências
dotnet restore

# Executar a função localmente
func start

4.2 Testar com curl
# Teste com CPF válido
curl -X POST http://localhost:7071/api/ValidateCpf \
  -H "Content-Type: application/json" \
  -d '{"cpf": "11144477735"}'

# Teste com CPF inválido
curl -X POST http://localhost:7071/api/ValidateCpf \
  -H "Content-Type: application/json" \
  -d '{"cpf": "12345678901"}'

# Teste com CPF formatado
curl -X POST http://localhost:7071/api/ValidateCpf \
  -H "Content-Type: application/json" \
  -d '{"cpf": "111.444.777-35"}'

Passo 5: Deploy para Azure
5.1 Criar Function App na Azure
az functionapp create \
    --resource-group $RESOURCE_GROUP \
    --consumption-plan-location "$LOCATION" \
    --runtime dotnet-isolated \
    --functions-version 4 \
    --name $FUNCTION_APP_NAME \
    --storage-account $STORAGE_ACCOUNT_NAME \
    --os-type Linux

5.2 Deploy da Aplicação
# Fazer o deploy
func azure functionapp publish $FUNCTION_APP_NAME

5.3 Configurar Application Insights (Opcional)
# Criar Application Insights
az monitor app-insights component create \
    --app $FUNCTION_APP_NAME \
    --location "$LOCATION" \
    --resource-group $RESOURCE_GROUP

# Obter instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
    --app $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --query instrumentationKey \
    --output tsv)

# Configurar na Function App
az functionapp config appsettings set \
    --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY"

Passo 6: Testes em Produção
6.1 Obter URL da Função
az functionapp function show \
    --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --function-name ValidateCpf \
    --query "invokeUrlTemplate" \
    --output tsv

6.2 Obter Function Key
az functionapp keys list \
    --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP

6.3 Testar na Azure
# Substitua pela URL e chave obtidas acima
FUNCTION_URL="https://your-function-app.azurewebsites.net/api/ValidateCpf"
FUNCTION_KEY="your-function-key"

curl -X POST "$FUNCTION_URL?code=$FUNCTION_KEY" \
  -H "Content-Type: application/json" \
  -d '{"cpf": "11144477735"}'

 Monitoramento e Logs
Visualizar Logs
# Logs em tempo real
func azure functionapp logstream $FUNCTION_APP_NAME

# Via Azure CLI
az functionapp log tail --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP

Métricas no Azure Portal
1.	Acesse o Azure Portal
2.	Navegue para sua Function App
3.	Visualize métricas em "Monitor" > "Metrics"
4.	Configure alertas em "Monitor" > "Alerts"
 Segurança
Níveis de Autorização
•	Function: Requer function key (padrão)
•	Admin: Requer master key
•	Anonymous: Sem autenticação (não recomendado)
Configurar CORS (se necessário)
az functionapp cors add \
    --name $FUNCTION_APP_NAME \
    --resource-group $RESOURCE_GROUP \
    --allowed-origins "*"

 Custos Estimados
Modelo de Preços Consumption Plan
•	Execuções: Primeiras 1M execuções gratuitas/mês
•	Duração: Baseado em GB-segundo de recursos consumidos
•	Storage: ~$0.18/GB/mês para Storage Account
Exemplo de Custo (estimativa para uso moderado)
•	10.000 execuções/mês: ~$0.00
•	50.000 execuções/mês: ~$0.20
•	100.000 execuções/mês: ~$0.60
 Otimizações e Melhorias
Performance
•	Cache de resultados para CPFs válidos
•	Validação assíncrona em batch
•	Compressão de respostas HTTP
Funcionalidades Adicionais
•	Validação de CNPJ
•	Rate limiting
•	Logs estruturados
•	Health check endpoint
CI/CD com GitHub Actions
name: Deploy Azure Function

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - name: Checkout
      uses: actions/checkout@v2
      
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 8.0.x
        
    - name: Build
      run: dotnet build --configuration Release
      
    - name: Deploy
      uses: Azure/functions-action@v1
      with:
        app-name: ${{ secrets.AZURE_FUNCTIONAPP_NAME }}
        package: './output'
        publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}

Testes Automatizados
Testes Unitários
[Test]
public void ValidateCpf_ValidCpf_ReturnsTrue()
{
    // Arrange
    string validCpf = "11144477735";
    
    // Act
    bool result = ValidateCpfNumber(validCpf);
    
    // Assert
    Assert.IsTrue(result);
}

[Test]
public void ValidateCpf_InvalidCpf_ReturnsFalse()
{
    // Arrange
    string invalidCpf = "12345678901";
    
    // Act
    bool result = ValidateCpfNumber(invalidCpf);
    
    // Assert
    Assert.IsFalse(result);
}

📚 Recursos Adicionais
Documentação Oficial
•	Azure Functions Documentation
•	.NET Isolated Process Guide
Exemplos de CPF para Teste
•	Válidos: 11144477735, 11111111111 (formato válido mas não real)
•	Inválidos: 12345678901, 00000000000
Comandos Úteis
# Verificar status da Function App
az functionapp show --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP

# Reiniciar Function App
az functionapp restart --name $FUNCTION_APP_NAME --resource-group $RESOURCE_GROUP

# Deletar recursos (cuidado!)
az group delete --name $RESOURCE_GROUP --yes --no-wait

❗ Troubleshooting
Problemas Comuns
1.	Erro de Storage Account: Verificar se o nome é único globalmente
2.	Timeout na Function: Ajustar functionTimeout no host.json
3.	Erro de CORS: Configurar origins permitidas
4.	Função não encontrada: Verificar se o deploy foi bem-sucedido
Debug Local
# Definir ambiente de debug
export AZURE_FUNCTIONS_ENVIRONMENT=Development

# Executar com debug
func start --verbose



Este microsserviço serverless para validação de CPF oferece:
•	✅ Alta Performance: Algoritmo otimizado de validação
•	✅ Escalabilidade Automática: Escala conforme a demanda
•	✅ Custo Eficiente: Paga apenas pelo que usar
•	✅ Fácil Manutenção: Código limpo e bem documentado
•	✅ Monitoramento Integrado: Application Insights para telemetria
•	✅ Segurança: Autenticação por chave de função
O projeto está pronto para produção e pode ser facilmente expandido para incluir outras validações ou funcionalidades adicionais.
