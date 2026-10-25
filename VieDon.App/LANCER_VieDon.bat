@echo off
chcp 65001 >nul
title VieDon - lancement
cd /d "%~dp0"

echo ============================================
echo   VieDon : verification puis lancement
echo ============================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERREUR] Le SDK .NET n'est pas installe.
  echo Installez-le : https://dotnet.microsoft.com/download/dotnet/8.0
  echo ^(SDK 8.0 ou plus recent, Windows x64^), puis relancez ce fichier.
  pause
  exit /b 1
)

rem Accepte le SDK .NET 8 ou plus recent (9, 10...) : le projet cible net8.0 et se compile aussi avec le SDK 9.
for /f "tokens=1 delims=." %%v in ('dotnet --version') do set SDKMAJOR=%%v
if "%SDKMAJOR%"=="" set SDKMAJOR=0
if %SDKMAJOR% LSS 8 (
  echo [ERREUR] SDK .NET trop ancien. Version presente :
  dotnet --version
  echo Installez le SDK 8.0 ou plus recent : https://dotnet.microsoft.com/download/dotnet/8.0
  pause
  exit /b 1
)
echo SDK .NET detecte : version %SDKMAJOR% ^(ok^)
echo.

echo [1/4] Installation de la charge de travail MAUI ^(une seule fois, quelques minutes^)...
dotnet workload install maui-windows
if errorlevel 1 (
  echo.
  echo [ATTENTION] L'installation de la charge de travail a echoue.
  echo Clic droit sur ce fichier ^> "Executer en tant qu'administrateur", puis reessayez.
  pause
  exit /b 1
)

echo.
echo [2/4] Restauration des paquets...
dotnet restore VieDon.sln -p:SansAndroid=true
if errorlevel 1 goto :erreur

echo.
echo [3/4] Tests des regles metier...
dotnet test VieDon.Core.Tests --nologo
if errorlevel 1 (
  echo [ATTENTION] Des tests echouent : copiez le message ci-dessus dans la conversation.
  pause
)

echo.
echo [4/4] Compilation et lancement de l'application...
dotnet run --project VieDon.App\VieDon.App.csproj -f net8.0-windows10.0.19041.0 -p:SansAndroid=true
if errorlevel 1 goto :erreur
exit /b 0

:erreur
echo.
echo [ERREUR] Une etape a echoue. Copiez TOUT le texte ci-dessus dans la conversation.
pause
exit /b 1
