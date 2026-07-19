# Plan: Mejora del Sistema de Spawners

## Estado Actual

El sistema actual es muy básico:
- **Un solo spawner** en la escena (`SpawnerTest` en origen)
- **Spawn único** en `Start()` — no hay respawn ni spawning continuo
- **Config hardcoded** en el Inspector de Unity (amount=10, radius=10, enemyId="zombie")
- **Sin sistema de oleadas**, sin managers, sin escalado de dificultad
- **Sin re-spawn** — cuando un mob muere, se destruye permanentemente

---

## Objetivo

Convertir el spawner en un sistema robusto, configurable y reutilizable:
1. **Fase actual**: Respawn constante de mobs eliminados con intervalos configurables
2. **Fase futura**: Sistema de oleadas con dificultad progresiva (dejar hooks preparados)
3. Configuración externa via JSON (consistente con el resto del proyecto)
4. Soporte para múltiples spawners sin límite, con configuraciones independientes
5. Spawn de diferentes tipos de enemigos por spawner (selección por peso)

---

## Decisiones Tomadas

| Decisión | Respuesta |
|----------|-----------|
| Modo de spawn | **Respawn constante** (hooks para oleadas a futuro) |
| Multiplicadores | **Multiplican** stats base (no se suman) |
| Posición de spawners | **Desde JSON** (fuente primaria, no Inspector) |
| Timing de spawn | **Por intervalos** (timer configurable) |
| Límite de spawners | **Sin límite** |

---

## Cambios Propuestos

### 1. Configuración JSON del Spawner

**Nuevo archivo**: `Assets/Resources/GameData/Config/spawners.json`

```json
{
  "spawners": [
    {
      "id": "cemetery_01",
      "position": [0, 0, 0],
      "radius": 10,
      "maxAlive": 10,
      "spawnInterval": 3.0,
      "respawnDelay": 5.0,
      "healthMultiplier": 1.0,
      "damageMultiplier": 1.0,
      "enemyTypes": [
        { "enemyId": "zombie", "weight": 80 },
        { "enemyId": "skeleton", "weight": 20 }
      ],
      "waves": null
    }
  ]
}
```

**Campos clave**:
| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | string | Identificador único del spawner |
| `position` | float[3] | Posición world desde JSON |
| `radius` | float | Radio de spawn y patrol |
| `maxAlive` | int | Máximo de mobs vivos simultáneamente |
| `spawnInterval` | float | Segundos entre cada spawn |
| `respawnDelay` | float | Segundos de espera antes de respawnear un mob muerto |
| `healthMultiplier` | float | Multiplica la vida base del enemigo |
| `damageMultiplier` | float | Multiplica el daño base del enemigo |
| `enemyTypes` | array | Pool de enemigos con pesos para selección aleatoria |
| `waves` | object/null | **Reservado para futuro** — null = modo respawn constante |

**Selección de enemigo por peso**: Se elige un tipo de enemigo al azar respetando los pesos. Ej: zombie=80, skeleton=20 → 80% zombies, 20% esqueletos.

**Agregar enemigos a `enemies.json`** — agregar campos `tags` y `prefabPath`:
```json
{
  "id": "zombie",
  "tags": ["undead", "melee", "slow"],
  "prefabPath": "Prefabs/Mobs/Zombie",
  ...
}
```

El `SpawnerManager` carga el prefab desde Resources según el `enemyId` del spawner. El prefab contiene el modelo 3D del enemigo.

---

### 2. Refactorizar `PrefabSpawner.cs`

**Archivo**: `Assets/Scripts/PrefabSpawner.cs`

Cambios:
- Eliminar los campos serializados hardcodeados (`amount`, `radius`, `enemyId`)
- Recibir config desde `SpawnerConfig` (cargado del JSON por `SpawnerManager`)
- Posición del spawner viene del JSON, no del Inspector
- Lógica de respawn constante con `spawnInterval` y `maxAlive`
- Trackear mobs vivos en `List<Mob>` para saber cuándo respawnear
- **Hook para futuro**: interfaz `ISpawnMode` con implementación actual `RespawnConstantMode` y futura `WaveMode`

