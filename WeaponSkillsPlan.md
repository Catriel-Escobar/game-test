# Plan: Skills atadas a las Armas (Clase única)

> Rama: `feature/skills-from-weapons`
>
> Cambio de enfoque: **una sola clase** y las skills **ya no vienen de la clase ni se desbloquean por nivel** — las otorgan los items equipados en las manos.

---

## Objetivo

| Antes | Después |
|-------|---------|
| Varias clases (warrior/mage) con base stats distintas | **1 sola clase** (stats base únicos, sin selección en creación) |
| Skills por clase (`classId`), desbloqueadas por `requiresLevel` | Skills **otorgadas por el equipo** |
| Hotbar = primeras 3 skills desbloqueadas de la clase | Hotbar = skills activas del **Weapon + OffHand** |

## Regla de skills por item (contrato)

- **Item en mano principal** (slot `Weapon`): otorga **2 skills**.
  - Ej: `warrior_sword_t1` → `seismic_strike` (sismo) + `dash_strike` (dash).
  - Ej: `mage_staff_t1` (staff) → 2 skills de mago (p.ej. `frost_nova` + `meteor`).
- **Item en mano secundaria** (slot `OffHand`): otorga **1 skill**.
  - Ej: `warrior_shield_t2` (escudo) → `bastion` (shield).
  - Ej: `mage_book_t1` (libro/pergamino) → 1 skill de mago (p.ej. `arcane_barrage`).
- **Builds válidas** (cualquier arma + cualquier offhand):
  - staff + libro, staff + escudo, espada + libro, espada + escudo.
- **No hay mecánica de 2 manos** — no bloquea OffHand.
- **Forward-compat (futuro)**: el modelo ya soporta **armas de 2 manos que otorguen 3 skills** — `skillIds` es un array y `PlayerSkills` concatena Weapon + OffHand con cap de 3. Un arma de 2 manos con 3 skillIds llenará el hotbar sin OffHand. Cuando existan, solo habrá que decidir si bloquean el OffHand (regla de equipamiento, fuera de scope actual).
- Las skills **siempre están disponibles al equipar** el item (se elimina el gating por `requiresLevel`).
- Combinación máxima: Weapon (2) + OffHand (1) = **3 skills activas** (el hotbar actual es de 3 slots Q/E/R, no cambia).

---

## Modelo de Datos

### `items.json` — nuevo campo en `Item`

Solo se agrega **`skillIds`** (lista de ids referenciando `skills.json`, el catálogo global). No se agrega flag de 2 manos.

```json
{
    "id": "warrior_sword_t1",
    "slot": "Weapon",
    "skillIds": ["seismic_strike", "dash_strike"],
    "stats": { ... }
}
```

```json
{
    "id": "mage_staff_t1",
    "slot": "Weapon",
    "skillIds": ["frost_nova", "meteor"],
    "stats": { ... }
}
```

```json
{
    "id": "warrior_shield_t2",
    "slot": "OffHand",
    "skillIds": ["bastion"],
    "stats": { ... }
}
```

```json
{
    "id": "mage_book_t1",
    "slot": "OffHand",
    "skillIds": ["arcane_barrage"],
    "stats": { ... }
}
```

### `skills.json` — catálogo sin clase

- Se **elimina el campo `classId`** (ya no hay clases).
- Se **elimina/ignora `requiresLevel`** (skills siempre disponibles al equipar). Se borra del modelo para no confundir.
- El resto del modelo (`targeting`, `effects`, `manaCost`, `cooldown`, `castTime`, `animationId`) **no cambia** — el pipeline de casting se reutiliza tal cual.

### `character_classes.json` / selección de clase

- **Eliminar la selección de clase** en la creación de personaje.
- `character_classes.json` queda con **1 sola entrada** (o se elimina y los base stats pasan a `player.json`).
- `CharacterData.classId` queda sin uso (se puede conservar en el modelo de save por compatibilidad, ignorado).

---

## Cambios de Runtime

### `PlayerSkills.cs` — reescrito
Deja de ser "skills de la clase desbloqueadas por nivel" y pasa a ser un **provider de skills activas según el equipo**:

- Guarda ref al `PlayerEquipment` y a `SkillsConfig`.
- Escucha `PlayerEquipment.OnEquipmentChanged` → recalcula las skills activas y dispara `OnSkillsChanged` (para que hotbar/spellbook se refresquen).
- `GetEquippedSkillIds()`: arma las skills activas en orden — **primero Weapon (2), luego OffHand (1)**:
  ```text
  Weapon.skillIds[0] → slot Q
  Weapon.skillIds[1] → slot E
  OffHand.skillIds[0] → slot R
  ```
- Se eliminan `ClassId`, `Unlock()`, `IsUnlocked()`, `GetClassSkills()` (o se reemplazan por acceso directo a las skills activas). `GetSkill(id)` se mantiene (lo usa `SkillCaster`).

### `SkillCaster.cs` — validación
- `TryCastSkill`: en vez de validar `IsUnlocked()`, valida que la skill id esté **dentro de las skills activas** del equipo (`_player.Skills.GetEquippedSkillIds()`).
- El resto del casting (mana, cooldown, cast time, efectos) **no cambia**.

### `SkillHotbarUI.cs`
- Se inicializa con las skills **activas** del equipo (no las de clase).
- Se suscribe a `PlayerEquipment.OnEquipmentChanged` (además de los eventos actuales) para **re-construir los slots** al equipar/desequipar.
- Mantiene 3 slots (Q/E/R) con cooldown overlay + mana (sin cambios de UI).

