# Plan: Ataque de Mobs por Hitbox

## Estado Actual

El mob ataca con **daño instantáneo por timer**:

- `AttackState.cs` acumula un `_attackTimer` y cada **1 segundo** (`AttackCooldown`) ejecuta:
  1. `_ai.Animation?.PlayAttack()` (solo visual)
  2. `_combatService.Attack(owner, target, attack)` — **aplica el daño de inmediato**, sin hitbox ni timing

**Problemas del sistema actual:**
- El daño se aplica en el mismo frame que arranca la animación (sin windup ni ventana real)
- El golpe **siempre acierta** si el target está a `AttackRange` — no importa si el mob está girando, de espaldas, o si hay algo en medio
- No hay forma de controlar *cuándo* dentro de la animación ocurre el contacto
- `MobCombat.cs` existe pero está **vacío** (esqueleto sin uso)

---

## Objetivo

Modificar el ataque de los mobs para que funcione **por hitbox**, igual que el ataque básico del player:

```
ACTUAL:      Timer(1s) → Animación + Daño instantáneo
PROPUESTO:   Timer(1s) → Animación → [Evento INICIO] → Hitbox ON → Daño en colisión → [Evento FIN] → Hitbox OFF
```

**Decisión clave del usuario**: la ventana de ataque se controla con **eventos de animación**. Dos funciones alternan un estado:
- Evento de inicio → una función pone el estado en `true`
- Evento de fin → otra función lo pone en `false`

El usuario coloca el evento de inicio y el de fin **donde quiera** dentro del clip de ataque, lo que le da control total sobre el windup y la duración del contacto.

---

## Decisiones Tomadas

| Decisión | Respuesta |
|----------|-----------|
| Configuración de la hitbox | **Child GameObject en el prefab** (editor), como en `Player.prefab` |
| Control de la ventana de ataque | **Eventos de animación** (`OnAttackStart` / `OnAttackEnd`) |
| Objetivo de la hitbox | **Solo el Player** (sin friendly fire entre mobs) |
| Reutilización | Espejo de la arquitectura `PlayerCombat` → `PlayerAttackHitbox` |
| Dato del golpe | `CombatService.Attack()` — el mismo punto único de cálculo de daño |
| Cooldown | Se mantiene el timer actual de `AttackState` (1s) |

---

## Arquitectura Propuesta

Espejo directo del pipeline del player:

```
PLAYER:  PlayerCombat ── OnAttackStateChanged ──► PlayerAttackHitbox ──► CombatService
MOB:     MobCombat    ── OnAttackStateChanged ──► MobAttackHitbox    ──► CombatService
            ▲
            │ (eventos de animación)
        MobAnimationController
            ▲
        AttackState (dispara TryBeginAttack)
```

### Flujo completo de un golpe de mob

1. `AttackState.Tick()` — el cooldown venció y el mob no está atacando
2. `MobCombat.TryBeginAttack(attack)` — guarda `CurrentAttack` y dispara `MobAnimationController.PlayAttack()`
3. La animación `BasicAttack` corre
4. **Evento de animación `OnAttackStart`** → `MobCombat.SetAttackActive(true)`
5. `OnAttackStateChanged(true)` → `MobAttackHitbox` habilita su collider trigger
6. Si el Player entra en la hitbox → `OnTriggerEnter` → `CombatService.Attack(mob, player, attack)`
7. **Evento de animación `OnAttackEnd`** → `MobCombat.SetAttackActive(false)`
8. `OnAttackStateChanged(false)` → hitbox deshabilitada y `_alreadyHit` limpio

---

## Cambios Propuestos

### 1. Completar `MobCombat.cs` (archivo existe, está vacío)

**Archivo**: `Assets/Scripts/Mob/MobCombat.cs`

Componente en el **root del mob**. Maneja el estado de ataque y el evento que la hitbox escucha.

