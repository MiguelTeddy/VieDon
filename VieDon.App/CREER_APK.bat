@echo off
chcp 65001 >nul
title VieDon - creer le fichier APK
cd /d "%~dp0"

echo ============================================
echo   VieDon : creer un fichier APK a installer
echo   sur un telephone Android ^(sans cable ni emulateur^)
echo ============================================
echo.

where dotnet >nul 2>nul
if errorlevel 1 (
  echo [ERREUR] Le SDK .NET 8 n'est pas installe : https://dotnet.microsoft.com/download/dotnet/8.0
  pause
  exit /b 1
)

echo [1/2] Charge de travail Android ^(une seule fois^)...
dotnet workload install maui-android
if errorlevel 1 (
  echo [ATTENTION] Echec : clic droit sur ce fichier ^> "Executer en tant qu'administrateur".
  pause
  exit /b 1
)

echo.
echo [2/2] Creation de l'APK ^(plusieurs minutes^)...
dotnet build VieDon.App\VieDon.App.csproj -f net8.0-android -c Debug -p:EmbedAssembliesIntoApk=true
if errorlevel 1 (
  echo.
  echo [ERREUR] Copiez TOUT le texte ci-dessus dans la conversation.
  pause
  exit /b 1
)

echo.
echo Termine. Le fichier se termine par "-Signed.apk" dans :
echo   %~dp0VieDon.App\bin\Debug\net8.0-android\
start "" "%~dp0VieDon.App\bin\Debug\net8.0-android\"
echo Copiez-le sur le telephone ^(cable, Bluetooth, WhatsApp, Telegram...^) puis ouvrez-le pour l'installer.
pause
exit /b 0