### `SpellBookUI.cs`
- Lista las skills activas (del equipo) en vez de `GetClassSkills()`.

### `SkillUnlockService.cs` — **ELIMINAR**
- Ya no hay unlock por nivel. Se borra junto con su wiring en `Player.cs`.

### `Player.cs` / `GameBootstrap.cs`
- `Initialize(config, saveData)` **sin `classId`**.
- Se elimina la creación de `SkillUnlockService`.
- `GameBootstrap` deja de pasar `character?.classId`.
- **Orden de inicialización importante**: `Equipment` debe inicializarse antes que `Skills` (o `Skills` se suscribe al evento `OnEquipmentChanged` y fuerza un refresh inicial al terminar el setup).

### `PlayerInputs.cs`
- `TryCastEquippedSkill(i)` sigue igual: casta `GetEquippedSkillIds()[i]`. Sin cambios de lógica (solo ya no depende de "unlocked").

### Save
- `PlayerSaveData.unlockedSkillIds` queda **obsoleto** (se ignora al cargar / se elimina del modelo). Las skills se derivan siempre del equipo guardado (`equippedItems`).

### UI de notificación (opcional)
- `SkillUnlockNotificationUI` se puede **reutilizar como "skill gained"**: mostrar al equipar un item que otorgue una skill nueva ("Nueva skill: Seismic Strike"). Si no, se elimina.

---

## Catálogo de contenido inicial

Se mantienen las skills existentes y se agregan items nuevos:

| skill | rol | otorgada por |
|-------|-----|--------------|
| `seismic_strike` | sismo (damage_aoe + stun, self) | espada (Weapon) |
| `dash_strike` | dash (dash + stun) | espada (Weapon) |
| `bastion` | shield (self-buff) | escudo (OffHand) |
| `frost_nova` | damage_aoe self + slow | staff (Weapon) |
| `meteor` | damage_aoe mouse + stun | staff (Weapon) |
| `arcane_barrage` | damage_aoe mouse rango largo | libro (OffHand) |

Items nuevos en `items.json`:
- `mage_staff_t1` — Weapon, skillIds: `frost_nova`, `meteor`.
- `mage_book_t1` — OffHand, skillIds: `arcane_barrage`.
- (opcional) `mage_staff_t2` / `mage_book_t2` con stats mejores.

---

## Orden de Implementación

1. **Modelo** — `Items.cs`: agregar `skillIds` a `Item`; `SkillData.cs`: quitar `classId` y `requiresLevel`.
2. **Data** — `items.json`: agregar `skillIds` a armas/escudo + items nuevos (staff, libro); `skills.json`: quitar `classId`/`requiresLevel`.
3. **Runtime skills** — reescribir `PlayerSkills` (provider activo por equipo), actualizar `SkillCaster`.
4. **Wiring** — `Player.cs` (sin classId, sin SkillUnlockService), `GameBootstrap.cs`.
5. **UI** — `SkillHotbarUI` (refresh al cambiar equipo), `SpellBookUI`.
6. **Creación de personaje** — quitar selección de clase (`CreateCharacterModal`, `CharacterSelectionService`), 1 sola entrada en `character_classes.json` (o mover stats a `player.json`).
7. **Eliminaciones** — `SkillUnlockService.cs`, `SkillUnlockNotificationUI.cs` (o repurposear), `unlockedSkillIds` en save.
8. **Verificación** — compilar (csc manual), probar en Unity: espada → Q/E = sismo/dash; + escudo → R = shield; + libro → R = arcane barrage; staff → Q/E = frost nova/meteor; espada+libro → 3 skills.

---

## Archivos Involucrados

### Modificar
| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/ClassJson/Items.cs` | `Item`: + `skillIds` |
| `Assets/Scripts/Skills/SkillData.cs` | quitar `classId`, `requiresLevel` |
| `Assets/Scripts/Skills/PlayerSkills.cs` | reescritura → skills activas por equipo |
| `Assets/Scripts/Skills/SkillCaster.cs` | validar skill activa (no unlocked) |
| `Assets/Scripts/Player/Player.cs` | sin classId, sin SkillUnlockService |
| `Assets/Scripts/GameBoostrap.cs` | no pasar classId |
| `Assets/Scripts/Player/PlayerInputs.cs` | (verificar, mínimo cambio) |
| `Assets/Scripts/UI/Skills/SkillHotbarUI.cs` | refresh por cambio de equipo |
| `Assets/Scripts/UI/Skills/SpellBookUI.cs` | skills activas del equipo |
| `Assets/Scripts/CharacterSelection/*` | quitar selección de clase |
| `Assets/Resources/GameData/Config/items.json` | skillIds + items nuevos (staff, libro) |
| `Assets/Resources/GameData/Config/skills.json` | quitar classId/requiresLevel |
| `Assets/Resources/GameData/Config/character_classes.json` | 1 clase / stats únicos |
| `Assets/Resources/GameData/Config/localization.json` | keys nuevas (staff, libro) |

### Eliminar
| Archivo | Razón |
|---------|-------|
| `Assets/Scripts/Skills/SkillUnlockService.cs` | ya no hay unlock por nivel |
| `Assets/Scripts/UI/Skills/SkillUnlockNotificationUI.cs` | o repurposear a "skill gained" al equipar |
| `PlayerSaveData.unlockedSkillIds` | obsoleto (skills derivadas del equipo) |

### Crear
| Archivo | Rol |
|---------|-----|
| este documento | plan |
| items nuevos en `items.json` | staff (Weapon), libro (OffHand) |