```csharp
using System;
using UnityEngine;

public class MobCombat : MonoBehaviour
{
    [SerializeField] private MobAnimationController animation;

    private Mob _owner;

    public bool IsAttacking { get; private set; }
    public Attack CurrentAttack { get; private set; }

    public event Action<bool> OnAttackStateChanged;

    private void Awake()
    {
        _owner = GetComponent<Mob>();

        if (animation == null)
            animation = GetComponent<MobAnimationController>();
    }

    // Llamado por AttackState cuando el cooldown terminó.
    // No interrumpe un ataque en curso.
    public bool TryBeginAttack(Attack attack)
    {
        if (attack == null || IsAttacking) return false;

        CurrentAttack = attack;
        animation?.PlayAttack();
        return true;
    }

    // Evento de animación: true = inicio del golpe, false = fin.
    // Controla la ventana en la que la hitbox está activa.
    public void SetAttackActive(bool active)
    {
        if (IsAttacking == active) return;

        IsAttacking = active;

        if (!active)
            CurrentAttack = null;

        OnAttackStateChanged?.Invoke(IsAttacking);
    }

    private void OnDisable()
    {
        if (IsAttacking)
            SetAttackActive(false);
    }
}
```

> Nota: `MobCombat` y `MobAnimationController` están en el mismo GameObject que el `Animator`, así los eventos de animación del clip de ataque pueden invocar métodos de cualquiera de los dos.

---

### 2. Agregar eventos de animación a `MobAnimationController.cs`

**Archivo**: `Assets/Scripts/Mob/Animator/MobAnimationController.cs`

Los eventos del clip llaman estos dos métodos (la capa de presentación delega en la lógica):

```csharp
[SerializeField] private MobCombat combat;

private void Awake()
{
    animator = GetComponent<Animator>();

    if (combat == null)
        combat = GetComponent<MobCombat>();
}

// Evento de animación: se coloca en el clip en el momento del contacto.
public void OnAttackStart()
{
    combat?.SetAttackActive(true);
}

// Evento de animación: se coloca al terminar el golpe.
public void OnAttackEnd()
{
    combat?.SetAttackActive(false);
}
```

---

### 3. Nuevo script `MobAttackHitbox.cs`

**Archivo**: `Assets/Scripts/Mob/MobAttackHitbox.cs` (nuevo, + `.meta`)

Componente en el **child "AttackHitbox"** del mob. Es el espejo de `PlayerAttackHitbox`:

```csharp
using System.Collections.Generic;
using UnityEngine;

public class MobAttackHitbox : MonoBehaviour
{
    [SerializeField] private MobCombat mobCombat;
    [SerializeField] private Collider hitboxCollider;

    private readonly HashSet<Collider> _alreadyHit = new HashSet<Collider>();
    private CombatService _combatService;
    private Mob _mob;

    private void Awake()
    {
        _combatService = new CombatService();

        if (hitboxCollider == null)
            hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
        }

        if (mobCombat == null)
            mobCombat = GetComponentInParent<MobCombat>();

        if (mobCombat != null)
            _mob = mobCombat.GetComponent<Mob>();
    }

    private void Start()
    {
        if (mobCombat != null)
            mobCombat.OnAttackStateChanged += SetActiveHitbox;
    }

    private void OnDestroy()
    {
        if (mobCombat != null)
            mobCombat.OnAttackStateChanged -= SetActiveHitbox;
    }

    public void SetActiveHitbox(bool active)
    {
        if (!active)
            _alreadyHit.Clear();

        if (hitboxCollider != null)
            hitboxCollider.enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mobCombat == null || !mobCombat.IsAttacking)
            return;

        if (_alreadyHit.Contains(other))
            return;

        if (!other.TryGetComponent<Player>(out var player) || !player.IsAlive)
            return;

        Attack attack = mobCombat.CurrentAttack;
        if (attack == null || _mob == null)
            return;

        _alreadyHit.Add(other);

        _combatService.Attack(_mob, player, attack);
    }
}
```

**Reglas de la hitbox:**
- Solo daña al **Player** vivo (decidido) — ningún mob puede golpear a otro mob
- Cada swing golpea una sola vez al mismo collider (`HashSet<Collider>`)
- El collider arranca deshabilitado y solo se activa durante la ventana de ataque

---

### 4. Integrar `MobCombat` en `Mob.cs`

**Archivo**: `Assets/Scripts/Mob/Mob.cs`

- Agregar `[RequireComponent(typeof(MobCombat))]` junto a los existentes
- Exponer `public MobCombat Combat { get; private set; }`
- En `Initialize()`: `Combat = GetComponent<MobCombat>();`

