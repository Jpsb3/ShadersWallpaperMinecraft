@echo off
chcp 65001 >nul
title Shaders Wallpaper - Instalar
cd /d "%~dp0"

echo [1/4] Cerrando versiones anteriores y Lively...
taskkill /IM ShadersWallpaper.exe /F >nul 2>&1
taskkill /IM Lively.exe /F >nul 2>&1
timeout /t 1 >nul
if exist "app" rmdir /s /q "app"
rem Limpia archivos de la version anterior (interfaz rehecha en WPF)
for %%F in (AppController.cs McUi.cs MenuWindow.cs ToastWindow.cs Sounds.cs WallpaperWindow.cs DesktopButton.cs Config.cs config.json) do if exist "src\%%F" del /q "src\%%F"
if exist "src\assets" rmdir /s /q "src\assets"
if exist "src\bin" rmdir /s /q "src\bin"
if exist "src\obj" rmdir /s /q "src\obj"

echo [2/4] Compilando la app...
dotnet publish "src\ShadersWallpaper.csproj" -c Release -r win-x64 --self-contained false -o "app" --nologo -v q > build.log 2>&1
if errorlevel 1 (
  type build.log
  echo.
  echo ERROR al compilar. Revisa build.log
  pause
  exit /b 1
)

echo [3/4] Copiando tu fondo de Lively (index.html, videos, sonidos, fuente)...
set "LIVELY=%LOCALAPPDATA%\Packages\12030rocksdanister.LivelyWallpaper_97hta09mmv6hy\LocalCache\Local\Lively Wallpaper\Library\SaveData\wptmp\zf2ksdpg.pjr"
if exist "%LIVELY%\index.html" (
  robocopy "%LIVELY%" "app\wallpaper" /E /XF LivelyInfo.json abrir_vista_previa.bat /NJH /NJS /NFL /NDL /NP >nul
) else (
  echo   No encontre la carpeta de Lively. Copia tu index.html y sus archivos a "app\wallpaper".
)

echo [4/4] Abriendo Shaders Wallpaper...
start "" "app\ShadersWallpaper.exe"
echo.
echo Listo. Si algo no se ve, revisa app\log.txt
timeout /t 4 >nul
