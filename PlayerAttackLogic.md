# Lógica de Ataque del Player

## Resumen General

El sistema de combate del player sigue un pipeline de 6 etapas:
**Input → Config → Stats → Hitbox → CombatService → Daño + Feedback**

---

## 1. Input (`PlayerInputs.cs`)

El jugador presiona un botón de ataque (configurado en el Input System de Unity).

```
BasicAttack.performed → PlayerCombat.OnBasicAttack(context)
```

Solo se ejecuta en `InputActionPhase.Performed` (cuando se presiona, no mientras se mantiene).

---

## 2. Config del Ataque (`PlayerCombat.cs`)

`OnBasicAttack()` busca el ataque por ID en `AttackConfig` (cargado desde `attacks.json`):

```csharp
Attack candidateAttack = FindAttackById("basic_attack");
```

**Ataques disponibles** (attacks.json):

| ID | damageMultiplier | damageType | duration | range |
|----|-----------------|------------|----------|-------|
| `basic_attack` | 1.0 | Physical (0) | 0.8s | 2.0 |
| `heavy_attack` | 2.5 | Physical (0) | 1.4s | 2.5 |
| `fireball` | 3.0 | Magical (1) | 1.1s | - |

Cada ataque tiene un `damageMultiplier` que se aplica al attackPower base.

---

## 3. Stats del Player (`Player.cs` → `CombatStats`)

Los stats de combate se computean dinámicamente cada vez que se accede a `Player.CombatStats`:

```csharp
public CombatStats CombatStats => new CombatStats
{
    PhysicalAttack  = Stats.Strength  * StatsConfig.strength.damagePerPoint,
    MagicAttack     = Stats.Intelligence * StatsConfig.intelligence.spellDamagePerPoint,
    PhysicalDefense = Stats.Vitality * StatsConfig.vitality.healthPerPoint,
    MagicDefense    = Stats.Intelligence * StatsConfig.intelligence.spellDamagePerPoint,
    CriticalChance  = PlayerConfig.combat.criticalChance
                      + (Stats.Dexterity * StatsConfig.Dexterity.criticalChancePerPoint),
    CriticalDamage  = PlayerConfig.combat.criticalDamage
};
```

**Valores por defecto** (player.json + stats.json):

| Stat | Fórmula | Ejemplo (10 dex) |
|------|---------|-------------------|
| PhysicalAttack | Strength × 2 | 10 × 2 = 20 |
| CriticalChance | 0.05 + (Dexterity × 0.002) | 0.05 + 0.02 = 0.07 (7%) |
| CriticalDamage | 1.5 (fijo del config) | 1.5x |

---

## 4. Attack Speed (`PlayerCombat.cs`)

La velocidad de ataque afecta la duración de la animación:

```csharp
float effectiveAttackSpeed = baseAttackSpeed
    + (playerStats.Dexterity * statsConfig.Dexterity.attackSpeedPerPoint);

float actualDuration = candidateAttack.duration / effectiveAttackSpeed;
```

**Ejemplo**: `basic_attack` (0.8s) con 10 dex (0.01/pt):
- effectiveAttackSpeed = 1.0 + (10 × 0.01) = 1.1
- actualDuration = 0.8 / 1.1 = 0.727s

Esto abre la "attack window" — un período donde `IsAttacking = true` y la hitbox está activa.

---

## 5. Hitbox (`PlayerAttackHitbox.cs`)

Cuando `OnAttackStateChanged(true)` se dispara:
- Se habilita el `Collider` (trigger)
- Se limpia el `HashSet<Collider> _alreadyHit`

En `OnTriggerEnter()`:
1. Verifica que esté atacando (`playerCombat.IsAttacking`)
2. Verifica que no haya golpeado ya este collider en esta swing
3. Verifica que no sea otro Player
4. Obtiene `ICombatEntity` del collider
5. Llama a `CombatService.Attack(player, damageable, attack)`

**Importante**: cada swing solo golpea una vez cada enemigo (HashSet previene hits múltiples).

---

## 6. CombatService (`CombatService.cs`)

### Fórmula de daño

```
attackPower = (damageType == Physical) ? attacker.PhysicalAttack : attacker.MagicAttack
defense     = (damageType == Physical) ? target.PhysicalDefense  : target.MagicDefense
baseDamage  = max(1, round(attackPower × damageMultiplier) - defense)
```

