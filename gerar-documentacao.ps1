# Instalar a ferramenta DocFX caso ainda não esteja instalada
if (-not (Get-Command docfx -ErrorAction SilentlyContinue)) {
    Write-Output "Instalando DocFX..."
    dotnet tool install -g docfx
}

# Compilar o projeto para gerar os arquivos XML de documentação
Write-Output "Compilando o projeto..."
dotnet build ESA-Terra-Argila/ESA-Terra-Argila.csproj

# Criar diretório api se não existir
if (-not (Test-Path -Path "api")) {
    Write-Output "Criando diretório 'api'..."
    New-Item -ItemType Directory -Path "api" | Out-Null
}

# Gerar metadados da API
Write-Output "Gerando metadados da API..."
docfx metadata

# Gerar site HTML
Write-Output "Gerando site HTML..."
docfx build

Write-Output "Documentação gerada com sucesso! Verifique o diretório '_site'." 