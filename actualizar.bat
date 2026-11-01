@echo off
chcp 65001 >nul
title Shaders Wallpaper - Actualizar
cd /d "%~dp0"
echo Cerrando la app...
taskkill /IM ShadersWallpaper.exe /F >nul 2>&1
timeout /t 1 >nul
echo Compilando...
dotnet publish "src\ShadersWallpaper.csproj" -c Release -r win-x64 --self-contained false -o "app" --nologo -v q > build.log 2>&1
if errorlevel 1 (
  type build.log
  echo ERROR al compilar. Revisa build.log
  pause
  exit /b 1
)
start "" "app\ShadersWallpaper.exe"
echo Listo.
timeout /t 2 >nul
