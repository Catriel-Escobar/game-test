# Plan: Capa Visual de Skills (VFX declarativos por skill)

> Rama: `feature/skills-from-weapons`
>
> Separar **Gameplay** de **Visual** dentro de una skill, para que una skill pueda tener N VFX (cast, proyectil, impacto, persistente, estela) sin multiplicar efectos ni scripts. La idea parte del árbol propuesto por el usuario; esta es la versión escalable.

---

## Estado Actual

Hoy el VFX de skills está **embebido y disperso**:

| Pieza | Estado | Problema |
|-------|--------|----------|
| `SkillEffectDefinition.impactVfx` | Campo suelto en el efecto | Solo lo lee `DamageAreaEffect` — no escala si una skill tiene 5 VFX |
| `DamageAreaEffect` | Spawnea impacto por enemigo golpeado | VFX atado al código del efecto, no declarativo |
| `DamageReductionStatusEffect` | Hardcodea `VFX/ShieldVFX` (path + follow) | VFX de skill dentro de un status effect |
| `ImpactVfxSpawner` | Spawner genérico one-shot | No conoce cast/proyectil/persistente |
| — | No existe VFX de cast, de proyectil ni de estela de dash | Dash no deja estela, meteor no "cae" |

**VFX por skill hoy = 0 declarados.** Cada VFX nuevo requiere tocar C#. Ese es el cuello de botella que resolvemos.

---

## Objetivo

```text
Skill
 ├── Gameplay  →  effects[] + campos base (manaCost, cooldown, castTime, targeting)
 │                  ✓ ya existe — NO se toca la lógica de casting
 │
 └── Visual    →  visuals[] (NUEVO)
                      ├── Cast VFX        (trigger: cast start)
                      ├── Projectile VFX  (origen → centro, con vuelo)
                      ├── Impact VFX      (centro / por enemigo golpeado)
                      └── Persistent VFX  (sigue al jugador mientras dura)
```

Regla de oro: **agregar un VFX a una skill = agregar una entrada JSON**. C# solo se toca si se inventa un *tipo* de VFX nuevo.

---

## Decisiones de Diseño

| # | Decisión | Detalle |
|---|----------|---------|
| 1 | **Nivel del VFX** | El VFX vive a nivel de **skill** (`visuals[]`), NO dentro de cada efecto. Un VFX es presentación; puede haber skill con 1 efecto y 5 VFX (dash) o con 3 efectos y 1 VFX. |
| 2 | **No bloqueante** | El VFX es **cosmético y paralelo**. El gameplay resuelve exactamente igual que hoy. No se agrega delay al daño en v1 (ver "Futuro": `impactDelay`). |
| 3 | **Tipos v1** | `cast`, `projectile`, `impact`, `hit`, `persistent`. Cubren el árbol del usuario + el caso "por enemigo golpeado". |
| 4 | **Anclas** | `origin` (jugador), `center` (centro resuelto), `hitpoint` (por golpe), `player` (follow transform). Con `offset` opcional. |
| 5 | **Trigger** | `cast` (inicio de casteo), `resolve` (cast completado, se resuelven efectos), `end` (termina la cadena de efectos — usado para el impacto de llegada del dash). |
| 6 | **Data** | JSON en `skills.json` (`visuals[]`), prefabs en `Resources/VFX/`. Consistente con el proyecto (JsonUtility). |
| 7 | **Migración** | Se **elimina** `impactVfx`/`impactVfxDuration` de los efectos → pasan a `visuals` (`hit`). Se **mantiene** `ImpactVfxSpawner` porque `PlayerCombat` lo usa para el ataque básico (no es de skills). |
| 8 | **VFX del escudo** | El `VFX/ShieldVFX` hardcodeado en `DamageReductionStatusEffect` **se mantiene** (ver desviación al final del doc). Razón: está atado al lifecycle del status effect (expira/remplea con el efecto, dispara `ShieldRipples` al recibir daño) — migrarlo a `persistent` generaría doble spawn del escudo y perdería las ondas. |
| 9 | **Estela de dash** | Es un `persistent` visual (particle prefab que emite estela) **followPlayer** con `destroyAfter` ≈ `distance/speed` (diseño define el número). |
| 10 | **Duración** | `destroyAfter` numérico (segundos), o token `"castTime"` para visuales de casteo. |
| 11 | **Gameplay tree del usuario** | Los sub-campos del usuario (Damage/Range/Cooldown/Duration) **ya existen**: en `effects[]` (damage, radius, range, duration) y en la skill (cooldown, castTime, manaCost). No los re-estructuro a un objeto `gameplay{}` porque sería churn sin beneficio funcional. Se puede hacer como refactor cosmético en el futuro. |

