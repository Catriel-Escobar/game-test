# Plan: Sistema de Skills para el Player

> Documento co-diseñado con el equipo. Las decisiones marcadas en **Decisiones de Diseño** son el contrato inicial del sistema; el catálogo de contenido al final es para futuras iteraciones.

---

## Estado Actual

El proyecto ya tiene **infraestructura parcial** de skills sin implementar:

| Pieza | Estado |
|-------|--------|
| `skillPointsPerLevel` en `progression.json` | Existe pero **inert** (se mantiene sin uso) |
| `Attack.cooldown` / `Attack.manaCost` en `attacks.json` | Existen pero **no se verifican** en combate |
| `UnlockedAttackIds` en `Player.cs` | Patrón de desbloqueo persistido — se reutiliza para skills |
| `ConsumeMana()` en `PlayerResources.cs` | Existe pero no se llama en combate |
| `AttackWindow` en `PlayerCombat.cs` | Patrón de ventana de duración — base para el cast time |
| `CombatService.Attack()` | Punto único de cálculo de daño — reutilizable |
| `AttackDamageType` (Physical/Magical) | Ya discrimina daño físico vs mágico |

---

## Decisiones de Diseño (Contrato Inicial)

| # | Decisión | Detalle |
|---|----------|---------|
| 1 | **Tipo de skills** | Solo **activas** (el jugador castea con una tecla). Sin pasivas, canalización ni procs en el sistema base. |
| 2 | **Clases** | **100% por clase** — cada clase (warrior/mage) tiene su set de skills exclusivo. No hay pool común. |
| 3 | **Adquisición** | **Unlock plano** — la skill se desbloquea automáticamente al alcanzar `requiresLevel` de personaje. Sin gastar puntos ni subir niveles individuales. |
| 4 | **Recurso** | Solo **mana**. |
| 5 | **Mana regen** | **Regen pasivo de mana** (mana/s) — hoy no existe ningún regen, se agrega. |
| 6 | **Regulación** | **Cooldown por skill** (independiente por skill). Sin cooldown global. |
| 7 | **Targeting** | **self** (AoE alrededor del jugador), **mouse** (AoE en posición del cursor), **dirección de movimiento** (AoE/desplazamiento hacia donde se mueve). Sin auto-aim. |
| 8 | **Casteo** | **Con cast time** — cada skill tiene una duración de casteo donde el jugador queda casteando (patrón de attack window actual) y el efecto se produce al completarse. |
| 9 | **Categorías de efecto** | **Daño**, **self-buffs**, **movimiento (dash)**, **CC (stun/slow)**. Sin curación. |
| 10 | **Buffs/debuffs** | Solo **self-buffs** por ahora (modifican stats del jugador por duración). Debuffs a enemigos más adelante. |
| 11 | **Movimiento** | **Dash del jugador** que **stunea a los enemigos** con los que colisiona durante el dash. |
| 12 | **Efectos combinados** | Cada skill puede **encadenar múltiples efectos** (ej: daño + slow, o dash + stun). |
| 13 | **Status effects** | Sistema de **status effects con duración y tick** desde el inicio (stun, slow, buffs). |
| 14 | **Proyectiles** | **No** por ahora — las skills de rango son **AoE instantáneo en la posición del mouse**. Proyectiles quedan fuera de scope. |
| 15 | **UI** | **Hotbar de 3 slots (Q/E/R)** con cooldown overlay y aviso de mana, **notificación de nuevo unlock**, y **spellbook** (panel con skills de la clase). |
| 16 | **Skill points** | **Inertes** — se mantiene el campo pero no se consume. |
| 17 | **Data model** | **JSON** (`skills.json`) cargado con JsonUtility, consistente con el proyecto. |

---

## Pipeline de Casting