**Pseudocódigo de la nueva lógica**:
```
Initialize(spawnerConfig):
  Guardar config (id, position, radius, maxAlive, spawnInterval, etc.)
  Mover GameObject a position del JSON
  Spawnear cantidad inicial = maxAlive

Update():
  Si timer >= spawnInterval AND vivos.Count < maxAlive:
    Seleccionar tipo de enemigo (por peso)
    Instanciar mob con multiplicadores de config
    Agregar a vivos
    Reset timer

OnMobDied(mob):
  Remover de vivos
  // El respawn se maneja por el intervalo en Update()
```

**Hook futuro para oleadas** (no implementar aún, solo preparar la estructura):
```csharp
// Interfaz para modos de spawn (futuro)
public interface ISpawnMode
{
    void Initialize(SpawnerConfig config, System.Action<Mob> onSpawn);
    void Update(float deltaTime);
    void OnMobDied(Mob mob);
}

// Fase actual: modo respawn constante
public class RespawnConstantMode : ISpawnMode { ... }

// Fase futura: modo oleadas
public class WaveMode : ISpawnMode { ... }
```

---

### 3. Nuevo Script: `SpawnerManager.cs`

**Archivo**: `Assets/Scripts/SpawnerManager.cs`

Responsabilidades:
- Singleton que coordina todos los spawners de la escena
- Carga la config JSON de spawners al inicio
- Carga prefabs de enemigos desde Resources según `prefabPath` de `enemies.json`
- Instancia spawners根据 la config (posición, prefab, config)
- Registra `PrefabSpawner` al momento de crearlos
- Expone eventos: `OnMobSpawned`, `OnMobDied`
- Sin límite de spawners simultáneos

```csharp
public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance { get; private set; }
    
    private SpawnersConfig config;
    private EnemiesConfig enemiesConfig;
    private List<PrefabSpawner> activeSpawners = new();
    
    // Cache de prefabs cargados por enemyId
    private Dictionary<string, GameObject> prefabCache = new();
    
    void Awake() => Instance = this;
    
    public void RegisterSpawner(PrefabSpawner spawner);
    public SpawnerConfig GetConfig(string spawnerId);
    public GameObject GetPrefab(string enemyId); // carga desde enemies.json prefabPath
    public void OnMobDied(Mob mob);
    public int GetTotalAliveMobs();
}
```

**Flujo de instanciación**:
1. `SpawnerManager.Awake()` carga `spawners.json` y `enemies.json`
2. Para cada spawner en la config, instancia un GameObject con `PrefabSpawner`
3. `PrefabSpawner.Initialize(config)` recibe su config individual
4. Al spawneear un mob, `PrefabSpawner` pide el prefab a `SpawnerManager.GetPrefab(enemyId)`
5. `SpawnerManager` busca `prefabPath` en `enemies.json` y carga con `Resources.Load()`

---

### 4. Nuevo Script: `Spawners.cs` (Data Model)

**Archivo**: `Assets/Scripts/ClassJson/Spawners.cs`

Modelo de datos para deserializar `spawners.json`:

```csharp
[Serializable]
public class SpawnersConfig
{
    public List<SpawnerConfig> spawners;
}

[Serializable]
public class SpawnerConfig
{
    public string id;
    public float[] position;
    public float radius;
    public int maxAlive;
    public float spawnInterval;
    public float respawnDelay;
    public float healthMultiplier;
    public float damageMultiplier;
    public List<EnemyTypeWeight> enemyTypes;
    public List<WaveConfig> waves; // null = respawn constante, no nulo = futuro modo oleadas
}

[Serializable]
public class EnemyTypeWeight
{
    public string enemyId;
    public int weight;
}

// Reservado para futuro — no se implementa aún
[Serializable]
public class WaveConfig
{
    public int waveNumber;
    public List<EnemyTypeWeight> enemyTypes;
    public int totalSpawns;
    public float spawnInterval;
    public float healthMultiplier;
    public float damageMultiplier;
}
```