---

## Modelo de Datos

### `SkillDefinition` — nuevo campo

```json
{
  "id": "dash_strike",
  "manaCost": 12,
  "cooldown": 10.0,
  "castTime": 0.2,
  "animationId": "Dash",
  "targeting": "move_dir",
  "effects": [ ... ],       // Gameplay — sin cambios
  "visuals": [              // Visual — NUEVO
    {
      "type": "persistent",
      "trigger": "resolve",
      "prefab": "VFX/DashTrail",
      "anchor": "player",
      "offset": { "y": 0.2 },
      "destroyAfter": 0.3
    },
    {
      "type": "impact",
      "trigger": "end",
      "prefab": "VFX/Dash_Impact",
      "anchor": "center",
      "destroyAfter": 0.8
    }
  ]
}
```

### Campo `visual` (entrada individual)

| Campo | Tipo | Default | Qué hace |
|-------|------|---------|----------|
| `type` | string | — | `cast` \| `projectile` \| `impact` \| `hit` \| `persistent` |
| `trigger` | string | según type | `cast` \| `resolve` \| `end` |
| `prefab` | string | — | Path en `Resources/` (ej: `VFX/VFX_Impact01`) |
| `anchor` | string | según type | `origin` \| `center` \| `hitpoint` \| `player` |
| `offset` | {x,y,z} | 0 | Desplazamiento local del punto de spawn |
| `destroyAfter` | float | según type | Segundos hasta destruirse; `"castTime"` = dura todo el casteo |
| `delay` | float | 0 | Espera antes de spawnear tras el trigger |
| `followPlayer` | bool | false | Se adjunta y sigue al jugador (persistent) |
| `travelTime` | float | 0 | Solo `projectile`: duración del vuelo origen→center |

### Defaults por tipo

| type | trigger | anchor | destroyAfter |
|------|---------|--------|--------------|
| `cast` | `cast` | `origin` | `"castTime"` |
| `projectile` | `resolve` | `origin` | `travelTime` + `delay` |
| `impact` | `resolve` | `center` | `1.0` |
| `hit` | `resolve` | `hitpoint` | `0.6` |
| `persistent` | `resolve` | `player` | `1.0` |

> Con estos defaults, un "cast de frost nova con impacto en el centro" es solo:
> ```json
> "visuals": [
>   { "type": "cast", "prefab": "VFX/FrostNova_Cast" },
>   { "type": "impact", "prefab": "VFX/VFX_Impact01" }
> ]
> ```

---

## Pipeline de VFX

```
Q/E/R → SkillCaster
   │
   ├─ StartCast(skill)
   │     ├─ animación de casteo (existente)
   │     └─ SkillVisualDirector.PlayTrigger("cast")     ← cast VFX (origin, dura castTime)
   │
   ├─ CompleteCast(skill)
   │     ├─ cooldown + OnCastCompleted (existente)
   │     └─ SkillVisualDirector.PlayTrigger("resolve")   ← projectile + impact + persistent
   │
   └─ Fin de ExecuteEffectChain (NUEVO evento OnEffectsEnded)
         └─ SkillVisualDirector.PlayTrigger("end")      ← impacto de llegada (dash), center final
```

- El `context` (`Origin`, `Center`, `Direction`) lo actualiza `DashEffect` (`context.Center = posición final`), así el visual `end` del dash spawnea **en el punto de llegada** sin tocar el efecto.
- Los visuales se corren en coroutines propias del director; no bloquean al efecto.

---

## Estructura de Clases

```text
Assets/Scripts/Skills/
├── Visuals/
│   ├── SkillVisualDefinition.cs    # Modelo JSON (o dentro de SkillData.cs)
│   ├── ISkillVisual.cs             # IEnumerator Play(SkillVisualContext ctx)
│   ├── SkillVisualContext.cs       # player, skill, prefab, origin, center, direction
│   ├── SkillVisualFactory.cs       # string type → clase (mismo patrón que SkillEffectFactory)
│   ├── CastVisual.cs               # spawn en origin, destroy al terminar casteo
│   ├── ProjectileVisual.cs         # vuelo origin→center en travelTime (lerp/move)
│   ├── ImpactVisual.cs             # one-shot en center (con delay opcional)
│   ├── HitVisual.cs                # one-shot por hitpoint (lo usa DamageAreaEffect vía contexto)
│   ├── PersistentVisual.cs         # spawn + followPlayer + destroyAfter
│   ├── SkillVisualDirector.cs      # orquestador: PlayTrigger("cast|resolve|end")
│   └── SkillVfxSpawner.cs          # spawn con cache + destroy (generaliza ImpactVfxSpawner)
├── Assets/Scripts/Core/TransformFollower.cs   # genérico (generaliza ShieldFollow)
```