```
Input (Q/E/R) → PlayerInputs → PlayerSkills.TryCastSkill(id)
  │
  ├─ 1. ¿Skill aprendida?            (unlocked, según requiresLevel)
  ├─ 2. ¿Cooldown listo?            (SkillCooldownManager)
  ├─ 3. ¿Mana suficiente?           (PlayerResources — consumir al INICIO del cast)
  │
  ▼
  CAST TIME (duración de casteo, jugador casteando)
  ├─ Bloquea/estado de casting (reusar patrón AttackWindow de PlayerCombat)
  ├─ Animación de casteo (trigger por skill)
  │
  ▼
  AL COMPLETAR EL CAST:
  ├─ Cooldown empieza (timing: al completar, no al iniciar)
  ├─ Resolver targeting (self / mouse / dirección)
  └─ Ejecutar efectos EN CADENA (ISkillEffect list)
      ├─ Daño → CombatService.Attack()
      ├─ Self-buff → StatusEffectManager (aplica al jugador)
      ├─ Dash → movimiento + detecta colisiones → StunEffect a enemigos
      └─ CC → StatusEffectManager (aplica a enemigos)
  └─ Feedback UI (cooldown, números de daño, VFX)
```

---

## Modelo de Datos (skills.json)

```json
{
  "skills": [
    {
      "id": "whirlwind",
      "nameKey": "skill.whirlwind",
      "descriptionKey": "skill.whirlwind.desc",
      "classId": "warrior",
      "requiresLevel": 3,
      "manaCost": 15,
      "cooldown": 8.0,
      "castTime": 0.6,
      "animationId": "Whirlwind",
      "targeting": "self",
      "effects": [
        {
          "type": "damage_aoe",
          "damageType": 0,
          "damageMultiplier": 1.4,
          "radius": 3.0,
          "range": 0.0
        },
        {
          "type": "slow",
          "duration": 2.0,
          "slowPercent": 0.3
        }
      ]
    },
    {
      "id": "dash_strike",
      "nameKey": "skill.dash_strike",
      "classId": "warrior",
      "requiresLevel": 5,
      "manaCost": 12,
      "cooldown": 10.0,
      "castTime": 0.2,
      "animationId": "Dash",
      "targeting": "move_dir",
      "effects": [
        {
          "type": "dash",
          "distance": 5.0,
          "speed": 18.0
        },
        {
          "type": "stun",
          "duration": 1.0,
          "radius": 1.2
        }
      ]
    },
    {
      "id": "meteor",
      "nameKey": "skill.meteor",
      "classId": "mage",
      "requiresLevel": 7,
      "manaCost": 40,
      "cooldown": 15.0,
      "castTime": 1.2,
      "animationId": "CastMeteor",
      "targeting": "mouse",
      "effects": [
        {
          "type": "damage_aoe",
          "damageType": 1,
          "damageMultiplier": 2.5,
          "radius": 3.5,
          "range": 12.0
        },
        {
          "type": "stun",
          "duration": 1.5,
          "radius": 3.5
        }
      ]
    }
  ]
}
```

### Modelo: `targeting`

| Valor | Resolución al completar el cast |
|-------|----------------------------------|
| `self` | Centro = posición del jugador |
| `mouse` | Centro = punto del mouse en el mundo (limitado por `range`) |
| `move_dir` | Dirección de movimiento (o facing si está quieto) |

### Modelo: `effects[]`

Lista ordenada de efectos. Cada efecto es un objeto con `type` + sus parámetros:

| `type` | Parámetros | Qué hace |
|--------|-----------|----------|
| `damage_aoe` | `damageType`, `damageMultiplier`, `radius`, `range` | Daño en área. `range=0` = centrado en el jugador (self), `>0` = AoE en mouse |
| `self_buff` | `statId`, `percent`, `duration` | Modifica un stat del jugador por duración |
| `dash` | `distance`, `speed` | Desplaza al jugador en dirección de movimiento |
| `stun` | `duration`, `radius` | Stunea enemigos en el área/recorrido |
| `slow` | `duration`, `slowPercent` | Ralentiza enemigos en el área |

> El efecto `dash` + `stun` combinados implementan el "dash que stunea" (el dash recorre el camino y stunea enemigos golpeados).

---

## Estructura de Clases Sugerida