---

### 5. Agregar `tags` a `enemies.json`

**Archivo**: `Assets/Resources/GameData/Config/enemies.json`

Agregar campo `tags` a cada enemigo para permitir filtrado futuro por spawner:

```json
{
  "enemies": [
    {
      "id": "zombie",
      "tags": ["undead", "melee", "slow"],
      "prefabPath": "Prefabs/Mobs/Zombie",
      "health": 120,
      ...
    },
    {
      "id": "skeleton",
      "tags": ["undead", "melee", "fast"],
      "prefabPath": "Prefabs/Mobs/Skeleton",
      "health": 80,
      ...
    }
  ]
}
```

**Modelo** en `Enemies.cs` — agregar campos:
```csharp
public List<string> tags;
public string prefabPath; // ruta en Resources para cargar el prefab
```

---

### 6. Integración con `Mob.cs`

**Archivo**: `Assets/Scripts/Mob/Mob.cs`

Cambios:
- Recibir `healthMultiplier` y `damageMultiplier` vía `MobSpawnData`
- Al morer, notificar a `SpawnerManager.OnMobDied(this)` antes de `Destroy()`

---

### 7. Integración con `MobSpawnData.cs`

**Archivo**: `Assets/Scripts/Mob/Data/MobSpawnData.cs`

Expandir con multiplicadores de spawner:

```csharp
public struct MobSpawnData
{
    public Vector3 SpawnPosition;
    public float PatrolRadius;
    public string EnemyId;
    public float HealthMultiplier;  // nuevo
    public float DamageMultiplier;  // nuevo
}
```

---

### 8. Integración con `MobResources.cs`

**Archivo**: `Assets/Scripts/Mob/MobResources.cs`

Al inicializar, aplicar multiplicador:
```csharp
maxHealth = baseHealth * healthMultiplier;
currentHealth = maxHealth;
```

---

### 9. Integración con `ConfigBootstrap.cs`

**Archivo**: `Assets/Scripts/ConfigBoostrap.cs`

Agregar carga de `spawners.json`:
```csharp
public SpawnersConfig SpawnersConfig { get; private set; }

void Initialize()
{
    // ... carga existente ...
    SpawnersConfig = JsonUtility.FromJson<SpawnersConfig>(
        Resources.Load<TextAsset>("GameData/Config/spawners").text);
}
```

Registrar en `game.json`:
```json
{
  "configs": {
    "spawners": "GameData/Config/spawners.json"
  }
}
```

---

### 10. Limpiar escena

**Archivo**: `Assets/OutdoorsScene.unity`

- Remover componente `PrefabSpawner` del GameObject `SpawnerTest`
- Los spawners ahora se instancian desde `SpawnerManager`根据 el JSON
- El GameObject `SpawnerTest` puede eliminarse o dejarse vacío

---

## Archivos a Crear

| Archivo | Descripción |
|---------|-------------|
| `Assets/Scripts/ClassJson/Spawners.cs` | Modelos de datos para spawner config |
| `Assets/Scripts/SpawnerManager.cs` | Singleton manager + instanciación de spawners |
| `Assets/Resources/GameData/Config/spawners.json` | Config JSON de spawners |

## Archivos a Modificar

| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/PrefabSpawner.cs` | Refactor completo: respawn constante, maxAlive, config desde JSON |
| `Assets/Scripts/Mob/Data/MobSpawnData.cs` | Agregar healthMultiplier, damageMultiplier |
| `Assets/Scripts/Mob/Mob.cs` | Notificar muerte a SpawnerManager |
| `Assets/Scripts/Mob/MobResources.cs` | Aplicar healthMultiplier al init |
| `Assets/Scripts/ClassJson/Enemies.cs` | Agregar campo `tags` |
| `Assets/Resources/GameData/Config/enemies.json` | Agregar tags a cada enemigo |
| `Assets/Scripts/ConfigBoostrap.cs` | Cargar spawners.json |
| `Assets/Resources/GameData/Config/game.json` | Agregar path a spawners config |

## Archivos sin Cambio (referencia)

| Archivo | Razón |
|---------|-------|
| `MobAI.cs` | Usa SpawnPosition de MobSpawnData (sin cambios) |
| `ReturnToSpawnState.cs` | Usa SpawnPosition (sin cambios) |
| `ChaseState.cs` / `AttackState.cs` | Leash logic (sin cambios) |

---

## Orden de Implementación Sugerido

1. **Crear `Spawners.cs`** — modelos de datos (con WaveConfig reservado para futuro)
2. **Crear `spawners.json`** — config inicial con 1 spawner
3. **Modificar `game.json`** — registrar path
4. **Modificar `ConfigBootstrap.cs`** — cargar config
5. **Modificar `Enemies.cs` + `enemies.json`** — agregar tags
6. **Modificar `MobSpawnData.cs`** — agregar multiplicadores
7. **Crear `SpawnerManager.cs`** — manager singleton + instanciación
8. **Refactorizar `PrefabSpawner.cs`** — lógica de respawn constante
9. **Modificar `Mob.cs`** — notificación de muerte
10. **Modificar `MobResources.cs`** — aplicar healthMultiplier
11. **Limpiar escena** — eliminar spawner hardcodeado

---

---

# FASE 2: SISTEMA DE OLEADAS (FUTURO)

> Esta sección documenta cómo activar el modo de oleadas cuando se decida.
> La implementación actual ya deja hooks preparados para esta fase.

---

## ¿Qué Cambia?

| Aspecto | Respawn Constante (Fase 1) | Oleadas (Fase 2) |
|---------|---------------------------|-------------------|
| Comportamiento | Siempre spawnea mientras haya huecos | Spawnea una ola, espera a que se limpien, avanza |
| `waves` en JSON | `null` | Array con configs por ola |
| Fin del ciclo | Nunca termina | Puede terminar después de la última ola |
| Dificultad | Constante | Escala con cada ola |

---

## Flujo Visual

```
OLADA 1 (fácil)
├── Spawnea 5 zombies (80%) + 2 esqueletos (20%)
├── Spawn cada 3 segundos
├── Mult vida: x1.0  |  Mult daño: x1.0
├── Espera a que MATEN a todos
│
OLADA 2 (medio)
├── Spawnea 8 zombies (60%) + 4 esqueletos (40%)
├── Spawn cada 2.5 segundos
├── Mult vida: x1.2  |  Mult daño: x1.3
├── Espera a que MATEN a todos
│
OLADA 3 (difícil)
├── Spawnea 10 zombies (40%) + 8 esqueletos (60%)
├── Spawn cada 2 segundos
├── Mult vida: x1.5  |  Mult daño: x1.7
├── Espera a que MATEN a todos
│
...sigue creciendo
```

---

## Config JSON (modo oleadas)

Cambiar `waves: null` por un array de oleadas:

```json
{
  "spawners": [
    {
      "id": "cemetery_01",
      "position": [0, 0, 0],
      "radius": 10,
      "maxAlive": 10,
      "spawnInterval": 3.0,
      "respawnDelay": 5.0,
      "healthMultiplier": 1.0,
      "damageMultiplier": 1.0,
      "enemyTypes": [
        { "enemyId": "zombie", "weight": 80 },
        { "enemyId": "skeleton", "weight": 20 }
      ],
      "waves": [
        {
          "waveNumber": 1,
          "enemyTypes": [
            { "enemyId": "zombie", "weight": 80 },
            { "enemyId": "skeleton", "weight": 20 }
          ],
          "totalSpawns": 7,
          "spawnInterval": 3.0,
          "healthMultiplier": 1.0,
          "damageMultiplier": 1.0
        },
        {
          "waveNumber": 2,
          "enemyTypes": [
            { "enemyId": "zombie", "weight": 60 },
            { "enemyId": "skeleton", "weight": 40 }
          ],
          "totalSpawns": 12,
          "spawnInterval": 2.5,
          "healthMultiplier": 1.2,
          "damageMultiplier": 1.3
        },
        {
          "waveNumber": 3,
          "enemyTypes": [
            { "enemyId": "zombie", "weight": 40 },
            { "enemyId": "skeleton", "weight": 60 }
          ],
          "totalSpawns": 18,
          "spawnInterval": 2.0,
          "healthMultiplier": 1.5,
          "damageMultiplier": 1.7
        }
      ]
    }
  ]
}
```

---

## Lógica Interna (WaveMode)

```
Initialize(config):
  waves = config.waves
  currentWaveIndex = 0
  spawnsRestantes = waves[0].totalSpawns
  vivos = []