`SkillVisualDirector` se agrega al mismo GameObject que `SkillCaster` (o lo crea `Player.cs` si no está), y `SkillCaster` le inyecta ref.

---

## Integración con lo existente

| Script existente | Cambio |
|------------------|--------|
| `SkillData.cs` | Quitar `impactVfx`/`impactVfxDuration` de `SkillEffectDefinition`; agregar `SkillVisualDefinition[] visuals` a `SkillDefinition` |
| `SkillCaster.cs` | Inyectar `SkillVisualDirector`; `PlayTrigger("cast")` en `StartCast`, `PlayTrigger("resolve")` en `CompleteCast`, nuevo evento `OnEffectsEnded` al terminar `ExecuteEffectChain` → `PlayTrigger("end")` |
| `DamageAreaEffect.cs` | Quitar el `ImpactVfxSpawner.Spawn(...)` (el `hit` visual lo reemplaza) — el efecto queda puro gameplay |
| `DamageReductionStatusEffect.cs` | Quitar `SpawnShieldVfx` + `ShieldFollow` hardcodeados → `bastion` declara un `persistent` visual |
| `Player.cs` | Asegurar `SkillVisualDirector` en el GameObject del caster (si no, `AddComponent`) |
| `ImpactVfxSpawner.cs` | **Se mantiene** (lo usa `PlayerCombat` para el ataque básico) — no es de skills |
| `ShieldFollow.cs` | Se puede generalizar a `TransformFollower` y reutilizarlo en `PersistentVisual` |
| `skills.json` | Migrar los 5 skills: `visuals[]` + quitar `impactVfx` de los efectos |

---

## Orden de Implementación

### Fase 1 — Modelo
1. `SkillVisualDefinition` + `visuals[]` en `SkillDefinition`; quitar `impactVfx`/`impactVfxDuration` del efecto.

### Fase 2 — Core
2. `SkillVfxSpawner` (spawn + cache + destroy, derivado de `ImpactVfxSpawner`).
3. `TransformFollower` genérico (generaliza `ShieldFollow`).

### Fase 3 — Tipos de visual
4. `ISkillVisual` + `SkillVisualContext` + `SkillVisualFactory`.
5. `CastVisual`, `ImpactVisual`, `HitVisual`, `PersistentVisual`, `ProjectileVisual`.

### Fase 4 — Orquestador
6. `SkillVisualDirector` con `PlayTrigger("cast"|"resolve"|"end")`.
7. Hooks en `SkillCaster` (`cast`/`resolve`/`end`) + evento `OnEffectsEnded`.
8. Wiring en `Player.cs`.

### Fase 5 — Migración de contenido
9. `skills.json`: agregar `visuals[]` a los 5 skills; quitar `impactVfx` de efectos.
10. Migrar el shield de `bastion` a `persistent` visual; limpiar `DamageReductionStatusEffect`.

### Fase 6 — Contenido VFX
11. Prefabs nuevos en `Resources/VFX/`: estela de dash, impacto de dash, cast circles, proyectil de meteor, etc.
12. Balancear `destroyAfter`/`delay`/`travelTime` por skill.

### Fase 7 — Verificación
13. Compilar (csc manual). Probar en Unity:
    - `dash_strike` → estela detrás durante el dash + impacto en el punto de llegada.
    - `meteor` → cast VFX + proyectil cayendo al centro + impacto.
    - `seismic_strike`/`frost_nova` → cast + impacto en el centro.
    - `bastion` → cast + escudo persistente siguiendo al jugador 3s (mismo visual que hoy).
    - `arcane_barrage` → proyectil + impacto.
    - El daño/stun/slow/dash **se resuelven igual que antes** (el VFX no bloquea).

---

## Ejemplo: contenido por skill (v1)

| skill | visuals |
|-------|---------|
| `dash_strike` | `persistent` estela (followPlayer, destroyAfter 0.3) + `impact` en `end` (punto de llegada) |
| `meteor` | `cast` (círculo de casteo) + `projectile` (meteor cayendo, travelTime ~1.0) + `impact` (explosión en center) |
| `seismic_strike` | `cast` + `impact` (sismo en center) |
| `frost_nova` | `cast` + `impact` (nova en center) |
| `bastion` | `cast` + `persistent` shield (followPlayer, destroyAfter 3.0) |
| `arcane_barrage` | `cast` + `projectile` + `impact` |

