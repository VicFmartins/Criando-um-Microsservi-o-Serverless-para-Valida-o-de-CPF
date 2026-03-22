# Criando um Microsserviço Serverless para Validação de CPF

![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet&logoColor=white)
![Azure Functions](https://img.shields.io/badge/runtime-Azure_Functions-0062AD?logo=azurefunctions&logoColor=white)
![Serverless](https://img.shields.io/badge/arquitetura-serverless-2e8b57)
![Tests](https://img.shields.io/badge/tests-xUnit-0a7ea4)

Microsserviço serverless para validação de CPF usando Azure Functions no modelo .NET Isolated, com endpoint HTTP, health check, testes unitários, infraestrutura como código e workflow de deploy.

O repositório foi melhorado para deixar de ser apenas um guia descritivo e virar uma base real de projeto.

## O Que Este Projeto Entrega

- Azure Function HTTP para validar CPF
- Endpoint de health check
- Serviço isolado para a regra de negócio
- Normalização de CPF com ou sem máscara
- Respostas JSON padronizadas
- Testes unitários com xUnit
- Workflow de deploy com GitHub Actions
- Infraestrutura como código com Bicep

## Estrutura Atual

```text
.
├── CpfValidator.sln
├── README.md
├── .gitignore
├── .github/
│   └── workflows/
│       └── deploy.yml
├── infra/
│   └── main.bicep
├── src/
│   └── CpfValidator.Functions/
│       ├── Functions/
│       ├── Models/
│       ├── Services/
│       ├── Program.cs
│       ├── host.json
│       └── local.settings.example.json
└── tests/
    └── CpfValidator.Functions.Tests/
```

## Endpoints

### `POST /api/cpf/validate`

Recebe um CPF e retorna se ele é válido ou inválido.

Exemplo de payload:

```json
{
  "cpf": "111.444.777-35"
}
```

Exemplo de resposta:

```json
{
  "cpf": "111.444.777-35",
  "isValid": true,
  "message": "CPF valido.",
  "timestampUtc": "2026-03-22T00:00:00Z",
  "normalizedCpf": "11144477735"
}
```

### `GET /api/health`

Retorna status simples do serviço.

## Como Executar Localmente

### 1. Restaurar dependências

```bash
dotnet restore CpfValidator.sln
```

### 2. Copiar a configuração local

```bash
cp src/CpfValidator.Functions/local.settings.example.json src/CpfValidator.Functions/local.settings.json
```

No PowerShell:

```powershell
Copy-Item src/CpfValidator.Functions/local.settings.example.json src/CpfValidator.Functions/local.settings.json
```

### 3. Rodar os testes

```bash
dotnet test CpfValidator.sln
```

### 4. Executar a Function localmente

Se você tiver Azure Functions Core Tools:

```bash
func start --script-root src/CpfValidator.Functions
```

## Exemplo de Teste com `curl`

```bash
curl -X POST http://localhost:7071/api/cpf/validate \
  -H "Content-Type: application/json" \
  -d "{\"cpf\":\"11144477735\"}"
```

## Componentes Principais

### `CpfValidatorService`

Arquivo: `src/CpfValidator.Functions/Services/CpfValidatorService.cs`

Responsável por:

- remover máscara do CPF;
- validar tamanho;
- impedir sequências repetidas;
- calcular os dígitos verificadores.

### `ValidateCpfFunction`

Arquivo: `src/CpfValidator.Functions/Functions/ValidateCpfFunction.cs`

Responsável por:

- receber a requisição HTTP;
- validar o payload;
- chamar o serviço de negócio;
- devolver a resposta JSON.

## Deploy

### GitHub Actions

O workflow em [deploy.yml](C:/Users/vitor/OneDrive/Documentos/Playground/repo-cpf-serverless/.github/workflows/deploy.yml) já faz:

1. restore;
2. build;
3. test;
4. publish;
5. deploy para Azure Functions.

Secrets esperados:

- `AZURE_FUNCTIONAPP_NAME`
- `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`

### Infraestrutura com Bicep

O arquivo [main.bicep](C:/Users/vitor/OneDrive/Documentos/Playground/repo-cpf-serverless/infra/main.bicep) provisiona:

- Storage Account
- Consumption Plan
- Application Insights
- Function App Linux para .NET Isolated

Exemplo de deploy:

```bash
az deployment group create \
  --resource-group rg-cpf-validator \
  --template-file infra/main.bicep \
  --parameters functionAppName=cpf-validator-demo storageAccountName=cpfvalidatordemo123
```

## Melhorias Aplicadas Nesta Versão

- criação do projeto .NET de Azure Functions;
- separação da regra de negócio em serviço reutilizável;
- inclusão de testes automatizados;
- adição de health check;
- estrutura de deploy com GitHub Actions;
- infraestrutura como código com Bicep;
- documentação alinhada com o código real.

## Próximos Passos

- adicionar autenticação por API key ou Easy Auth;
- incluir validação de CNPJ;
- adicionar OpenAPI/Swagger;
- registrar logs estruturados;
- criar endpoint batch para validar múltiplos CPFs.

## Observação

Este projeto agora funciona como um exemplo prático de microsserviço serverless em Azure. Ele já serve como portfólio técnico e também como base para evoluções reais em produção.