```csharp
[RequireComponent(typeof(MobResources))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MobAnimationController))]
[RequireComponent(typeof(MobCombat))]   // nuevo
public class Mob : MonoBehaviour, ICombatEntity
{
    public MobCombat Combat { get; private set; }

    // dentro de Initialize():
    Combat = GetComponent<MobCombat>();
}
```

---

### 5. Simplificar `AttackState.cs`

**Archivo**: `Assets/Scripts/Mob/MachineState/AttackState.cs`

Elimina el daño directo por timer. Ahora solo decide *cuándo* lanzar el swing:

```csharp
public class AttackState : IMobState
{
    private readonly MobAI _ai;
    private float _attackTimer;

    private const float AttackCooldown = 1f;

    public AttackState(MobAI ai) => _ai = ai;

    public void Enter()
    {
        _attackTimer = 0;
        _ai.TargetSpeed = 0f;
        _ai.Movement.Stop();
        FaceTarget();
    }

    public void Tick()
    {
        if (_ai.Target == null)
        {
            _ai.ChangeState(new ReturnToSpawnState(_ai));
            return;
        }

        float distanceToTarget =
            Vector3.Distance(_ai.Position, _ai.Target.position);

        float distanceToSpawn =
            Vector3.Distance(_ai.Position, _ai.SpawnPosition);

        if (distanceToTarget > _ai.AttackRange)
        {
            _ai.ChangeState(new ChaseState(_ai));
            return;
        }

        if (distanceToSpawn > _ai.LoseTargetRange)
        {
            _ai.Target = null;
            _ai.ChangeState(new ReturnToSpawnState(_ai));
            return;
        }

        FaceTarget();

        // No apilar golpes: la ventana la cierra el evento OnAttackEnd.
        if (_ai.Owner.Combat.IsAttacking)
            return;

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= AttackCooldown)
        {
            _attackTimer = 0;

            _ai.Owner.Combat.TryBeginAttack(new Attack
            {
                damageMultiplier = 1f,
                damageType = AttackDamageType.Physical
            });
        }
    }

    public void Exit()
    {
        // Si el estado se corta a mitad de swing, forzar cierre de hitbox.
        _ai.Owner.Combat.SetAttackActive(false);
    }

    private void FaceTarget() { /* igual que hoy */ }
}
```

**Cambios clave:**
- Se quita `CombatService` del estado (el daño ahora ocurre en `MobAttackHitbox`)
- `TryBeginAttack` guarda el `CurrentAttack` y dispara la animación
- `Exit()` fuerza `SetAttackActive(false)` para no dejar la hitbox colgada si el mob se va a Chase/Return a mitad del swing

---

### 6. Prefabs: agregar hitbox + `MobCombat` (editor)

**Archivos**: `Assets/Assets/Resources/Prefabs/Mobs/Zombie.prefab`, `Assets/Assets/Resources/Prefabs/Mobs/Skeleton.prefab`

En el editor de Unity, en **ambos** prefabs:

**a) Agregar componente `MobCombat` al root**
- Seleccionar el root (Zombie / Skeleton)
- Add Component → `MobCombat`
- `animation` → arrastrar el componente `MobAnimationController`

**b) Crear el child `AttackHitbox`**
- Click derecho en el root → Create Empty → renombrar a `AttackHitbox`
- Add Component → `BoxCollider`
  - `Is Trigger` = ✅
  - Ajustar `Size` y `Center` para cubrir el frente del mob (ver tabla abajo)
  - Deshabilitar el collider (checkbox en la parte superior del componente) — el script lo enciende solo durante la ventana
- Add Component → `MobAttackHitbox`
  - `mobCombat` → arrastrar el componente `MobCombat` del root
  - `hitboxCollider` → el propio `BoxCollider` del child

**Posición sugerida del collider** (el mob mira al target girando el root, y `+Z local` apunta al target):

| Mob | Local Position | Size | Reach total |
|-----|----------------|------|-------------|
| Zombie (`attackRange`: 2) | `(0, 0.5, 1)` | `(1.2, 1, 2)` | de 0 a 2 m al frente |
| Skeleton (`attackRange`: 2.5) | `(0, 0.5, 1.25)` | `(1.2, 1, 2.5)` | de 0 a 2.5 m al frente |

> El tamaño del collider define el **alcance real del golpe**; `attackRange` en `enemies.json` solo controla cuándo el mob entra a `AttackState`. Conviene que sean consistentes.