Update():
  Si currentWaveIndex >= waves.Length:
    // Todas las oleadas completadas → detener o volver a respawn constante
    return

  wave = waves[currentWaveIndex]

  // Spawneando en esta ola
  Si timer >= wave.spawnInterval AND vivos.Count < maxAlive AND spawnsRestantes > 0:
    Seleccionar enemigo por peso de wave.enemyTypes
    Spawnear con wave.healthMultiplier y wave.damageMultiplier
    spawnsRestantes--
    Reset timer

  // Ola completada: todos muertos y sin spawns pendientes
  Si spawnsRestantes == 0 AND vivos.Count == 0:
    currentWaveIndex++
    Si currentWaveIndex < waves.Length:
      spawnsRestantes = waves[currentWaveIndex].totalSpawns
      // Pausa breve entre oleadas (opcional)
    Si no:
      // Oleadas agotadas → detener spawner

OnMobDied(mob):
  Remover de vivos
```

---

## Estados del Spawner

```
┌─────────────┐
│  RESPAWN     │ ◄── Modo actual (Fase 1)
│  CONSTANTE   │     Siempre spawnea, nunca termina
└──────┬──────┘
       │ waves != null
       ▼
┌─────────────┐
│   WAVE 1    │ ──spawn──►  vivos > 0  ──espera──┐
└──────┬──────┘                                    │
       │ spawns=0, vivos=0                        │
       ▼                                          │
┌─────────────┐                                   │
│   WAVE 2    │ ──spawn──►  vivos > 0  ──espera──┤
└──────┬──────┘                                    │
       │ spawns=0, vivos=0                        │
       ▼                                          │
┌─────────────┐                                   │
│   WAVE 3    │ ──spawn──►  ...                   │
└──────┬──────┘                                    │
       │ todas las oleadas completadas             │
       ▼                                          │
┌─────────────┐                                   │
│  TERMINADO  │ ◄── o volver a RESPAWN_CONSTANTE ─┘
└─────────────┘
```

---

## Escalado de Oleadas

Cada ola puede escalar de formas distintas:

| Qué escala | Ejemplo |
|------------|---------|
| **Cantidad** | Ola 1: 7, Ola 2: 12, Ola 3: 18 |
| **Mezcla** | Ola 1: 80% zombie, Ola 3: 60% skeleton |
| **Intervalo** | Ola 1: 3s, Ola 3: 2s (más rápido = más presión) |
| **Stats** | Ola 3: vida x1.5, daño x1.7 |

---

## Pasos para Activar (Fase 2)

Cuando se decida implementar oleadas:

1. **Crear `WaveMode.cs`** — implementar `ISpawnMode` con la lógica de oleadas
2. **En `PrefabSpawner.Initialize()`** — cambiar detección:
   ```csharp
   if (config.waves != null && config.waves.Count > 0)
       spawnMode = new WaveMode(config);
   else
       spawnMode = new RespawnConstantMode(config);
   ```
3. **Actualizar `spawners.json`** — cambiar `waves: null` → `waves: [...]`
4. **Opcional**: Agregar UI de oleada actual, pausa entre oleadas, recompensas

> No se toca `SpawnerManager.cs` ni `Mob.cs` — la interfaz `ISpawnMode` ya absorbe la diferencia.