```text
Assets/Scripts/Skills/
├── PlayerSkills.cs                  # Fachada: skills desbloqueadas por nivel, TryCastSkill
├── SkillsConfig.cs                  # Contenedor deserializado de skills.json
├── SkillData.cs                     # Modelo: SkillDefinition + SkillEffectDefinition
├── SkillUnlockService.cs            # Otorga skills al subir de nivel (escucha Progression)
├── SkillCooldownManager.cs          # Timers por skill (Dictionary<string, float>)
├── SkillCaster.cs                   # Valida unlock/mana/cooldown + cast time window
├── Targeting/
│   ├── ISkillTargeting.cs           # Resuelve centro/dirección según targeting
│   ├── SelfTargeting.cs
│   ├── MouseTargeting.cs
│   └── MoveDirectionTargeting.cs
├── Effects/
│   ├── ISkillEffect.cs              # Apply(SkillContext context)
│   ├── SkillContext.cs              # Player, origen, dirección, stats, daño
│   ├── DamageAreaEffect.cs
│   ├── SelfBuffEffect.cs
│   ├── DashEffect.cs
│   ├── StunEffect.cs
│   └── SlowEffect.cs
├── StatusEffects/
│   ├── StatusEffectManager.cs       # Aplica, tickea y expira efectos
│   ├── StatusEffect.cs              # tipo, duración, tick, intensidad
│   ├── StunStatusEffect.cs
│   ├── SlowStatusEffect.cs
│   └── BuffStatusEffect.cs          # self-buffs sobre stats
├── Regen/
│   └── ManaRegen.cs                 # Regen pasivo mana/s
└── UI/
    ├── SkillHotbarUI.cs             # 3 slots
    ├── SkillSlotUI.cs               # icono + cooldown overlay + mana
    ├── SkillUnlockNotificationUI.cs # aviso al desbloquear
    └── SkillBookUI.cs               # spellbook de la clase
```

---

## Integración con lo existente

| Script existente | Cambio |
|------------------|--------|
| `Player.cs` | Exponer `PlayerSkills`; restaurar desbloqueadas del save |
| `PlayerResources.cs` | Agregar `ManaRegen`; usar `ConsumeMana()` en el cast |
| `CombatService.cs` | Reutilizar `Attack()` para efectos `damage_aoe` |
| `PlayerInputs.cs` | Conectar `Skill1/2/3` (Q/E/R) |
| `InputSystem_Actions.inputactions` | Nuevas acciones `Skill1`, `Skill2`, `Skill3`, `SkillBook` |
| `PlayerAnimationController.cs` | Triggers de casteo por skill (`animationId`) |
| `PlayerCombat.cs` | Reusar el patrón de `AttackWindow` para el cast time |
| `PlayerSaveData.cs` / `GameSaveService.cs` | Persistir `unlockedSkillIds` |
| `ConfigBoostrap.cs` | Cargar `skills.json` |
| `game.json` | Registrar path de `skills.json` |

---

## Orden de Implementación Sugerido

### Fase 0 — Data ✅ Implementada
1. ✅ `SkillData.cs` + `SkillsConfig.cs` (modelos JSON) — `Assets/Scripts/Skills/`
2. ✅ `skills.json` con 4 skills de prueba (2 por clase) — `Assets/Assets/Resources/GameData/Config/skills.json`
3. ✅ Registrar en `game.json` + cargar en `ConfigBoostrap.cs`

### Fase 1 — Desbloqueo (sin casting) ✅ Implementada
4. ✅ `PlayerSkills.cs` — conoce skills de la clase, desbloquea por `requiresLevel` — `Assets/Scripts/Skills/PlayerSkills.cs`
5. ✅ `SkillUnlockService.cs` — escucha `Progression.OnLevelChanged`, desbloquea + notifica (evento `OnSkillUnlocked`) — `Assets/Scripts/Skills/SkillUnlockService.cs`
6. ✅ Persistir `unlockedSkillIds` en save (`PlayerSaveData` + `GameSaveService`) — `classId` llega vía `Player.Initialize(config, saveData, classId)` desde `GameBootstrap`

### Fase 2 — Casting ✅ Implementada
7. ✅ `SkillCooldownManager.cs` — timers por skill — `Assets/Scripts/Skills/SkillCooldownManager.cs`
8. ✅ `SkillCaster.cs` — cast time window (coroutine, patrón AttackWindow), valida unlock/mana/cooldown, consume mana, cooldown al completar — `Assets/Scripts/Skills/SkillCaster.cs`
9. ✅ `Targeting/` — resolución self / mouse / dirección (`ISkillTargeting` + `SelfTargeting` + `MouseTargeting` + `MoveDirectionTargeting` + `SkillTargetingFactory`) — `Assets/Scripts/Skills/Targeting/`
10. ✅ Acciones `Skill1/2/3` en input (Q/E/R + gamepad LB/RB/Y) — wired en `PlayerInputs` vía `GetEquippedSkillIds()` (primeras 3 desbloqueadas en orden del JSON)

