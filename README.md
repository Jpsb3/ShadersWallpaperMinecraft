# 🌅 Shaders Wallpaper (Minecraft Java Edition)
*Fondo animado dinámico e interactivo para Windows, completamente independiente.*

[![Descargas](https://img.shields.io/github/downloads/Jpsb3/ShadersWallpaperMinecraft/total?label=descargas&color=brightgreen)](https://github.com/Jpsb3/ShadersWallpaperMinecraft/releases/latest)
[![Versión](https://img.shields.io/github/v/release/Jpsb3/ShadersWallpaperMinecraft?label=versi%C3%B3n)](https://github.com/Jpsb3/ShadersWallpaperMinecraft/releases/latest)
[![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-blue.svg)](https://microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![Framework](https://img.shields.io/badge/Engine-WebView2%20%2B%20Win32-green.svg)]()
[![Performance](https://img.shields.io/badge/RAM-~60MB-brightgreen.svg)]()

**Shaders Wallpaper** es una aplicación nativa para Windows creada en C# que transforma tu escritorio en un paisaje vivo de Minecraft con shaders a **60 FPS / 4K**. Cambia automáticamente según la hora real del día y cuenta con un menú interactivo idéntico a la pantalla de título de **Minecraft Java Edition** con sonidos oficiales.

Desarrollado de forma nativa e independiente por **Jpsb3** (sin necesidad de Lively Wallpaper ni Wallpaper Engine).

---

## ✨ Características Principales

* ⏰ **Ciclo Dinámico de 24 Horas:** Cambia de forma automática y suave entre 4 fases horarias:
  * 🌄 **Amanecer (06:00 - 09:00):** Cumbres y lago al amanecer en 60 FPS.
  * ☀️ **Día (09:00 - 17:00):** Bosque Sakura 4K con pétalos cayendo.
  * 🌅 **Atardecer (17:00 - 19:30):** Atardecer dorado sobre el lago con reflejos de sol.
  * 🌌 **Noche (19:30 - 06:00):** Aurora boreal 4K en cielo nocturno.
* 🎮 **Interfaz Oficial de Minecraft Java Edition:**
  * Logo oficial en 3D con textura de piedra tallada.
  * Cartel amarillo (*Splash Text*) con animación de rebote y frases dinámicas.
  * Botones clásicos con relieve, borde blanco y texto en oro brillante al pasar el cursor.
  * Notificación de avance (🏆 *¡Avance Realizado!*) al cambiar de fondo.
* 🔊 **Sonidos Oficiales de Minecraft:** Clics de interfaz (`click.wav`), sonido de logros (`toast_in/out.wav`) y orbes de experiencia con latencia cero.
* 🔘 **Botón Flotante Discreto:** Botón cuadrado de 42x42 px casi invisible en reposo (8% de opacidad) en la esquina inferior derecha que se ilumina con el borde blanco oficial al pasar el ratón.
* ⚡ **Optimización Extrema para Juegos (0% de CPU):**
  * Prioridad de proceso `BelowNormal` para nunca competir con juegos o programas de trabajo.
  * **Pausa Automática Inmediata:** Si abres un juego a pantalla completa (como **Valorant**, Minecraft, etc.), el fondo se congela al instante y el uso de CPU/GPU cae a 0.0%.
  * Pausa inteligente en laptops al desconectar el cargador (modo batería) o al bloquear la sesión (`Win + L`).
* 📦 **Consumo Ultrabajo:** Apenas **~60 MB de memoria RAM**.

---

## 🚀 Descarga e Instalación Rápida (Para Usuarios)

1. Ve a la sección de **[Releases](https://github.com/Jpsb3/ShadersWallpaperMinecraft/releases)** y descarga el archivo **`ShadersWallpaper_v2.1_Portable.zip`**.
2. Descomprime la carpeta en la ubicación que prefieras (por ejemplo, en `C:\Programas` o en tu carpeta de usuario).
3. Ejecuta **`ShadersWallpaper.exe`**. ¡El fondo arrancará de inmediato!
4. *(Opcional)* Haz clic derecho en el icono de la bandeja del sistema (junto al reloj) y marca **«Iniciar con Windows»** para que se ejecute solo al encender tu PC.

> [!NOTE]
> Al ser una aplicación creada por un desarrollador independiente, Windows SmartScreen puede mostrar un aviso azul de *"Windows protegió su PC"*. Haz clic en **«Más información»** y luego en **«Ejecutar de todas formas»**.

---

## 🛠️ Atajos y Controles

* **Botón del Escritorio:** Clic en el botón cuadrado en la esquina inferior derecha.
* **Atajo de Teclado:** Pulsa **`Ctrl + Alt + W`** en cualquier momento para abrir/cerrar el menú.
* **Bandeja del Sistema (Junto al reloj):**
  * Clic derecho para **Pausar fondo**, **Recargar fondo**, **Iniciar con Windows** o **Salir**.

---

## ⚙️ Personalización (`settings.json`)

Puedes editar el archivo `settings.json` para ajustar el comportamiento de la aplicación:

```json
{
  "wallpaperFolder": "wallpaper",
  "page": "index.html",
  "attachMode": "auto",
  "pauseOnFullscreen": true,
  "pauseOnMaximized": true,
  "pauseOnBattery": true
}
```

Para cambiar las frases del cartel amarillo (*Splash Text*) o agregar nuevos videos, simplemente edita `wallpaper/index.html` y pulsa **«Recargar fondo»** en la bandeja del sistema.

---

## 📄 Licencia

El código de este proyecto (la app en C# y el `index.html` de la interfaz) es © 2026 Juan Pablo Bustos y está bajo la **Licencia GNU GPL v3.0**. Consulta el archivo [LICENSE](LICENSE).

En resumen: puedes usarlo, estudiarlo, modificarlo y compartirlo, pero cualquier versión modificada que distribuyas también debe publicarse bajo GPL v3.0, con su código abierto y manteniendo el crédito al autor original.

> Minecraft, su logo, fuente y sonidos pertenecen a Mojang Studios / Microsoft. Los videos pertenecen a sus respectivos creadores. Estos recursos no están cubiertos por la licencia de este proyecto.

---

**Autor:** [Jpsb3](https://github.com/Jpsb3)  
*¡Si te gustó este proyecto, dale una ⭐ estrella en GitHub!*