---

### 7. Eventos de animación (editor, manual)

**Clip**: el ataque usado por el estado `BasicAttack` del `MobController.controller` (pertenece al modelo `guid 36a91d8b3624ad944994068a86300fe1`).

En la ventana **Animation** de Unity, con el mob seleccionado:

1. Abrir el clip de ataque en la pestaña Animation
2. En el **momento del contacto** (ej. 30% del clip) → Add Event → `OnAttackStart`
3. Al **final del golpe** (ej. 80-90%) → Add Event → `OnAttackEnd`
4. Ajustar libremente las posiciones hasta que el timing se sienta bien

**Comportamiento:**
- La hitbox se activa solo entre ambos eventos (el windup previo no golpea)
- El estado no puede encadenar otro golpe hasta que `OnAttackEnd` apague la hitbox

> El clip de ataque del mob **no es el mismo** que el del player (el player usa clips del modelo `25c1f9b...`), así que agregar estos eventos **no afecta al ataque del player**.

---

## Archivos a Crear

| Archivo | Descripción |
|---------|-------------|
| `Assets/Scripts/Mob/MobAttackHitbox.cs` (+ `.meta`) | Hitbox del mob, espejo de `PlayerAttackHitbox` |
| `MobAttackPlan.md` | Este documento |

## Archivos a Modificar

| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/Mob/MobCombat.cs` | Completar: estado `IsAttacking`, `CurrentAttack`, evento `OnAttackStateChanged`, `TryBeginAttack`, `SetAttackActive` |
| `Assets/Scripts/Mob/Animator/MobAnimationController.cs` | Agregar `OnAttackStart()` / `OnAttackEnd()` que delegan a `MobCombat` |
| `Assets/Scripts/Mob/Mob.cs` | `[RequireComponent(MobCombat)]` + propiedad `Combat` |
| `Assets/Scripts/Mob/MachineState/AttackState.cs` | Quitar daño directo; usar `TryBeginAttack`; gate por `IsAttacking`; `Exit()` limpia hitbox |
| `Assets/Assets/Resources/Prefabs/Mobs/Zombie.prefab` | Componente `MobCombat` en root + child `AttackHitbox` |
| `Assets/Assets/Resources/Prefabs/Mobs/Skeleton.prefab` | Ídem |

## Archivos sin Cambio (referencia)

| Archivo | Razón |
|---------|-------|
| `CombatService.cs` | Punto único de daño ya existente — se reutiliza tal cual |
| `PlayerCombat.cs` / `PlayerAttackHitbox.cs` | El player no se toca; es el modelo a imitar |
| `MobAI.cs` / `ChaseState.cs` / `ReturnToSpawnState.cs` | Sin cambios |
| `enemies.json` | `attackRange` sigue siendo el umbral para entrar a `AttackState` |

---

## Orden de Implementación Sugerido

1. **Crear `MobAttackHitbox.cs`** + `.meta` (y registrar GUID para prefabs)
2. **Completar `MobCombat.cs`** — estado + evento + `TryBeginAttack`/`SetAttackActive`
3. **Modificar `MobAnimationController.cs`** — handlers de eventos de animación
4. **Modificar `Mob.cs`** — `Combat` property + RequireComponent
5. **Modificar `AttackState.cs`** — quitar daño directo, usar hitbox
6. **Configurar prefabs** (Zombie + Skeleton) — `MobCombat` + child `AttackHitbox`
7. **Agregar eventos de animación** en el clip de ataque (`OnAttackStart` / `OnAttackEnd`)
8. **Probar en el editor** — golpe conecta solo dentro de la ventana, sin friendly fire, hitbox se apaga al terminar

---

## Verificación Manual (criterios de aceptación)

- [ ] El mob hace la animación de ataque pero el player **no** recibe daño durante el windup (antes del evento de inicio)
- [ ] El player recibe daño solo cuando está dentro de la hitbox delantera del mob
- [ ] El mob **no** golpea a otros mobs (solo al player)
- [ ] Un swing golpea al player una sola vez (sin hits múltiples por frame)
- [ ] Si el mob sale del estado de ataque a mitad del swing, la hitbox se apaga (sin dejar colliders colgados)
- [ ] El daño numérico y el crítico funcionan igual que antes (mismo `CombatService`)
