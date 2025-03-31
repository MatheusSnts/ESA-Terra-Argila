# Script para extrair comentários XML e gerar documentação HTML simples

# Criar diretório de documentação
$docsDir = "documentacao-html"
if (-not (Test-Path -Path $docsDir)) {
    Write-Output "Criando diretório '$docsDir'..."
    New-Item -ItemType Directory -Path $docsDir | Out-Null
}

# Função para extrair comentários XML de um arquivo
function Extract-XmlComments {
    param (
        [string]$FilePath
    )

    $content = Get-Content -Path $FilePath -Raw
    $fileName = [System.IO.Path]::GetFileNameWithoutExtension($FilePath)
    $namespaceName = ""
    
    # Extrair namespace
    if ($content -match "namespace\s+([^\s{]+)") {
        $namespaceName = $matches[1]
    }
    
    # Extrair comentários de classe
    $classComments = @()
    if ($content -match "///\s*<summary>(.*?)</summary>" -replace "\r\n", "") {
        $classComments += $matches[1].Trim()
    }
    
    # Extrair comentários de propriedades e métodos
    $propertyComments = @()
    $methodComments = @()
    
    $lines = $content -split "`n"
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        
        # Procurar por comentários XML
        if ($line -match "///\s*<summary>") {
            $commentBlock = ""
            $j = $i
            
            # Ler todo o bloco de comentário
            while ($j -lt $lines.Length -and -not ($lines[$j] -match "<\/summary>")) {
                $commentBlock += $lines[$j] + "`n"
                $j++
            }
            
            # Adicionar a última linha que contém </summary>
            if ($j -lt $lines.Length) {
                $commentBlock += $lines[$j] + "`n"
                $j++
            }
            
            # Verificar o que vem depois do comentário
            $codeLines = ""
            while ($j -lt $lines.Length -and -not ($lines[$j] -match "///")) {
                $codeLines += $lines[$j] + "`n"
                $j++
            }
            
            # Determinar se é propriedade ou método
            if ($codeLines -match "public\s+.*?\s+(\w+)\s*\{" -or $codeLines -match "protected\s+.*?\s+(\w+)\s*\{") {
                # É uma propriedade
                $propertyName = $matches[1]
                $commentSummary = ""
                
                if ($commentBlock -match "<summary>(.*?)</summary>" -replace "`n", "") {
                    $commentSummary = $matches[1].Trim()
                }
                
                $propertyComments += @{
                    Name = $propertyName
                    Summary = $commentSummary
                }
            }
            elseif ($codeLines -match "public\s+.*?\s+(\w+)\s*\(" -or $codeLines -match "protected\s+.*?\s+(\w+)\s*\(") {
                # É um método
                $methodName = $matches[1]
                $commentSummary = ""
                $parameters = @()
                
                if ($commentBlock -match "<summary>(.*?)</summary>" -replace "`n", "") {
                    $commentSummary = $matches[1].Trim()
                }
                
                # Extrair parâmetros
                $paramMatches = [regex]::Matches($commentBlock, "<param\s+name=[""'](\w+)[""']>(.*?)<\/param>")
                foreach ($match in $paramMatches) {
                    $parameters += @{
                        Name = $match.Groups[1].Value
                        Description = $match.Groups[2].Value.Trim()
                    }
                }
                
                $methodComments += @{
                    Name = $methodName
                    Summary = $commentSummary
                    Parameters = $parameters
                }
            }
            
            # Avançar para a próxima linha após o bloco de comentário
            $i = $j - 1
        }
    }
    
    return @{
        FileName = $fileName
        Namespace = $namespaceName
        ClassComments = $classComments
        PropertyComments = $propertyComments
        MethodComments = $methodComments
    }
}

