# Sistema de Localizacion

## Objetivo

Crear un sistema de internacionalizacion (i18n) para el juego que permita:
- Soportar multiples idiomas (por ahora: ingles y espanol)
- Agregar nuevos idiomas facilmente desde un JSON sin tocar codigo
- Acceder a textos localizados desde cualquier script

## Estructura de archivos

```
Assets/
├── Scripts/ClassJson/Localization.cs    ← Clase C# del modelo
└── Assets/Resources/GameData/
    └── Config/
        ├── game.json                    ← Referencia al nuevo config (modificado)
        └── localization.json            ← Todos los textos del juego
```

## Archivos creados

### `Assets/Scripts/ClassJson/Localization.cs`

Clase principal con dos tipos:

**`LocalizationConfig`** — Contenedor del config:
- `defaultLanguage` — Idioma por defecto (fallback)
- `entries[]` — Array de traducciones
- `BuildLookup()` — Construye un diccionario interno para busqueda rapida O(1)
- `Get(key, language)` — Devuelve el texto traducido. Si no encuentra el idioma pedido, usa el default. Si no encuentra la key, devuelve la key misma.

**`LocalizationEntry`** — Una entrada de traduccion:
- `key` — Identificador unico (ej: `"menu.play"`, `"item.sword"`)
- `en` — Texto en ingles
- `es` — Texto en espanol

### `Assets/Assets/Resources/GameData/config/localization.json`

JSON con todas las traducciones. Estructura:

```json
{
    "defaultLanguage": "en",
    "entries": [
        { "key": "menu.play", "en": "Play", "es": "Jugar" },
        ...
    ]
}
```

Categorias incluidas:
- `menu.*` — Play, Settings, Credits, Quit, Resume, MainMenu, Language
- `hud.*` — HP, MP, XP, Level
- `item.*` — Sword, Shield, Health Potion, Mana Potion
- `enemy.*` — Zombie, Skeleton
- `combat.*` — CriticalHit, Miss

## Archivos modificados

### `Assets/Scripts/ClassJson/Game.cs`

Se agrego el campo `localization` al `GameConfig`:

```csharp
public class GameConfig
{
    public string player;
    public string attacks;
    public string progression;
    public string items;
    public string enemies;
    public string stats;
    public string spawners;
    public string localization;  // ← nuevo
}
```

### `Assets/Scripts/ConfigBoostrap.cs`

Se agrego:
1. Campo publico `LocalizationConfig`
2. carga con `LoadConfig<LocalizationConfig>()`
3. Llamada a `BuildLookup()` para indexar las traducciones

```csharp
public LocalizationConfig LocalizationConfig;

// dentro de Initialize():
LocalizationConfig = LoadConfig<LocalizationConfig>(gameConfig.localization);
LocalizationConfig.BuildLookup();
```

### `Assets/Resources/GameData/Config/game.json`

Se agrego la ruta al config de localizacion:

```json
{
    ...,
    "localization": "GameData/Config/localization"
}
```

## Como usar

### Obtener un texto localizado

```csharp
// Idioma por defecto (ingles)
string texto = ConfigBoostrap.Current.LocalizationConfig.Get("menu.play");
// → "Play"

// Idioma especifico
string texto = ConfigBoostrap.Current.LocalizationConfig.Get("menu.play", "es");
// → "Jugar"
```

### Para usar en UI (TextMeshPro)

```csharp
using TMPro;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playButton;

    void Start()
    {
        playButton.text = ConfigBoostrap.Current.LocalizationConfig.Get("menu.play", "es");
    }
}
```

## Como agregar un nuevo idioma

### Paso 1: Agregar la columna en el JSON

En `localization.json`, agregar el campo del nuevo idioma en cada entrada:

```json
{ "key": "menu.play", "en": "Play", "es": "Jugar", "pt": "Jogar" }
```

### Paso 2: Actualizar la clase C#

En `Localization.cs`, dentro del metodo `BuildLookup()`, agregar:

```csharp
if (entry.pt != null) langs["pt"] = entry.pt;
```

Y en `LocalizationEntry` agregar el campo:

```csharp
public string pt;
```

### Paso 3: Usar

```csharp
ConfigBoostrap.Current.LocalizationConfig.Get("menu.play", "pt");
// → "Jogar"
```

## Como agregar nuevas traducciones

Solo agregar una entrada en `localization.json`:

```json
{ "key": "menu.newFeature", "en": "New Feature", "es": "Nueva Funcion" }
```

No hace falta tocar codigo C#. La key ya esta disponible via `Get()`.

## Flujo de inicializacion

```
GameBootstrap.Start()
  └─ new ConfigBoostrap().Initialize()
       ├─ Resources.Load("GameData/Config/game")  → GameConfig (rutas)
       ├─ Resources.Load("GameData/Config/localization") → LocalizationConfig (JSON crudo)
       └─ LocalizationConfig.BuildLookup()  → Diccionario indexado por key
            └─ Listo para usar con Get(key, language)
```

## Notas

- El sistema usa `JsonUtility` de Unity, asi que los campos deben ser publicos y no usar propiedades con get/set
- `Get()` nunca devuelve null: si no encuentra la traduccion, devuelve la key como fallback
- `BuildLookup()` se una vez al inicio para no repetir busquedas en cada `Get()`
- El idioma default funciona como fallback si una traduccion no esta disponible en el idioma pedido
