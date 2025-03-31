# Adicionar o pacote VSDocumentor ao projeto se não estiver instalado
if (-not (Test-Path -Path "ESA-Terra-Argila/packages")) {
    Write-Output "Adicionando pacote VSDocumentor..."
    dotnet add ESA-Terra-Argila/ESA-Terra-Argila.csproj package VSDocumentor
}

# Compilar o projeto para gerar os arquivos XML de documentação
Write-Output "Compilando o projeto..."
dotnet build ESA-Terra-Argila/ESA-Terra-Argila.csproj

# Criar diretório para documentação
if (-not (Test-Path -Path "documentacao")) {
    Write-Output "Criando diretório 'documentacao'..."
    New-Item -ItemType Directory -Path "documentacao" | Out-Null
}

# Copiar os arquivos XML de documentação para o diretório de documentação
Write-Output "Copiando arquivos XML de documentação..."
Copy-Item -Path "ESA-Terra-Argila/bin/Debug/net8.0/ESA-Terra-Argila.xml" -Destination "documentacao/"

Write-Output "Documentação XML gerada com sucesso! Verifique o diretório 'documentacao'."
Write-Output "Para visualizar a documentação, abra os arquivos XML em um navegador ou use um visualizador XML." 