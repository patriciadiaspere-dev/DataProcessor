param(
    [string]$Ambiente = "Development"
)

$ErrorActionPreference = "Stop"

Write-Host "==> Aplicando migrações para o banco configurado em appsettings.json ($Ambiente)..." -ForegroundColor Cyan

# Verifica se dotnet está disponível
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "A CLI do .NET não foi encontrada. Instale o SDK .NET 6 ou superior e tente novamente."
}

# Restaura a ferramenta dotnet-ef se existir manifesto
$toolsManifest = Join-Path $PSScriptRoot "..\.config\dotnet-tools.json"
if (Test-Path $toolsManifest) {
    Write-Host "Restaurando ferramentas declaradas em .config/dotnet-tools.json..." -ForegroundColor DarkCyan
    dotnet tool restore
}

# Executa o update apontando projeto de dados e startup
$env:ASPNETCORE_ENVIRONMENT = $Ambiente
Write-Host "Executando: dotnet ef database update -p src/DataProcessor.Data -s src/DataProcessor.Api" -ForegroundColor DarkCyan

dotnet ef database update -p src/DataProcessor.Data -s src/DataProcessor.Api