### Cálculo de crítico

```
critChance = attacker.CriticalChance
isCritical = random(0,1) < critChance
finalDamage = isCritical ? round(baseDamage × attacker.CriticalDamage) : baseDamage
```

### Resultado

```csharp
target.TakeDamage(new DamageData
{
    BaseDamage = attackPower,
    FinalDamage = finalDamage,
    DamageType = attack.damageType,
    IsCritical = isCritical,
    Source = attacker
});
```

**Ejemplo completo** (basic_attack, 10 str, 10 dex, vs zombie con 4 def):
- attackPower = 10 × 2 = 20
- rawDamage = round(20 × 1.0) - 4 = 16
- critChance = 0.05 + (10 × 0.002) = 0.07
- Si crítico: 16 × 1.5 = 24

---

## 7. Recepción del Daño

### En Mobs (`Mob.cs` → `MobResources.cs`)

```
Mob.TakeDamage(DamageData)
  → _resources.TakeDamage(damage, damageData.IsCritical)
    → Actualiza HP
    → DamageNumberManager.Show(position, damage, isCritical)
    → OnHealthChanged → EnemyHealthBarUI se actualiza
    → Si HP <= 0 → Die()
    → Si no → OnHit
```

### En Player (`Player.cs`)

```
Player.TakeDamage(DamageData)
  → Resources.TakeDamage(damage)
  → (sin damage numbers aún — solo health bar del HUD)
```

---

## Archivos Involucrados

| Archivo | Rol |
|---------|-----|
| `PlayerInputs.cs` | Captura input del jugador |
| `PlayerCombat.cs` | Busca config del ataque, calcula speed, maneja attack window |
| `PlayerAttackHitbox.cs` | Detecta colisiones durante el swing |
| `CombatService.cs` | Calcula daño final (con crítico) y aplica a target |
| `Player.cs` | Provee `CombatStats` computados desde stats + config |
| `PlayerStats.cs` | Stats base (Strength, Dex, Int, Vit) |
| `DamageData.cs` | Payload de daño (BaseDamage, FinalDamage, IsCritical, DamageType, Source) |
| `CombatStats.cs` | Estructura con todos los stats de combate |
| `Attacks.cs` | Config de cada ataque (multiplier, type, duration, range) |
| `Mob.cs` | Recibe daño, pasa isCritical a MobResources |
| `MobResources.cs` | Aplica daño, muestra damage number |
| `DamageNumberManager.cs` | Pool + muestra números flotantes |

---

## Config Values (archivos JSON)

### player.json
```json
"combat": {
    "attackSpeed": 1.0,
    "criticalChance": 0.05,
    "criticalDamage": 1.5
}
```

### stats.json
```json
"strength":  { "damagePerPoint": 2 },
"dexterity": { "attackSpeedPerPoint": 0.01, "criticalChancePerPoint": 0.002 }
```

### attacks.json
```json
{ "id": "basic_attack",  "damageMultiplier": 1.0, "damageType": 0, "duration": 0.8 }
{ "id": "heavy_attack",  "damageMultiplier": 2.5, "damageType": 0, "duration": 1.4 }
{ "id": "fireball",      "damageMultiplier": 3.0, "damageType": 1, "duration": 1.1 }
```

---

## Cómo Extender

### Agregar un nuevo ataque
1. Agregar en `attacks.json` con un id único
2. Si es un tipo nuevo de daño, agregar al enum `AttackDamageType`
3. Si tiene animación nueva, agregar en `PlayerAnimationController`

### Modificar fórmula de daño
Editar `CombatService.Attack()` — es el único punto donde se calcula el daño.

### Agregar stats que afecten daño
1. Agregar campo en `PlayerCombatConfig` (o `StatsConfig`)
2. Agregar en `player.json` / `stats.json`
3. Computar en `Player.CombatStats` getter
4. Usar en `CombatService.Attack()`

### Agregar tipo de daño nuevo (ej: fuego, hielo)
1. Agregar al enum `AttackDamageType`
2. Agregar stat de defensa correspondiente en `CombatStats`
3. Modificar `CombatService.Attack()` para usar la defensa correcta
4. Agregar color diferenciado en `DamageNumberManager` (futuro)
