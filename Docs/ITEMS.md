# Spec: Sistema de Items

> Especificacion del sistema de items del juego: estructura de datos, los 2 sets del warrior (visual), equipamiento y persistencia. Actualizado: 2026-08-08.

---

## 1. Objetivo

Permitir crear items de distintos **tipos** (equipables, consumibles, etc.) y, dentro de los equipables, de distintos **slots** (casco, pecho, guantes, botas, capa, arma, mano izq). El modelo del player ya contiene las piezas de los **2 sets del warrior** (T1 y T2); al equipar un item se activa la pieza visual correspondiente y se desactiva la del otro set. Las pociones y demas items futuros caen en la misma estructura.

---

## 2. Modelo de datos — `Assets/Scripts/ClassJson/Items.cs`

```csharp
[Serializable] public class ItemsConfig { public Item[] items; }

[Serializable] public class Item
{
    public string id;              // unico, ej: warrior_helmet_t2
    public string displayNameKey;  // key de localization.json
    public string icon;            // path/sprite (vacio por ahora)
    public ItemType type;
    public EquipmentSlot slot;
    public ItemRarity rarity;
    public int levelRequirement;
    public string visualKey;       // nombre de la pieza del modelo (toggle por nombre)
    public ItemStats stats;
    public ItemEffect effect;      // solo consumibles
}
```

### Enums
```csharp
public enum ItemType     { Equipment, Consumable }                          // extensible (quest, etc.)
public enum EquipmentSlot{ Helmet, Chest, Gloves, Boots, Cape, Weapon, OffHand }
public enum ItemRarity   { Common, Uncommon, Rare, Epic, Legendary }
```

### Stats y efecto
```csharp
[Serializable] public class ItemStats
{
    public int armor; public int health; public int mana; public int damage;
    public int strength; public int vitality; public int intelligence; public int dexterity;
}

[Serializable] public class ItemEffect
{
    public int heal;        // HP que restaura
    public int restoreMana; // mana que restaura
}
```

> **Nota JsonUtility:** los enums se parsean por indice numerico (`"type": 0` = Equipment, `"slot": 0` = Helmet). Si un campo no esta en el JSON queda en su valor por defecto.

---

## 3. Datos — `Assets/Assets/Resources/GameData/Config/items.json`

14 items. Los `visualKey` son los **nombres exactos de las piezas del modelo FBX** (`Player chibi 2 sets.fbx`).

| id | slot | rarity | req lvl | visualKey (pieza modelo) |
|---|---|---|---|---|
| warrior_helmet_t1 | Helmet (0) | Common | 1 | `Casco T1` |
| warrior_chest_t1 | Chest (1) | Common | 1 | `Remera T1` |
| warrior_gloves_t1 | Gloves (2) | Common | 1 | `Guantes T1` |
| warrior_boots_t1 | Boots (3) | Common | 1 | `Botas T1` |
| warrior_sword_t1 | Weapon (5) | Common | 1 | `Espada T1` |
| warrior_helmet_t2 | Helmet (0) | Rare | 5 | `CascoPlate T2` |
| warrior_chest_t2 | Chest (1) | Rare | 5 | `ArmaduraPlate T2` |
| warrior_gloves_t2 | Gloves (2) | Rare | 5 | `GuantePlate T2` |
| warrior_boots_t2 | Boots (3) | Rare | 5 | `BotasPlate T2` |
| warrior_sword_t2 | Weapon (5) | Rare | 5 | `EspadaPlate T2` |
| warrior_cape_t2 | Cape (4) | Rare | 5 | `Capa T2` |
| warrior_shield_t2 | OffHand (6) | Rare | 5 | `EscudoPlate T2` |
| health_potion | — | Common | 1 | *(consumible, effect.heal=100)* |
| mana_potion | — | Common | 1 | *(consumible, effect.restoreMana=50)* |

- El set **T2 (Plate)** es mejor que el **T1**: mas armor/health y mejor damage.
- Las piezas del modelo detectadas (via parseo del FBX): `Casco T1`, `CascoPlate T2`, `Remera T1`, `ArmaduraPlate T2`, `Guantes T1`, `GuantePlate T2`, `Botas T1`, `BotasPlate T2`, `Espada T1`, `EspadaPlate T2`, `Capa T2`, `EscudoPlate T2`, `cuerpo` (base).

### Localization — `localization.json`
Keys agregadas: `item.warrior_*_t1/t2`, `item.warrior_cape_t2`, `item.warrior_shield_t2`, `item.rarity.common/rare`, `slot.helmet/chest/gloves/boots/cape/weapon/offhand` (EN/ES).

---

