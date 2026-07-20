# Plan: Menu de Pausa (Escape)

## Objetivo

Crear un menu de pausa que se abre/cierra con la tecla Escape, conteniendo:
- Boton para salir del juego
- Boton de configuraciones (vacio por ahora)
- Boton de bandera para alternar entre espanol/ingles

## Archivos a crear

### `Assets/Scripts/UI/Menu/PauseMenu.cs`

Script principal del menu de pausa. Crea toda la UI programaticamente (sin prefab) siguiendo el patron de `DamageNumberManager`.

**Responsabilidades:**
- Escuchar la accion `Pause` (Escape) para abrir/cerrar el menu
- Crear los elementos UI: Panel overlay, titulo, 3 botones
- Pausar el tiempo (`Time.timeScale = 0/1`) al abrir/cerrar
- Activar/desactivar los inputs del jugador al abrir/cerrar
- Actualizar textos al cambiar de idioma
- Ejecutar acciones de los botones (Salir, Config, Cambiar idioma)

**Estructura UI que crea:**
```
PauseMenuPanel (CanvasGroup)
├── Title (TextMeshProUGUI)          ← "PAUSE" / "PAUSA"
├── ButtonPanel (VerticalLayoutGroup)
│   ├── ResumeButton (Button + TMP)  ← "Resume" / "Continuar"
│   ├── SettingsButton (Button + TMP)← "Settings" / "Ajustes"
│   ├── LanguageButton (Button + TMP)← Bandera emoji + codigo idioma
│   └── QuitButton (Button + TMP)    ← "Quit" / "Salir"
```

**Flujo:**
```
Update() → escucha accion Pause
  → Si menu cerrado: abre (SetActive true, Time.timeScale = 0, Time.timeScale = 0, Time.timeScale = 0)
  → Si menu abierto: cierra (SetActive false, Time.timeScale = 1, Time.timeScale = 1)

Boton Resume → cierra menu
Boton Settings → (vacio, solo log)
Boton Language → cambia idioma, actualiza textos de todos los botones
Boton Quit → Application.Quit()
```

### `Assets/Scripts/UI/Menu/PauseMenuInputs.cs`

Clase separada para manejar los inputs del jugador (activar/desactivar).

**Responsabilidades:**
- Desactivar `PlayerInputs` cuando el menu se abre (para que no se pueda mover/atacar)
- Reactivar `PlayerInputs` cuando el menu se cierra

## Archivos a modificar

### `Assets/Scripts/Player/PlayerInputs.cs`

Agregar referencia al componente `PlayerInputs` en `GameBootstrap` para poder activar/desactivarlo desde el `PauseMenu`.

**Cambios:**
- Ningun cambio en el script, solo se usa la referencia desde `GameBootstrap`

### `Assets/Scripts/GameBoostrap.cs`

Agregar referencia al `PauseMenu` y pasarlo al jugador.

**Cambios:**
- Agregar campo `[SerializeField] private PauseMenu _pauseMenu` o crearlo programaticamente
- Pasar referencia del `PlayerInputs` al `PauseMenu`

### `Assets/Scripts/ClassJson/Localization.cs`

Agregar soporte para cambio de idioma en runtime con eventos.

**Cambios:**
- Agregar campo `public string currentLanguage`
- Agregar evento `public event Action OnLanguageChanged`
- Agregar metodo `SetLanguage(string language)`
- Modificar `Get()` para usar `currentLanguage` por defecto

### `Assets/Assets/Resources/GameData/Config/localization.json`

Agregar keys nuevas para el menu de pausa:

```json
{ "key": "pause.title",       "en": "PAUSE",       "es": "PAUSA" },
{ "key": "pause.resume",      "en": "Resume",       "es": "Continuar" },
{ "key": "pause.settings",    "en": "Settings",     "es": "Ajustes" },
{ "key": "pause.language",    "en": "EN",           "es": "ES" },
{ "key": "pause.quit",        "en": "Quit",         "es": "Salir" }
```

## Flujo completo

```
1. Jugador aprieta Escape
   → PlayerInputs detecta accion "Pause"
   → Llama a PauseMenu.Toggle()

2. PauseMenu.Toggle()
   → Si menu esta cerrado:
     - panel.SetActive(true)
     - Time.timeScale = 0
     - playerInputs.enabled = false
     - Actualiza textos con idioma actual
   
   → Si menu esta abierto:
     - panel.SetActive(false)
     - Time.timeScale = 1
     - playerInputs.enabled = true

3. Jugador aprieta boton de idioma
   → PauseMenu cambia idioma via LocalizationConfig.SetLanguage()
   → LocalizationConfig dispara OnLanguageChanged
   → PauseMenu se suscribe y actualiza todos los textos

4. Jugador aprieta boton Salir
   → Application.Quit()
```

## Orden de ejecucion

```
GameBootstrap.Start()
  ├─ ConfigBoostrap.Initialize()           ← carga configs
  ├─ Player.Initialize(config)             ← inicializa jugador
  ├─ PlayerResourcesUI.Initialize(player)  ← inicializa HUD
  ├─ PlayerInputs.Initialize(player)       ← inicializa inputs
  ├─ SpawnerManager                        ← crea spawners
  └─ PauseMenu.Initialize(playerInputs)    ← inicializa menu pausa
```

## Notas importantes

- `Time.timeScale = 0` pausa la fisica y `Update()`, pero `LateUpdate()` y coroutines con `WaitForSecondsRealtime` siguen funcionando
- El `PauseMenu` crea toda la UI en codigo para no depender de prefabs
- El `CanvasGroup` permite hacer fade in/out del menu
- La accion `Pause` se agrega al action map `Player` del `InputSystem_Actions`
- Se debe desactivar el `PlayerInputs` al abrir el menu para que el jugador no se mueva/ataque mientras pausado
