# Log de Progreso — Sistema de Skills + Fase 4 UI

> Log de respaldo para retomar el trabajo si se apaga la PC. Actualizado: 2026-08-02.

---

## 1. Lo implementado hasta ahora

### Fase 0 — Data ✅
- `Assets/Scripts/Skills/SkillData.cs` — modelos `SkillDefinition` + `SkillEffectDefinition`
- `Assets/Scripts/Skills/SkillsConfig.cs` — contenedor de skills.json
- `Assets/Assets/Resources/GameData/Config/skills.json` — 4 skills (whirlwind, dash_strike = warrior; frost_nova, meteor = mage)
- Registrado en `game.json` + cargado en `ConfigBoostrap.cs`

### Fase 1 — Desbloqueo ✅
- `Assets/Scripts/Skills/PlayerSkills.cs` — skills por clase, unlock por nivel, `OnSkillUnlocked`
- `Assets/Scripts/Skills/SkillUnlockService.cs` — escucha `Progression.OnLevelChanged`
- `unlockedSkillIds` persistido en save (`PlayerSaveData` + `GameSaveService`); `classId` llega desde `GameBootstrap` → `Player.Initialize`

### Fase 2 — Casting ✅
- `Assets/Scripts/Skills/SkillCooldownManager.cs` — cooldown por skill
- `Assets/Scripts/Skills/SkillCaster.cs` — cast time (coroutine), valida unlock/mana/cooldown, consume mana al inicio, cooldown al completar
- `Assets/Scripts/Skills/Targeting/` — `ISkillTargeting`, `SelfTargeting`, `MouseTargeting`, `MoveDirectionTargeting`, `SkillTargetingFactory`
- Inputs `Skill1/2/3` (Q/E/R + gamepad LB/RB/Y) wired en `PlayerInputs.cs` vía `GetEquippedSkillIds()` (primeras 3 desbloqueadas)

### Fase 3 — Efectos ✅
- `Assets/Scripts/Skills/Effects/` — `ISkillEffect`, `SkillCastContext`, `DamageAreaEffect`, `StunEffect`, `SlowEffect`, `SelfBuffEffect`, `DashEffect`, `SkillEffectFactory`
- `Assets/Scripts/Skills/StatusEffects/` — `StatusEffectManager`, `StatusEffect`, `StunStatusEffect`, `SlowStatusEffect`, `BuffStatusEffect`
- `Mob.cs` — `AddStun/RemoveStun`, `AddSlow/RemoveSlow`, `IsStunned`, `IsSlowed`, `_baseSpeed`, `_slowFactor`
- `MobAI.cs` — `Tick()` retorna si `IsStunned`
- `Player.cs` — `CombatStats` con buff multipliers + `AddBuffMultiplier/RemoveBuffMultiplier`
- `PlayerMovement.cs` — `BeginDash/DashStep/EndDash`, `Move()` ignora si dashing
- `PlayerResources.cs` — mana regen pasivo (`manaRegenPerSecond` = 3 en `player.json`)
- `GameBootstrap.cs` — crea `StatusEffectManager`
- `EnemyHealthBarUI.cs` — texto "STUN"/"SLOW" encima de la barra (ya wired por el usuario, funciona)

### Fase 4 — UI (scripts ✅, armado en Unity ⏳ PENDIENTE)
**Scripts creados y compilando** (Tundra build success, solo warnings preexistentes de FindObjectOfType):
- `Assets/Scripts/UI/Skills/SkillSlotUI.cs` — slot individual de la hotbar
- `Assets/Scripts/UI/Skills/SkillHotbarUI.cs` — 3 slots, cooldown overlay, mana, click
- `Assets/Scripts/UI/Skills/SkillUnlockNotificationUI.cs` — aviso al desbloquear
- `Assets/Scripts/UI/Skills/SpellBookEntryUI.cs` — entrada del spellbook
- `Assets/Scripts/UI/Skills/SpellBookUI.cs` — panel spellbook (se abre con tecla B)
- `Assets/Scripts/GameBoostrap.cs` — **modificado**: 3 serialized fields nuevos (`_skillHotbar`, `_skillUnlockNotification`, `_spellBook`) + `Initialize` después de `_player.Initialize`
- `localization.json` — claves nuevas: `skill.unlocked`, `spellbook.title`, `spellbook.requiresLevel`

