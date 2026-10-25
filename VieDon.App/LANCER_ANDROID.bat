@echo off
chcp 65001 >nul
title VieDon - Android
cd /d "%~dp0"

echo ============================================
echo   VieDon sur telephone ou emulateur Android
echo ============================================
echo.
echo Avant de continuer :
echo  - Visual Studio avec la charge de travail ".NET MAUI" est installe (il apporte le SDK Android et l'emulateur)
echo  - un telephone en debogage USB est branche, OU un emulateur Android est deja demarre
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERREUR] Le SDK .NET 8 n'est pas installe : https://dotnet.microsoft.com/download/dotnet/8.0
  pause
  exit /b 1
)

echo [1/2] Charge de travail Android ^(une seule fois, plusieurs centaines de Mo^)...
dotnet workload install maui-android
if errorlevel 1 (
  echo [ATTENTION] Echec : clic droit sur ce fichier ^> "Executer en tant qu'administrateur".
  pause
  exit /b 1
)

echo.
echo [2/2] Compilation et installation sur l'appareil Android...
dotnet build VieDon.App\VieDon.App.csproj -f net8.0-android -t:Run
if errorlevel 1 (
  echo.
  echo [ERREUR] Copiez TOUT le texte ci-dessus dans la conversation.
  echo Astuce : dans Visual Studio, l'onglet "Android Device Manager" permet de verifier l'appareil.
  pause
  exit /b 1
)
exit /b 0
