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
  echo [ERREUR] Le SDK .NET 8 n'est pas installe.
  echo Installez-le : https://dotnet.microsoft.com/download/dotnet/8.0
  echo ^(choisir "SDK 8.0.x" pour Windows x64^), puis relancez ce fichier.
  pause
  exit /b 1
)

dotnet --list-sdks | findstr /b "8." >nul
if errorlevel 1 (
  echo [ERREUR] Aucun SDK .NET 8 trouve. Versions presentes :
  dotnet --list-sdks
  echo Installez le SDK 8.0 : https://dotnet.microsoft.com/download/dotnet/8.0
  pause
  exit /b 1
)

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
dotnet restore VieDon.sln
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
dotnet run --project VieDon.App\VieDon.App.csproj -f net8.0-windows10.0.19041.0
if errorlevel 1 goto :erreur
exit /b 0

:erreur
echo.
echo [ERREUR] Une etape a echoue. Copiez TOUT le texte ci-dessus dans la conversation.
pause
exit /b 1
