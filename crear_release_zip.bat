@echo off
title Shaders Wallpaper - Crear Release ZIP para GitHub
cd /d "%~dp0"

rem ==== Cambia aqui el numero de version ====
set "VERSION=2.1"
rem ===========================================

set "NAME=ShadersWallpaper_v%VERSION%_Portable"
set "DIST_FOLDER=dist\%NAME%"
set "ZIP=%USERPROFILE%\Desktop\%NAME%.zip"

echo ====================================================
echo  Preparando Release v%VERSION% para GitHub
echo ====================================================
echo.

echo [1/4] Compilando (.NET 8 y WebView2 incluidos en el .exe)...
dotnet publish "src\ShadersWallpaper.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -p:Version=%VERSION%.0 -o "dist_tmp" --nologo -v q
if errorlevel 1 (
  echo ERROR: Fallo la compilacion. Revisa tener el SDK de .NET 8.
  pause
  exit /b 1
)

echo [2/4] Preparando carpeta...
if exist "%DIST_FOLDER%" rmdir /s /q "%DIST_FOLDER%"
mkdir "%DIST_FOLDER%\wallpaper"
copy "dist_tmp\ShadersWallpaper.exe" "%DIST_FOLDER%\" >nul
copy "app\settings.json" "%DIST_FOLDER%\" >nul
copy "app\icon.ico" "%DIST_FOLDER%\" >nul
copy "README.md" "%DIST_FOLDER%\LEEME.txt" >nul
rmdir /s /q "dist_tmp"

echo [3/4] Copiando el fondo (index.html, sonidos, fuente) y solo los 10 videos del menu...
robocopy "app\wallpaper" "%DIST_FOLDER%\wallpaper" /E /XF *.mp4 /NJH /NJS /NFL /NDL /NP >nul
robocopy "app\wallpaper" "%DIST_FOLDER%\wallpaper" minecraft-sunset.mp4 minecraft-sunrise-mountains.mp4 sakura-forest.mp4 minecraft-northern-light.mp4 minecraft-aquarium-4k.mp4 minecraft-overgrown-cabin.mp4 minecraft-rainy-cabin.mp4 nature-in-minecraft.mp4 minecraft-nature.mp4 snowy-campfire.mp4 /NJH /NJS /NFL /NDL /NP >nul

set /a COUNT=0
for %%F in ("%DIST_FOLDER%\wallpaper\*.mp4") do set /a COUNT+=1
if not "%COUNT%"=="10" (
  echo ERROR: se esperaban 10 videos y hay %COUNT%. Revisa app\wallpaper.
  pause
  exit /b 1
)

echo [4/4] Comprimiendo %NAME%.zip...
if exist "%ZIP%" del /q "%ZIP%"
powershell -NoProfile -Command "Compress-Archive -Path '%DIST_FOLDER%\*' -DestinationPath '%ZIP%' -Force"

echo.
echo ====================================================
echo  COMPLETADO CON EXITO
echo  Archivo en tu Escritorio: %NAME%.zip
echo ====================================================
echo.
echo Ahora subelo en GitHub como un release nuevo con la etiqueta v%VERSION%.
pause