---

## 2. Lo que falta hacer en Unity (Fase 4)

### 2.1 Drag & drop en GameBootstrap
En el GameObject que tiene `GameBootstrap`, asignar:
- `Skill Hotbar` → objeto que tendrá `SkillHotbarUI`
- `Skill Unlock Notification` → objeto que tendrá `SkillUnlockNotificationUI`
- `Spell Book` → objeto que tendrá `SpellBookUI`

### 2.2 Hotbar — `SkillHotbarUI` con 3 hijos `SkillSlotUI` (en orden Q/E/R)
Cada slot necesita:
- `Icon` → **IMG** (Image, sprite de la skill)
- `Cooldown Overlay` → **IMG** tipo **Filled** (el código lo fuerza en Awake, para el barrido de cooldown)
- `Locked Overlay` → **GO** (visible cuando la skill no está desbloqueada)
- `No Mana Overlay` → **GO** (visible si no alcanza la mana)
- `Key Label` → **TXT** (TMP, ej: "Q")
- `Mana Cost Text` → **TXT** (TMP, costo de la skill)
- `Cooldown Text` → **TXT** (TMP, segundos restantes)
- `Button` → **BTN** (Unity UI Button — el click activa la skill)

### 2.3 Aviso de unlock — `SkillUnlockNotificationUI`
- `Panel` → **GO** (se activa/desactiva)
- `Title Text` → **TXT** ("Nueva Habilidad Desbloqueada!")
- `Skill Name Text` → **TXT**
- `Icon` → **IMG**

### 2.4 Spellbook — `SpellBookUI` (se abre con **B**)
- `Panel` → **GO**
- `Entry Container` → **Transform** (lista de entradas)
- `Entry Prefab` → **GO** con `SpellBookEntryUI`
- `Title Text` → **TXT**
- `Close Button` → **BTN**
- `Toggle Key` → B por defecto (configurable en inspector)

### 2.5 SpellBookEntryUI (prefab, uno por skill)
- `Icon` → **IMG**
- `Name Text` → **TXT**
- `Description Text` → **TXT**
- `Requirement Text` → **TXT** (muestra "Requiere Nv. X" si está bloqueada)
- `Locked Overlay` → **GO**

### 2.6 ICONOS OBLIGATORIOS (importante)
Poner sprites en `Assets/Assets/Resources/Skills/Icons/` con el **nombre del id** de cada skill:
- `whirlwind.png`
- `dash_strike.png`
- `frost_nova.png`
- `meteor.png`

Si no está el sprite, el slot/entrada queda con el sprite por defecto (NO crashea).

---

## 3. Debug útil

- `2` en juego → +50 XP (dispara unlocks)
- `3` en juego → `DebugPrintSkills()` en consola
- Bindings: `Skill1`=Q, `Skill2`=E, `Skill3`=R
- Log del editor: `%LOCALAPPDATA%\Unity\Editor\Editor.log`

## 4. Notas / gotchas

- `character?.classId` es obligatorio (`GameBootstrap` → `_player.Initialize`); personajes viejos sin clase NO desbloquean skills.
- `Unity 6000.4.2f1` estaba abierto en play mode → no se puede correr batch mode ni cerrar desde afuera.
- Los errores CS1061 de `Skill1/2/3` eran de un compile viejo, ya resuelto.
- Unity estaba en Play Mode con la escena; el recompile de mis scripts Fase 4 ya corrió y compiló OK.