## 4. Equipamiento — `Assets/Scripts/Items/PlayerEquipment.cs`

`MonoBehaviour` (se auto-agrega al Player si falta). Indexa items por slot: `Item[] _equipped = new Item[7]`.

**API publica:**
- `Equip(string itemId) : bool` — valida que exista, sea `Equipment`, slot valido y cumpla `levelRequirement`. Setea el item, aplica visual (`ApplySlot`) y dispara `OnEquipmentChanged`.
- `Unequip(EquipmentSlot slot)` — limpia el slot, aplica visual (`ClearSlot`) y dispara evento.
- `GetItemInSlot(slot) : Item`
- `GetEquippedIds() : string[]` — ids por slot (para save).
- `TotalStats : ItemStats` — suma de stats de todos los items equipados.
- `FindItemById(id) : Item`
- `UseConsumable(id) : bool` — valida que sea `Consumable` con `effect`, aplica `heal` via `Resources.Heal` y `restoreMana` via `Resources.RestoreMana` (nuevo metodo en `PlayerResources`), y loguea HP/MP resultante.
- `event Action OnEquipmentChanged` — refresca `PlayerResources.RefreshMaxResources()`.

**Integracion en `Player.cs`:**
- Property `Equipment`; se crea en `Initialize()` con `config.ItemsConfig` y `saveData?.equippedItemIds`.
- `CombatStats` suma del equipo: `strength/vitality/intelligence/dexterity` + `equipment.damage` a `PhysicalAttack` + `equipment.armor` a `PhysicalDefense`.

> **CRITICO — orden de inicializacion (fix 2026-08-08):** `Equipment.Initialize()` dispara `OnEquipmentChanged` → `PlayerResources.RefreshMaxResources()`. Si `Resources` aun no esta inicializado, `UpdateResources` usa `_resourcesConfigs` null → `NullReferenceException` en `PlayerResources.UpdateResources:134`. Ese NRE cortaba `GameBootstrap.Start()` entero: no cargaba save, `_playerInputs.Initialize` nunca corria (`_movement` null → NRE cada frame en `PlayerInputs.Update:133`, sin movimiento), y la UI no se inicializaba. **Orden correcto en `Player.Initialize`:** Stats → Progression → Movement → Combat → bloque saveData/Resources (`Resources.Initialize` + `SetCurrentValues`) → **`Equipment.Initialize`** → Skills → Caster. El `Equipment.Initialize` queda en Player.cs:162-165, despues del bloque if/else de Resources.

**Integracion en `PlayerResources.cs`:**
- Fix preexistente: `_player` nunca se asignaba en `Initialize` (agregado).
- `UpdateResources` suma del equipo: `(vitality + equipment.vitality) * healthPerPoint + equipment.health` → `MaxHp`; igual con intelligence/mana → `MaxMana`.
- Nuevo metodo publico `RefreshMaxResources()` — recalcula HP/MP maximos al equipar/desequipar.

---

## 5. Visual — `Assets/Scripts/Items/PlayerVisualEquipment.cs`

`MonoBehaviour`. Mapea slot → nombres de piezas del modelo:

```csharp
Helmet  → { "Casco T1", "CascoPlate T2" }
Chest   → { "Remera T1", "ArmaduraPlate T2" }
Gloves  → { "Guantes T1", "GuantePlate T2" }
Boots   → { "Botas T1", "BotasPlate T2" }
Cape    → { "Capa T2" }
Weapon  → { "Espada T1", "EspadaPlate T2" }
OffHand → { "EscudoPlate T2" }
```

- `CachePieces()` — indexa todos los transforms del player (`GetComponentsInChildren(true)`, incluye inactivos) por nombre.
- `ApplySlot(slot)` — activa la pieza con `visualKey == item.visualKey` y apaga las demas del mismo slot.
- `ClearSlot(slot)` — apaga todas las piezas del slot.
- `RefreshAll()` — aplica el estado segun los items equipados (usado al cargar).

> El modelo instanciado en `Player.prefab` es "Player chibi 2 sets" (guid `4067be15...`) y ya trae overrides `m_IsActive` por pieza.

---

## 6. Persistencia

- `Assets/Scripts/Save/PlayerSaveData.cs` — campos `string[] equippedItemIds` (id por slot, `""` si vacio) y `ItemStack[] inventoryItems` (inventario).
- `Assets/Scripts/Save/GameSaveService.cs` — `SaveGameplay` guarda `player.Equipment?.GetEquippedIds()` y `player.Inventory?.Stacks`; `CreateNewSave` inicializa ambos vacios.
- `Player.Initialize(saveData)` — restaura el equipo desde `saveData.equippedItemIds` y el inventario desde `saveData.inventoryItems` (via `RestoreInventory`).