### Fase 3 — Efectos ✅ Implementada
11. ✅ `ISkillEffect` + `SkillCastContext` (reusa el contexto de Targeting) — `Assets/Scripts/Skills/Effects/ISkillEffect.cs`
12. ✅ `DamageAreaEffect` (reusa `CombatService`) — `Assets/Scripts/Skills/Effects/DamageAreaEffect.cs`
13. ✅ `StatusEffectManager` + `StunStatusEffect` + `SlowStatusEffect` — `Assets/Scripts/Skills/StatusEffects/`
14. ✅ `DashEffect` + `StunEffect` (dash que stunea al pasar) — `Assets/Scripts/Skills/Effects/`
15. ✅ `SelfBuffEffect` + `BuffStatusEffect` (buffs sobre stats del jugador) — `Assets/Scripts/Skills/Effects/` + `StatusEffects/`
16. ✅ `ManaRegen` en `PlayerResources` (manaRegenPerSecond en player.json) + `SkillEffectFactory`
17. ✅ Stun/slow en `Mob` (`AddStun/RemoveStun/AddSlow/RemoveSlow`) + `StatusEffectManager` creado en `GameBootstrap`

### Fase 4 — UI
17. `SkillSlotUI` + `SkillHotbarUI` (3 slots, cooldown overlay, mana)
18. `SkillUnlockNotificationUI`
19. `SkillBookUI` (lista de skills de la clase, bloqueadas/desbloqueadas)
20. Feedback de daño ya cubierto por `DamageNumberManager`

### Fase 5 — Contenido inicial
21. Skills warrior (whirlwind + daño/slow, dash_strike + stun)
22. Skills mage (meteor + daño/stun, skill de mouse AoE)
23. Balancear números en `skills.json`

---

## Catálogo de Contenido (para futuras iteraciones)

### Warrior (Fuerza)
- `cleave` — `damage_aoe` en abanico frontal (targeting: move_dir)
- `whirlwind` — `damage_aoe` self + `slow`
- `dash_strike` — `dash` + `stun`
- `battle_cry` — `self_buff` (+daño físico por duración)
- `iron_skin` — `self_buff` (+defensa / reducción de daño)
- `leap_slam` — `damage_aoe` en mouse + `slow`
- `shield_bash` — `damage_aoe` frontal + `stun`

### Mage (Inteligencia)
- `meteor` — `damage_aoe` en mouse + `stun`
- `frost_nova` — `damage_aoe` self + `slow`
- `arcane_barrage` — `damage_aoe` en mouse (rango largo)
- `haste` — `self_buff` (+velocidad de ataque y movimiento)
- `mana_shield` — `self_buff` (daño absorbido por mana)
- `fire_eruption` — `damage_aoe` en mouse + burn (DoT futuro)

### Futuro (fuera de scope del sistema base)
- Debuffs a enemigos (bajar defensa)
- Proyectiles (rango dirigido)
- Pasivas / skill trees visuales / procs
- Skills de mobs reusando `SkillCaster`

---

## Archivos Involucrados

### Crear
| Archivo | Rol |
|---------|-----|
| `Assets/Scripts/Skills/*` | Todo el sistema (ver estructura de clases) |
| `Assets/Resources/GameData/Config/skills.json` | Config de skills |

### Modificar
| Archivo | Cambio |
|---------|--------|
| `ConfigBoostrap.cs` | Cargar `skills.json` |
| `Player.cs` | Exponer `PlayerSkills` + restaurar del save |
| `PlayerInputs.cs` / `InputSystem_Actions.inputactions` | Acciones Q/E/R + spellbook |
| `PlayerSaveData.cs` / `GameSaveService.cs` | Persistir skills desbloqueadas |
| `PlayerAnimationController.cs` | Triggers de casteo por skill |
| `PlayerResources.cs` | Agregar regen de mana |
| `PlayerCombat.cs` | Reutilizar patrón de ventana para cast time (o extraer a helper compartido) |
