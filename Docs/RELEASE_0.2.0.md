# Release 0.2.0 — Sistema de Audio

**Fecha:** 20 de Julio, 2026
**Versión anterior:** 0.1.0

---

## Resumen

Implementación completa del sistema de audio del juego, incluyendo música de fondo, efectos de sonido, menú de configuración de audio con sliders y mute, y sistema de sonidos de pasos para el jugador.

---

## Nuevos Archivos

### `Assets/Scripts/Audio/AudioManager.cs`
Singleton que maneja todo el audio del juego.
- **BGM** (Background Music): `AudioSource` con loop, volumen configurable
- **SFX** (Sound Effects): `AudioSource` para one-shot, volumen configurable
- Métodos: `PlayBGM()`, `StopBGM()`, `SetBGMVolume()`, `PlaySFX()`, `SetSFXVolume()`
- `DontDestroyOnLoad` para persistir entre escenas

### `Assets/Scripts/Audio/FootstepAudio.cs`
Script para sonidos de pasos del jugador.
- Array de `AudioClip` para variaciones de pasos
- Método `PlayFootstep()` invocado por Animation Events
- Selección aleatoria de clips
- Volumen configurable

---

## Archivos Modificados

### `Assets/Scripts/GameBoostrap.cs`
- Se agrega creación del `AudioManager` al inicio del juego
- Carga automática de `game-sound-01.mp3` desde Resources
- Reproducción de BGM al iniciar

### `Assets/Scripts/UI/Menu/PauseMenu.cs`
- Se agrega botón **"Sound"** / **"Sonido"** en el menú principal
- Nuevo sub-panel de configuración de audio con:
  - Slider de Music (BGM) — 0 a 1
  - Slider de Effects (SFX) — 0 a 1
  - Checkbox "Mute" para cada canal
  - Botón "Back" / "Volver" al menú principal
- Navegación entre menú principal y sub-panel de sonido

### `Assets/Assets/Resources/GameData/Config/localization.json`
Nuevas claves de localización (EN/ES):
- `pause.sound` — Sound / Sonido
- `sound.title` — SOUND / SONIDO
- `sound.bgm` — Music / Musica
- `sound.sfx` — Effects / Efectos
- `sound.mute` — Mute / Silenciar
- `sound.back` — Back / Volver

---

## Estructura de Audio

```
AudioManager (Singleton, DontDestroyOnLoad)
├── AudioSource BGM      → loop=true, volumen=0.5
└── AudioSource SFX      → loop=false, volumen=0.7

FootstepAudio (en Player)
└── AudioClip[]          → variaciones de pasos
    └── PlayFootstep()   → invocado por Animation Events
```

---

## Configuración Requerida en Unity Editor

### Para sonidos de pasos:
1. Agregar componente `FootstepAudio` al Player prefab
2. Asignar clips de audio de pasos al array `_footstepClips`
3. En el Animator Controller → clips de walk/run → agregar Animation Events
4. En cada evento, seleccionar `FootstepAudio.PlayFootstep`

### Para música de fondo:
- El archivo `game-sound-01.mp3` debe estar en `Assets/Assets/Resources/Sounds/`
- Se carga automáticamente al iniciar el juego

---

## Dependencias

- `com.unity.modules.audio` (ya incluido en el proyecto)
- Unity Audio System (configurado por defecto)

---

## Notas para Desarrolladores

- El `AudioManager` está disponible globalmente via `AudioManager.Instance`
- Para agregar SFX desde cualquier script: `AudioManager.Instance.PlaySFX(clip)`
- El volumen de BGM y SFX es independiente
- Los sliders del menú de pausa controlan el volumen en tiempo real
- El checkbox de mute silencia el canal sin cambiar la posición del slider

---

## Changelog Completo (v0.1.0 → v0.2.0)

```
Added:
  - AudioManager singleton with BGM and SFX channels
  - FootstepAudio component for player footstep sounds
  - Sound settings submenu in pause menu
  - Volume sliders for BGM and SFX
  - Mute toggles for BGM and SFX
  - Localization keys for sound menu (EN/ES)

Changed:
  - GameBootstrap now initializes AudioManager on startup
  - PauseMenu expanded with Sound button and settings panel

Fixed:
  - N/A
```