---

## 7. Inventario — `Assets/Scripts/Items/PlayerInventory.cs`

`MonoBehaviour` (se auto-agrega al Player si falta). Almacena stacks por `itemId` (sin cap max por ahora).

**API publica:**
- `AddItem(string itemId, int count = 1)` — suma a un stack existente o crea uno nuevo; dispara `OnInventoryChanged`.
- `RemoveItem(string itemId, int count = 1) : bool` — resta; si el stack llega a 0 lo elimina; dispara evento.
- `HasItem(string itemId) : bool`
- `GetCount(string itemId) : int`
- `Stacks : IReadOnlyList<ItemStack>` — para iterar (UI/save).
- `event Action OnInventoryChanged` — lista para refrescar la UI futura.

**`ItemStack`** (en `ClassJson/Items.cs`): `{ string itemId; int count; }`, `[Serializable]` para guardar/restaurar con JsonUtility.

**Conexion con consumibles:** `PlayerEquipment.UseConsumable(id)` ahora exige el item en el inventario (`HasItem`) y al usarlo resta 1 (`RemoveItem`). Asi las pociones se compran/obtienen (se agregan con `AddItem`) y se gastan desde el inventario.

**Integracion en `Player.cs`:** property `Inventory`; se crea en `Initialize()` (GetComponent o AddComponent) y se restaura desde `saveData.inventoryItems`. Va despues de `Equipment.Initialize`, antes de `Skills`.

---

## 8. Como probar en juego

> **Nota:** el proyecto usa SOLO el nuevo Input System (`activeInputHandler: 1`) — `Input.GetKeyDown` legacy no funciona; hay que usar `Keyboard.current` del Input System.

Desde la consola de Unity (o un script de debug):

```csharp
player.Equipment.Equip("warrior_helmet_t2");   // activa "CascoPlate T2" + stats
player.Equipment.Equip("warrior_sword_t1");    // activa "Espada T1"
player.Equipment.Unequip(EquipmentSlot.Weapon);// apaga la espada
player.Equipment.UseConsumable("health_potion"); // cura HP
```

### Test por teclado — `EquipmentDebugTester.cs`

MonoBehaviour que se agrega a cualquier objeto de la escena (ej. el Player); se auto-encuentra el `Player` con `FindObjectOfType`. Usa `Keyboard.current` del nuevo Input System.

| Tecla | Accion |
|---|---|
| `3` | Casco (cicla: T1 → T2 → desequipar) |
| `4` | Pecho |
| `5` | Guantes |
| `6` | Botas |
| `7` | Espada |
| `8` | Capa |
| `9` | Escudo |
| `0` | Pocion de vida (`health_potion`) |
| `-` | Pocion de mana (`mana_potion`) |
| `=` | Imprime stats completos (nivel, XP, HP/MP, stats base/equipo, combat, items equipados) |
| `Enter` | Dump de la jerarquia visual (nombres de piezas + estado activo) |
| `[` | Da 1 `warrior_sword_t1` al inventario |
| `]` | Da 1 `health_potion` al inventario |
| `\` | Da 1 `mana_potion` al inventario |
| `;` | Imprime el inventario (stacks) |

> Las teclas 1-2 ya las usa el test viejo (`Test1` en `PlayerInputs`: 1=daño, 2=XP, 3=skills debug), por eso el tester arranca en 3.

> **Nota:** `0`/`-` ahora usan las pociones **del inventario**: si no tenes `health_potion`/`mana_potion` en el inventario, `UseConsumable` lo rechaza. Primero dale con `]`/`\`.

---

## 9. Pendiente (fase 5+)

> El detalle de las fases de inventario esta en **[`Docs/INVENTORY.md`](INVENTORY.md)**: drops de mobs (fase 6), afijos/affixes (fase 7), drops en el mundo como bolsa pickup con click/espacio (fase 6b) y la UI que conecta inventario con equipment (fase 8).

- **Drops de mobs**: `dropTable` en `enemies.json` + `MobDropHandler` en `Mob.HandleDeath`.
- **Drops en el mundo**: `WorldDrop` (bolsa procedural) + `DropRegistry`; click (raycast + auto-move si esta lejos) o espacio dentro del radio (accion `Interact`).
- **Afijos (affixes)**: `ItemAffix` + `affixes.json` + generacion aleatoria + soporte en stacks (`affixHash`).
- **UI del inventario + equipment**: panel con slots, accion `I`, equipar/usar por click (`OnInventoryChanged` ya lista).
- Icons de items (`Item.icon`).
- Mas tipos de items y rarezas.
- Set bonuses por equipar piezas del mismo set.
