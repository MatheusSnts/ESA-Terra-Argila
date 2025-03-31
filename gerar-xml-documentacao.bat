@echo off
echo Compilando o projeto para gerar documentação XML...
dotnet build ESA-Terra-Argila/ESA-Terra-Argila.csproj

echo.
echo Documentação XML gerada com sucesso!
echo Os arquivos XML estão em: ESA-Terra-Argila\bin\Debug\net8.0\ESA-Terra-Argila.xml
echo.
pause 