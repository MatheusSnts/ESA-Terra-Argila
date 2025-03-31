# Documentação ESA Terra Argila

## Visão Geral

Esta pasta contém a documentação do projeto ESA Terra Argila, gerada a partir dos comentários XML no código-fonte.

## Como Usar a Documentação

1. Abra o arquivo `index.html` em qualquer navegador web para visualizar a documentação principal
2. Navegue entre as seções usando os links no topo da página
3. Cada classe documentada inclui:
   - Descrição geral
   - Propriedades principais com explicações
   - Métodos principais com explicações sobre parâmetros e retornos

## Como Documentar Novas Classes ou Métodos

Para adicionar documentação a novas classes ou métodos:

1. Use comentários XML (três barras `///`) antes da declaração de classes, propriedades ou métodos:

```csharp
/// <summary>
/// Descrição da classe ou método
/// </summary>
public class MinhaClasse
{
    /// <summary>
    /// Descrição da propriedade
    /// </summary>
    public string MinhaPropriedade { get; set; }

    /// <summary>
    /// Descrição do método
    /// </summary>
    /// <param name="parametro">Descrição do parâmetro</param>
    /// <returns>Descrição do valor de retorno</returns>
    public string MeuMetodo(string parametro)
    {
        // Implementação
        return string.Empty;
    }
}
```

2. Certifique-se de que a configuração `GenerateDocumentationFile` esteja habilitada no arquivo de projeto:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

3. Após adicionar novos comentários, execute um dos scripts para gerar ou atualizar a documentação:
   - `gerar-xml-documentacao.bat` para gerar apenas o arquivo XML
   - Atualize manualmente o arquivo `index.html` para incluir as novas classes ou métodos

## Tipos de Tags XML Suportadas

- **`<summary>`**: Descrição geral da classe, propriedade ou método
- **`<param name="nome">`**: Descrição de um parâmetro de método
- **`<returns>`**: Descrição do valor de retorno de um método
- **`<remarks>`**: Informações adicionais ou observações importantes
- **`<example>`**: Exemplos de uso
- **`<exception cref="TipoExcecao">`**: Documentação de exceções que podem ser lançadas

## Próximos Passos para Expansão da Documentação

1. Documentar todas as classes restantes no projeto
2. Adicionar mais detalhes sobre implementações específicas
3. Incluir exemplos de uso para APIs públicas
4. Expandir a documentação com diagramas UML e fluxos de processo

## Ferramentas Alternativas para Documentação

Para uma documentação mais completa e automatizada, considere usar ferramentas como:

1. **DocFX**: Framework completo para gerar documentação
   - Instalação: `dotnet tool install -g docfx`
   - Uso: Consulte o script `gerar-documentacao.ps1`

2. **Swagger/OpenAPI**: Para documentação de APIs REST
   - Adicione o pacote NuGet: `Swashbuckle.AspNetCore`
   - Configure no `Startup.cs` ou `Program.cs` 