---

## Futuro (fuera de scope v1)

- **`impactDelay`** en la skill: si se quiere que el daño del meteor se aplique **cuando cae** el proyectil (hoy se resuelve al completar el cast). Requiere que el efecto espere al visual — decidir si vale la pena.
- **`telegraph`** (ground indicator) para skills `mouse` durante el cast time.
- **Sonido por visual** (`sfx` field) y **trigger por animation event** en vez de timing fijo.
- **VFX por status effect** (ej: tint de hielo en mobs ralentizados) reusando el director.
- Refactor cosmético a objeto `gameplay{}` para calcar el árbol del usuario.

---

## Archivos Involucrados

### Crear
| Archivo | Rol |
|---------|-----|
| `Assets/Scripts/Skills/Visuals/*` | Sistema de VFX (ver estructura de clases) |
| `Assets/Scripts/Core/TransformFollower.cs` | Follow genérico (generaliza `ShieldFollow`) |
| `Resources/VFX/*` | Prefabs nuevos (estela, cast, proyectil, impactos por skill) |
| `SkillVisualsPlan.md` | este documento |

### Modificar
| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/Skills/SkillData.cs` | `visuals[]` en skill; quitar `impactVfx` del efecto |
| `Assets/Scripts/Skills/SkillCaster.cs` | hooks cast/resolve/end + `OnEffectsEnded` |
| `Assets/Scripts/Skills/Effects/DamageAreaEffect.cs` | quitar spawn de impacto (pasa a `hit` visual) |
| `Assets/Scripts/Skills/StatusEffects/DamageReductionStatusEffect.cs` | quitar VFX hardcodeado del shield |
| `Assets/Scripts/Player/Player.cs` | asegurar `SkillVisualDirector` |
| `Assets/Assets/Resources/GameData/Config/skills.json` | `visuals[]` + migración |
| `Assets/Assets/VFX/shield/ShieldFollow.cs` | opcional: generalizar a `TransformFollower` |

### Se mantienen
| Archivo | Razón |
|---------|-------|
| `ImpactVfxSpawner.cs` | Lo usa `PlayerCombat` para el ataque básico (no es de skills) |
| `SkillEffectFactory` / `SkillTargetingFactory` | Patrón que replica `SkillVisualFactory` |
| `effects[]` + `SkillCaster` | Gameplay intacto — solo se agrega la capa visual |

---

## Estado de Implementación ✅

Todo lo de arriba está implementado y compila (verificado por csc contra los módulos de Unity, 0 errores):

| Fase | Estado |
|------|--------|
| 1. Modelo (`SkillVisualDefinition` + `visuals[]`; se quitó `impactVfx`/`impactVfxDuration` del efecto) | ✅ |
| 2. Core (`SkillVfxSpawner` + `TransformFollower`) | ✅ |
| 3. Tipos (`CastVisual`, `ProjectileVisual`, `ImpactVisual` [cubre impact+hit vía ancla], `PersistentVisual` + factory) | ✅ |
| 4. `SkillVisualDirector` (triggers cast/resolve/end + `PlayHit`) | ✅ |
| 5. `SkillCaster` hooks + evento `OnEffectsEnded`; director auto-creado en `Initialize` | ✅ |
| 6. `DamageAreaEffect` → impacto por golpe vía `PlayHit` | ✅ |
| 7. `skills.json` → `visuals[]` en las 6 skills; `impactVfx` removido de efectos | ✅ |
| 8. Prefabs placeholder (`VFX_Cast01`, `VFX_DashTrail`, `VFX_MeteorProjectile`) | ✅ |

### Desviaciones del plan
- **Escudo de bastion**: se mantiene en `DamageReductionStatusEffect` (hardcodeado `VFX/ShieldVFX`). Ver Decisión 8 — migrarlo rompería el feedback de `ShieldRipples` o duplicaría el escudo.
- **`HitVisual`**: no se creó como clase separada — `ImpactVisual` cubre `impact` y `hit` (la diferencia es el ancla `center` vs `hitpoint` y el `destroyAfter` default por tipo). Misma cantidad de comportamiento, una clase menos.
- Los prefabs de cast/estela/proyectil son **copias placeholder** de `VFX_Impact01` (mismo visual, GUID propio). Hay que reemplazarlos por VFX reales — el sistema ya los resuelve por path.
