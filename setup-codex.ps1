# ==============================
# 🚀 OpenAI Codex Setup Script
# Autor: Patricia Dias (by ChatGPT)
# Descrição: Instala Node.js (se necessário), Codex CLI e configura a API Key
# ==============================

Write-Host "=== OpenAI Codex Local Setup ===" -ForegroundColor Cyan

# 1️⃣ Verifica Node.js
Write-Host "Verificando Node.js..."
$nodeVersion = node -v 2>$null
if (-not $nodeVersion) {
    Write-Host "Node.js não encontrado. Instalando via winget..." -ForegroundColor Yellow
    winget install OpenJS.NodeJS.LTS -e --accept-source-agreements --accept-package-agreements
} else {
    Write-Host "✅ Node.js detectado: $nodeVersion" -ForegroundColor Green
}

# 2️⃣ Instala Codex CLI
Write-Host "`nInstalando OpenAI Codex CLI..."
npm install -g @openai/codex

# 3️⃣ Solicita a chave de API
Write-Host "`nPor favor, cole sua chave de API da OpenAI (começa com 'sk-')"
$apiKey = Read-Host "Chave de API"

if ($apiKey -notmatch "^sk-") {
    Write-Host "❌ Chave inválida. Deve começar com 'sk-'. Saindo..." -ForegroundColor Red
    exit
}

# 4️⃣ Configura variável de ambiente (sessão atual + persistente)
[System.Environment]::SetEnvironmentVariable("OPENAI_API_KEY", $apiKey, "User")
$env:OPENAI_API_KEY = $apiKey
Write-Host "✅ OPENAI_API_KEY configurada com sucesso." -ForegroundColor Green

# 5️⃣ Teste rápido da API
Write-Host "`nTestando conex�o com a API OpenAI..."

try {
    $response = Invoke-WebRequest `
        -Uri "https://api.openai.com/v1/models" `
        -Headers @{ "Authorization" = "Bearer $env:OPENAI_API_KEY" }

    if ($response.StatusCode -eq 200) {
        Write-Host "?? Conex�o bem-sucedida! Codex pronto para uso." -ForegroundColor Green
    } else {
        Write-Host "?? Resposta inesperada da API: $($response.StatusCode)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "?? N�o foi poss�vel validar a chave. Mensagem:" -ForegroundColor Yellow
    Write-Host $_.Exception.Message -ForegroundColor DarkYellow
}

# 6️⃣ Dica final
Write-Host "`n💡 Dica: Agora você pode usar o comando 'codex' no terminal!" -ForegroundColor Cyan
Write-Host "Exemplo: codex 'explique o código atual em português'" -ForegroundColor White
Write-Host "`n===================================" -ForegroundColor DarkGray