# Gerar página HTML para um arquivo
function Generate-HtmlPage {
    param (
        [hashtable]$Data,
        [string]$OutputDir
    )
    
    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Documentação - $($Data.FileName)</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        h1 { color: #2c3e50; }
        h2 { color: #3498db; margin-top: 30px; }
        h3 { color: #16a085; }
        .property { margin-left: 20px; margin-bottom: 15px; }
        .method { margin-left: 20px; margin-bottom: 30px; }
        .param { margin-left: 40px; margin-bottom: 5px; }
        .param-name { font-weight: bold; }
        pre { background-color: #f5f5f5; padding: 10px; border-radius: 5px; }
        nav { margin-bottom: 20px; }
        a { color: #3498db; text-decoration: none; }
        a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <nav>
        <a href="index.html">Página Inicial</a>
    </nav>
    <h1>$($Data.FileName)</h1>
    <pre>Namespace: $($Data.Namespace)</pre>
    
    <h2>Descrição</h2>
    <p>$($Data.ClassComments -join "<br>")</p>
    
    <h2>Propriedades</h2>
"@
    
    foreach ($prop in $Data.PropertyComments) {
        $html += @"
    <div class="property">
        <h3>$($prop.Name)</h3>
        <p>$($prop.Summary)</p>
    </div>
"@
    }
    
    $html += @"
    
    <h2>Métodos</h2>
"@
    
    foreach ($method in $Data.MethodComments) {
        $html += @"
    <div class="method">
        <h3>$($method.Name)</h3>
        <p>$($method.Summary)</p>
"@
        
        if ($method.Parameters.Count -gt 0) {
            $html += @"
        <p><strong>Parâmetros:</strong></p>
"@
            
            foreach ($param in $method.Parameters) {
                $html += @"
        <div class="param">
            <span class="param-name">$($param.Name):</span> $($param.Description)
        </div>
"@
            }
        }
        
        $html += @"
    </div>
"@
    }
    
    $html += @"
</body>
</html>
"@
    
    $outputFile = Join-Path $OutputDir "$($Data.FileName).html"
    $html | Out-File $outputFile -Encoding UTF8
    
    Write-Output "Documentação para $($Data.FileName) gerada em $outputFile"
}

# Gerar página de índice
function Generate-IndexPage {
    param (
        [array]$Files,
        [string]$OutputDir
    )
    
    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Documentação do Projeto ESA-Terra-Argila</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        h1 { color: #2c3e50; }
        h2 { color: #3498db; margin-top: 30px; }
        .file-list { margin-left: 20px; }
        .file-item { margin-bottom: 10px; }
        a { color: #3498db; text-decoration: none; }
        a:hover { text-decoration: underline; }
    </style>
</head>
<body>
    <h1>Documentação do Projeto ESA-Terra-Argila</h1>
    <p>Esta documentação foi gerada automaticamente a partir dos comentários XML no código-fonte.</p>
    
    <h2>Modelos</h2>
    <div class="file-list">
"@
    
    $models = $Files | Where-Object { $_.FileName -notmatch "Controller" }
    foreach ($model in $models) {
        $html += @"
        <div class="file-item">
            <a href="$($model.FileName).html">$($model.FileName)</a>
        </div>
"@
    }
    
    $html += @"
    </div>
    
    <h2>Controladores</h2>
    <div class="file-list">
"@
    
    $controllers = $Files | Where-Object { $_.FileName -match "Controller" }
    foreach ($controller in $controllers) {
        $html += @"
        <div class="file-item">
            <a href="$($controller.FileName).html">$($controller.FileName)</a>
        </div>
"@
    }
    
    $html += @"
    </div>
</body>
</html>
"@
    
    $outputFile = Join-Path $OutputDir "index.html"
    $html | Out-File $outputFile -Encoding UTF8
    
    Write-Output "Página de índice gerada em $outputFile"
}

# Diretórios para processar
$directories = @(
    "ESA-Terra-Argila/Models",
    "ESA-Terra-Argila/Controllers"
)

# Processa todos os arquivos .cs nos diretórios
$allFiles = @()
foreach ($dir in $directories) {
    if (Test-Path $dir) {
        $files = Get-ChildItem -Path $dir -Filter "*.cs" -Recurse
        
        foreach ($file in $files) {
            Write-Output "Processando $($file.FullName)..."
            $data = Extract-XmlComments -FilePath $file.FullName
            Generate-HtmlPage -Data $data -OutputDir $docsDir
            $allFiles += $data
        }
    }
    else {
        Write-Output "Diretório $dir não encontrado."
    }
}

# Gerar página de índice
Generate-IndexPage -Files $allFiles -OutputDir $docsDir

Write-Output "Documentação HTML gerada com sucesso em $docsDir!" 