# MobAI - Ficha Tecnica

## Resumen

Sistema de IA para mobs con maquina de estados, deteccion de aggro por radio y combate basado en `CombatService`.

## Arquitectura

```
Mob (MonoBehaviour)
  ├─ MobAI (logica, estados, deteccion)
  │    ├─ PatrolState
  │    ├─ ChaseState
  │    ├─ AttackState
  │    └─ ReturnToSpawnState
  ├─ MobMovement (NavMeshAgent wrapper)
  └─ MobResources (HP, muerte)
```

## Maquina de Estados

```
                  ┌─────────────┐
                  │ PatrolState │ ◄──────────────────────┐
                  └──────┬──────┘                        │
                         │ Target != null                │
                         ▼                               │
                  ┌─────────────┐                        │
                  │ ChaseState  │                        │
                  └──┬──────┬───┘                        │
     AttackRange     │      │  LoseTargetRange           │
                     ▼      │  o Target null             │
              ┌──────────┐  │  o Target muerto           │
              │AttackState│  │                            │
              └──────┬───┘  │                            │
                     │      │                            │
                     │      ▼                            │
                     │ ┌──────────────────┐              │
                     └►│ReturnToSpawnState│──────────────┘
                       └──────────────────┘
```

### PatrolState
- Elige un punto aleatorio dentro de `PatrolRadius` desde `SpawnPosition`
- Se mueve hacia el punto; al llegar, elige otro
- Transiciona a `ChaseState` cuando `Target != null`

### ChaseState
- Sigue al `Target` usando `NavMeshAgent`
- Pierde el target si:
  - `Target == null`
  - El target esta muerto (`player.IsAlive == false`)
  - La distancia al spawn supera `LoseTargetRange`
- Transiciona a `AttackState` cuando `distanceToTarget <= AttackRange`

### AttackState
- Se detiene (`NavMeshAgent.ResetPath()`)
- Cada `AttackCooldown` (1s) aplica dano via `CombatService`
- Pierde el target si la distancia al spawn supera `LoseTargetRange`
- Transiciona a `ChaseState` si el target se aleja de `AttackRange`

### ReturnToSpawnState
- Se mueve de vuelta a `SpawnPosition`
- Al llegar, transiciona a `PatrolState`

## Deteccion de Aggro

Implementada en `MobAI.DetectAggro()`, ejecutada cada frame antes del tick del estado actual.

```
Physics.OverlapSphere(Position, AggroRange)
  → Filtra colliders con componente Player
  → Descarta jugadores muertos
  → Selecciona el mas cercano
  → Asigna a Target
```

La deteccion solo busca cuando `Target == null` (no cambia de target si ya tiene uno).

## Configuracion (JSON)

Valores en `enemies.json` por tipo de mob:

| Propiedad | Zombie | Skeleton | Descripcion |
|---|---|---|---|
| `aggroRange` | 10 | 12 | Radio de deteccion del jugador |
| `loseTargetRange` | 18 | 20 | Distancia maxima al spawn antes de perder target |
| `attackRange` | 2 | 2.5 | Distancia para entrar en AttackState |

### Enemy class (`Enemies.cs`)

```csharp
public float aggroRange;
public float loseTargetRange;
public float attackRange;
```

Se plombean a `MobAI` en `Mob.Initialize()` despues de resolver el config.

## Combate

- `AttackState` usa `CombatService.Attack(attacker, target, attack)`
- El ataque del mob es `Physical` con `damageMultiplier = 1.0`
- Usa `Mob.CombatStats` (del config JSON) como stat de ataque
- El dano se aplica al `Player` via `ICombatEntity.TakeDamage()`

## Multiplayer - Soporte Actual

### Que funciona
- Cada mob detecta jugadores independientemente via `OverlapSphere`
- `Physics.OverlapSphere` retorna TODOS los colliders en rango, incluyendo multiples jugadores
- El mob selecciona al jugador mas cercano como target
- Si el target muere, vuelve a patrol y puede detectar otro jugador

### Que NO funciona today
- **Sin sincronizacion de estados**: Cada cliente ve la maquina de estados localmente. No hay red.
- **Sin target sharing**: Si dos jugadores estan en rango, el mob solo persigue al mas cercano. No hay sistema de aggro table (prioridad por quien ataco primero, etc.)
- **Sin validacion server-side**: El dano se aplica localmente. No hay autoridad del servidor.
- **Sin interpolacion de movimiento**: Otros clientes ven teleportation del mob.

### Cosas a tener en cuenta para multiplayer futuro
- `DetectAggro()` usa `Physics.OverlapSphere` que es local. Para multiplayer se necesita:
  - Server-authoritative aggro detection
  - Sincronizacion de `Target` entre clientes
  - Possession/network identity en el mob
- `CombatService.Attack()` es unmethod pura. Se puede llamar desde el server sin problema.
- Los valores de config (`aggroRange`, `attackRange`, etc.) son compartidos. No hay desync por config.

## Mejoras Futuras

### Prioridad
1. **LayerMask en OverlapSphere** - Actualmente detecta TODO. Usar un LayerMask "Player" para evitar detectar other objects innecesariamente.
2. **Aggro table** - El mob deberia priorizar jugadores que lo atacaron primero, no solo al mas cercano.
3. **Cooldown de deteccion** - `DetectAggro()` corre cada frame. Podria ejecutarse cada N frames o con un timer para performance con muchos mobs.
4. **Animaciones de ataque** - `AttackState` no tiene animaciones. Falta integrar con `MobAnimationController` similar al player.
5. **Sonidos de combate** - No hay feedback de audio.
6. **Knockback / Hit reaction** - Los mobs no reaccionan a ser golpeados (stagger, knockback).
7. **Ranged attacks** - La IA actual solo soporta melee. Para mobs ranged se necesitaria un `RangedAttackState` con proyectiles.
8. **Leash dinamico** - Actualmente `loseTargetRange` es fijo. Podria escalarse con la velocidad del mob o el tipo de ataque.
9. **Habilitar/deshabilitar aggro por mob type** - Algunos mobs pacificos no deberian tener deteccion. Agregar un flag `canAggro` al config.
10. **Vision cone** - `OverlapSphere` detecta en 360 grados. Un cono de vision (dot product con forward) haria la deteccion mas realista.

## Archivos Relacionados

| Archivo | Rol |
|---|---|
| `Scripts/Mob/Mob.cs` | MonoBehaviour principal, inicializacion, ICombatEntity |
| `Scripts/Mob/MobAI.cs` | Logica de IA, estados, deteccion de aggro |
| `Scripts/Mob/MobMovement.cs` | Wrapper de NavMeshAgent |
| `Scripts/Mob/MobResources.cs` | HP, muerte, eventos |
| `Scripts/Mob/Data/MobSpawnData.cs` | Datos de spawn (position, patrolRadius, enemyId) |
| `Scripts/Mob/MachineState/PatrolState.cs` | Estado de patrullaje |
| `Scripts/Mob/MachineState/ChaseState.cs` | Estado de persecucion |
| `Scripts/Mob/MachineState/AttackState.cs` | Estado de ataque melee |
| `Scripts/Mob/MachineState/ReturnToSpawnState.cs` | Estado de retorno al spawn |
| `Scripts/ClassJson/Enemies.cs` | Clase Enemy con config del JSON |
| `Scripts/Service/CombatService.cs` | Calculo de dano (attacker stats - target defense) |
| `Resources/GameData/Config/enemies.json` | Config de stats por tipo de mob |
| `Scripts/PrefabSpawner.cs` | Spawner que crea mobs y llama Initialize |
