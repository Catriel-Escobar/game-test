# Spec: Sistema de Inventario

> Especificacion del inventario del player: modelo de datos, persistencia y las fases pendientes (drops, afijos/affixes y UI que conecta inventario con equipment). Actualizado: 2026-08-08.

---

## 1. Objetivo

El player posee items (no solo los que tiene equipados), organizados en stacks por `itemId`. Los items se obtienen por drops de mobs (pendiente), pueden llevar afijos que modifiquen sus stats (pendiente), y se ven/gestionan desde una UI que los conecta con el sistema de equipment ya existente (pendiente).

**Estado actual:** el modelo de datos + persistencia + consumo desde inventario + **drops de mobs** + **afijos/affixes** + **drops en el mundo (bolsa pickup por click/espacio)** estan **implementados**. La UI (fase 8) esta **pendiente**.

---

## 2. Estado actual — IMPLEMENTADO

### 2.1 Modelo de datos

- `Assets/Scripts/ClassJson/Items.cs` — `ItemStack`:
  ```csharp
  [Serializable] public class ItemStack
  {
      public string instanceId;  // unico por item (cada item es distinto por sus afijos)
      public string itemId;      // id del item en items.json
      public int count;          // cantidad en el stack (1 para items unicos, >1 para stackables como pociones)
      public ItemAffix[] affixes;
  }
  ```
  > El stacking se define por config en `items.json`: el campo `maxStackSize` del `Item` (0/1 = no stackea, cada item es **instancia unica** con su `instanceId`; >1 = stack maximo, ej. pociones con `maxStackSize: 20`). Los items de equipo/afijos quedan unicos por instancia; solo los stackables se agrupan.

### 2.2 `Assets/Scripts/Items/PlayerInventory.cs`

`MonoBehaviour` (se auto-agrega al Player si falta). Lista de `ItemStack` (`List<ItemStack>`), sin cap max por ahora.

**API publica:**
- `AddItem(string itemId, int count = 1, ItemAffix[] affixes = null, string instanceId = null)` — si el item es **stackable** (`maxStackSize > 1`), agrega `count` a los stacks parciales del mismo `itemId`+afijos (llenando hasta `maxStackSize`) creando nuevos stacks si hace falta. Si **no** es stackable, crea una **instancia unica por item** (con `count` se crean `count` instancias, cada una con su GUID). Si se pasa `instanceId` (restore de save), crea un único stack con ese count y lo conserva. Dispara `OnInventoryChanged`.
- `RemoveItem(string itemId, int count = 1, ItemAffix[] affixes = null) : bool` — decrementa `count` desde los stacks que matchean por `itemId` + afijos (desde el final); borra el stack si llega a 0; si no alcanzan devuelve false; dispara evento.
- `HasItem(string itemId) : bool`
- `GetCount(string itemId) : int`
- `Stacks : IReadOnlyList<ItemStack>` — iterar para UI/save.
- `event Action OnInventoryChanged` — pensado para refrescar la UI futura.

### 2.3 Persistencia

- `Assets/Scripts/Save/PlayerSaveData.cs` — campo `ItemStack[] inventoryItems`.
- `Assets/Scripts/Save/GameSaveService.cs` — `SaveGameplay` guarda `player.Inventory?.Stacks`; `CreateNewSave` inicializa `new ItemStack[0]`.
- `Assets/Scripts/Player/Player.cs` — property `Inventory`; se crea en `Initialize()` (GetComponent o AddComponent) y se restaura desde `saveData.inventoryItems` con `RestoreInventory(stack)`. Va despues de `Equipment.Initialize`, antes de `Skills`.

### 2.4 Consumibles

- `PlayerEquipment.UseConsumable(id)` ahora exige el item en el inventario (`HasItem`) y al usarlo resta 1 (`RemoveItem`). Flujo: `Inventory.AddItem("health_potion")` → `Equipment.UseConsumable("health_potion")` → resta 1 y cura HP.

### 2.5 Tester por teclado — `EquipmentDebugTester.cs`

| Tecla | Accion |
|---|---|
| `[` | Da 1 `warrior_sword_t1` al inventario |
| `]` | Da 1 `health_potion` al inventario |
| `\` | Da 1 `mana_potion` al inventario |
| `;` | Imprime el inventario (stacks) |

> `0`/`-` usan pociones **del inventario**: si no tenes, `UseConsumable` lo rechaza. Primero dártelas con `]`/`\`.

---

## 3. Fase 6 — Drops de mobs (IMPLEMENTADO)

### Objetivo
Al morir un mob, dropear items al inventario del player que le dio el golpe final (el `_lastAttacker`, el mismo que recibe la XP).

### Implementacion
1. **Config en `enemies.json`** — nueva seccion `dropTable` por enemigo:
   ```json
   "dropTable": {
       "itemDrops": [
           { "itemId": "health_potion", "chance": 0.5, "minCount": 1, "maxCount": 1 },
           { "itemId": "warrior_helmet_t1", "chance": 0.12, "minCount": 1, "maxCount": 1 }
       ]
   }
   ```
   - Zombie: health_potion (50%), mana_potion (25%), warrior_helmet_t1 (12%), warrior_chest_t1 (10%).
   - Skeleton: health_potion (30%), warrior_gloves_t1 (12%), warrior_boots_t1 (12%), warrior_sword_t1 (8%), warrior_helmet_t2 (2%).
2. **Clases de config** en `ClassJson/Enemies.cs`:
   ```csharp
   [Serializable] public class DropTableConfig { public ItemDrop[] itemDrops; }
   [Serializable] public class ItemDrop { public string itemId; public float chance; public int minCount; public int maxCount; }
   ```
   > **Nota JsonUtility:** campos numericos faltantes quedan en 0; `chance` 0 = no dropea. `minCount/maxCount` 0 = 1.
3. **`Assets/Scripts/Items/MobDropHandler.cs`** — estatico, `RollDrops(Mob, Player)`:
   - Para cada `itemDrop`, `Random.value <= chance` → `count = Random.Range(minCount, maxCount+1)` (si `maxCount <= minCount`, usa `max(1, minCount)`).
   - Valida que el item exista en `items.json` via `player.Equipment.FindItemById` (si no, loguea warning y lo ignora).
   - Los items de equipo ruedan **afijos siempre**: `RollAffixes(DropRarityFor(item.rarity))` — si el item es `Common` usa `Uncommon` como minimo (1 afijo), y escala con la rarity del item (Rare=2, Epic=3, Legendary=4). Las pociones no llevan afijos.
   - Spawnea una bolsa en el mundo: `WorldDrop.Spawn(itemId, count, affixes, pos)` — el pickup (click/espacio) hace `Inventory.AddItem(itemId, count, affixes)`.
   - Log por consola: `[Drops] zombie dejo caer 'health_potion' x1 en el mundo` (+ "con N afijos" si los lleva).
4. **Hook de muerte:** en `Mob.HandleDeath()` (`Assets/Scripts/Mob/Mob.cs`), junto a `AddExperience`, se llama `MobDropHandler.RollDrops(this, player)`. Nuevo accessor `Mob.EnemyConfig` expone la config para el handler.

### Verificacion
- Matar mobs → en consola se ven los `[Drops]` que salen (equipo con `N afijos`).
- `;` en el tester para confirmar que los items llegan al inventario.
- Guardar/recargar → los drops persisten; revisar `save_<id>.json` para confirmar que `affixes` se guarda.

### Pendiente menor
- `goldChance` (oro) no implementado — no hay sistema de oro todavia.
- Notificacion flotante/UI de drops (futuro).

---

## 4. Fase 7 — Afijos / Affixes (IMPLEMENTADO)

### Objetivo
Items con modificadores extra aleatorios (ej. +5 FUE, +10 armor, +20 HP) que se generan en el drop o se agregan manualmente.

### Implementacion
1. **Modelo** — en `ClassJson/Items.cs`:
   ```csharp
   [Serializable] public class ItemAffix
   {
       public string stat;    // stat: strength, vitality, intelligence, dexterity, armor, health, mana, damage
       public int value;      // bonus flat (ej: 5)
       public float percent;  // bonus porcentual (ej: 0.1 = +10%); 0 si es flat
   }

   [Serializable] public class AffixesConfig { public ItemAffixConfig[] affixes; }
   [Serializable] public class ItemAffixConfig
   {
       public string id;
       public string displayNameKey;
       public string stat;
       public bool isPercent;
       public int minValue;   // rango para generacion aleatoria
       public int maxValue;
   }

   [Serializable] public class EquippedAffixData { public ItemAffix[] affixes; } // wrapper para save (JsonUtility no serializa jagged arrays)
   ```
   - `ItemStack` gana `ItemAffix[] affixes` + `GetAffixKey()` (clave canonica `stat:value:percent|...`) y `HasAffixes()`.
2. **Datos** — `GameData/Config/affixes.json` con 10 afijos (flat: strength/vitality/intelligence/dexterity/armor/health/mana/damage; porcentual: armor_percent, damage_percent). `game.json` y `GameConfig` ganaron la key `affixes`; `ConfigBoostrap.AffixesConfig` la carga. Localization: keys `affix.*` (EN/ES).
 3. **`Assets/Scripts/Items/AffixService.cs`** — estatico:
    - `AffixCountForRarity(rarity)` — Common 0, Uncommon 1, Rare 2, Epic 3, Legendary 4.
    - `RarityForAffixCount(count)` — **inverso**: deriva la categoria del item desde cuantos afijos trae (0 Common, 1 Uncommon, 2 Rare, 3 Epic, >=4 Legendary). `ItemStack.GetRarity()` lo usa.
    - `RollAffixes(rarity, config)` — tira N afijos aleatorios del pool sin repetir stat; flat o porcentual segun `isPercent` (el `value` de config pasa a `value` o a `percent/100`).
    - `ApplyAffixes(baseStats, affixes)` — clona stats y aplica flat/percent por stat.
4. **Items con stacking por config** — `PlayerInventory.AddItem(itemId, count, affixes)`:
   - Items **stackables** (`maxStackSize > 1` en items.json, ej. pociones) se agrupan llenando stacks parciales hasta el max y creando nuevos si hace falta; comparten `itemId` y un solo `instanceId` por stack.
   - Items **no stackables** (equipo/afijos) crean una instancia unica por item con su GUID.
   - `RemoveItem` decrementa el `count` del stack (borra el stack al llegar a 0); matchea por `itemId` + afijos. `HasItem`/`GetCount` agregan todos los stacks del item.
5. **Equipamiento con afijos** — `PlayerEquipment`:
   - `Equip(string itemId, ItemAffix[] affixes = null)` — guarda el item y sus afijos por slot (`_equippedAffixes[7][]`).
   - `TotalStats` aplica `AffixService.ApplyAffixes(item.stats, slotAffixes)` a cada item equipado.
   - `GetEquippedAffixes() : ItemAffix[][]` + `GetEquippedAffixData() : EquippedAffixData[]` (para save) + `FromAffixData(data)` (para load) + `GetEquippedAffixesInSlot(slot)`.
6. **Persistencia** — `PlayerSaveData.equippedItemAffixes : EquippedAffixData[]` (indexado por slot, null si vacio). `GameSaveService` guarda `player.Equipment.GetEquippedAffixData()`; `Player.Initialize` pasa `FromAffixData(saveData.equippedItemAffixes)`. El inventario ya guarda afijos dentro de `ItemStack.affixes`.
 7. **Drops con afijos y rarity aleatoria** — `MobDropHandler` genera `AffixService.RollAffixes(RandomDropRarity(), affixesConfig)` para items de equipo dropeados (los consumibles no llevan afijos). `RandomDropRarity()` tira con peso: Uncommon 45%, Rare 30%, Epic 18%, Legendary 7% — asi aparecen todas las categorias. La **categoria del item** se deriva de sus afijos (`ItemStack.GetRarity()`), no del `rarity` estatico de items.json. En el tester, `;` y `=` muestran la categoria: (Comun), (Poco comun), (Raro), (Epico), (Legendario).

### Tester (nuevas teclas)
- `F1` — da 1 `warrior_helmet_t2` con afijos fijos (+5 vitality, +8 armor).
- `F2` — da 1 `warrior_sword_t2` con afijo porcentual (+15% damage).
- `F3` — da 1 `warrior_sword_t1` con afijos aleatorios de rareza Rare.
- `;` ahora muestra los afijos de cada stack; `=` muestra los afijos de los items equipados.

### Verificacion
- `F1`/`F2`/`F3` → `;` muestra afijos; `=` muestra stats con afijos aplicados; equipar con afijos persiste al guardar/recargar.
- Drops con afijos → se ven distintos stats entre drops del mismo item.
- `0`/`-` (pociones) siguen sin afijos y stackean normal.

---

## 5. Fase 6b — Drops en el mundo (bolsa) (IMPLEMENTADO)

### Objetivo
Al morir un mob, en vez de sumar directo al inventario, dejar caer una **bolsa 3D** en el mundo. Se agarra con **click** (si el player esta lejos, se acerca solo y recien al llegar la agarra) o con **espacio** (solo dentro de un radio especifico). La bolsa es un **placeholder procedural** (primitivas marrones, reemplazable por un modelo 3D real).

### Implementacion
1. **`Assets/Scripts/Items/WorldDrop.cs`** — `MonoBehaviour` de la bolsa:
   - Datos: `ItemId`, `Count`, `Affixes`, `pickupRadius` (default **1.5m**, `[SerializeField]`).
   - `WorldDrop.Spawn(itemId, count, affixes, position)` — fabrica la bolsa procedural (cubo marron "cuerpo" + esfera mas oscura "nudo") y agrega el componente.
   - **Colliders en modo trigger** — el player puede **traspasarla** (no bloquea el `CharacterController`); el raycast del click usa `QueryTriggerInteraction.Collide` para seguir golpeandola.
   - **Animacion** — la bolsa **gira** (45°/s) y flota suavemente (seno ±0.15m) para verse bien.
   - `TryPickup(Player)` — valida que el item exista (`player.Equipment.FindItemById`), hace `player.Inventory.AddItem(ItemId, Count, Affixes)` (crea `Count` instancias unicas) y destruye la bolsa (flag `_pickedUp` anti doble-pickup).
2. **`DropRegistry`** (misma archivo, estatica):
   - `Register`/`Unregister` (via `OnEnable`/`OnDisable`).
   - `FindDropAtScreenPoint(Vector2)` — raycast desde la camara por el punto del mouse → busca `WorldDrop` en el collider golpeado (click).
   - `FindNearestPickupable(Player)` — devuelve la bolsa mas cercana dentro de `pickupRadius` (espacio).
3. **`PlayerMovement.MoveTo(target, stopDistance, onArrived)` / `CancelMoveTo()`** (`Assets/Scripts/Player/PlayerMovement.cs`):
   - Auto-move con `CharacterController` hacia el destino (rota hacia el, corre, gravedad). Si hay input manual (`Move` con `sqrMagnitude > 0.01`) se cancela. Timeout de 10s por seguridad.
   - Al llegar a `stopDistance` invoca `onArrived`. Cancelar limpia `_velocity`/estado.
4. **Click — `PlayerCombat.OnBasicAttack`** (`Assets/Scripts/Player/PlayerCombat.cs`):
   - Antes del ataque, `TryInteractWithDrop()`: raycast por el mouse; si hay bolsa:
     - Si el player esta **dentro del radio** → `TryPickup` directo.
     - Si esta **lejos** → `playerMovement.MoveTo(drop.position, drop.PickupRadius, () => drop.TryPickup(player))` (se acerca y agarra al llegar).
     - Devuelve `true` (no ataca). Si no hay bolsa bajo el mouse → ataque normal (y `CancelMoveTo`).
5. **`MobDropHandler.RollDrops`** — ahora hace `WorldDrop.Spawn(itemId, count, affixes, mobPosition + offset aleatorio ±0.6)`. Ya no agrega directo al inventario.
6. **Input — accion `Interact` (espacio)**:
   - `Assets/InputSystem_Actions.inputactions` — nueva accion `Interact` (Button) en el mapa `Player`, binding `<Keyboard>/space`.
   - `PlayerInputs` — suscribe `_input.Player.FindAction("Interact")` (lookup runtime: compila aunque Unity todavia no regenere la clase; la regeneracion la hace el editor al importar el `.inputactions`). En `OnInteract` → `DropRegistry.FindNearestPickupable(_player)` → `TryPickup`.
7. **Nombre + color por rarity encima de la bolsa** — `Assets/Scripts/UI/Drop/ItemNameLabelManager.cs` (estatico) + `ItemNameLabel.cs`:
   - Al spawnear, `WorldDrop.Initialize` llama `ItemNameLabelManager.Show(this, GetDisplayName(itemId))` — el label es un `TextMeshProUGUI` creado en runtime sobre el primer **canvas overlay** de la escena (mismo patron que `DamageNumberManager`).
   - `ItemNameLabel.LateUpdate` posiciona el texto con `Camera.WorldToScreenPoint(drop.position + (0,1.15,0))` (patron `EnemyHealthBarUI`); se oculta si la bolsa queda detras de la camara.
   - Color por rarity (`ItemNameLabelManager.ColorForRarity`, derivada de los afijos via `WorldDrop.GetRarity()`): **Comun** gris, **Poco comun** verde, **Raro** azul, **Epico** violeta, **Legendario** naranja. Las pociones salen grises (sin afijos).
   - Al recoger/destruir la bolsa, `OnDisable` llama `ItemNameLabelManager.Hide(this)`.
8. **Tester** — `F4` genera una bolsa de prueba (`health_potion` x2) a 2m frente al player.

### Verificacion
- `F4` → bolsa marron aparece girando/flotando; se puede **pasar a traves** de ella (no bloquea); con **espacio** adentro del radio (1.5m) se agarra; **click** la agarra si estas cerca, o te acercas solo y la agarra al llegar.
- El nombre del item aparece **encima de la bolsa** con color segun rarity (gris/verde/azul/violeta/naranja); al recogerla desaparece.
- Matar mobs → bolsas en el mundo; recoger con click/espacio → `;` confirma que llego al inventario (con afijos si es equipo).
- Recoger 2 items iguales → aparecen como **2 stacks separados** (cada uno con su `instanceId`).
- Si atacas un mob (click sobre mob), no hay bolsa → ataca normal.

### Pendiente menor
- Prefab 3D real de la bolsa (hoy placeholder procedural).
- Brillo/outline al pasar el mouse sobre la bolsa (UX, futuro).

---

## 6. Fase 8 — UI del inventario (PENDIENTE)

### Objetivo
Panel que lista los stacks del inventario, permite **equipar items de equipo** (conecta con `PlayerEquipment`) y **usar consumibles** (conecta con `UseConsumable`).

### Puntos de diseño
1. **Input** — nueva accion `Inventory` (tecla `I`) en `Assets/InputSystem_Actions.inputactions` (mapa `Player`, hoy no existe). Handler en la UI (patron de `PauseMenu`, que tiene su propio `InputSystem_Actions`).
2. **Panel** — `InventoryPanel` (Canvas, patron de `PauseMenu`: `CanvasGroup` + fade + `Time.timeScale = 0` al abrir, desactivar `PlayerInputs`). Solo se puede abrir fuera de menues abiertos.
3. **Slot** — `InventorySlotUI` (patron de `SkillSlotUI`): icono (`Item.icon`, hoy vacio), nombre (via `displayNameKey` + `LocalizationConfig`), cantidad (`x{count}`), tooltip con stats/rarity.
4. **Grid** — panel de slots generados al abrir desde `Inventory.Stacks`; escuchar `OnInventoryChanged`.
5. **Click en slot:**
   - `ItemType.Equipment` → `player.Equipment.Equip(itemId)` (si cumple nivel); si el slot de destino ya tiene item, intercambiar (equipar el clickeado y mandar el anterior al inventario — o simplemente equipar y perder/stackear el anterior: **decidir**).
   - `ItemType.Consumable` → `player.Equipment.UseConsumable(itemId)` (resta 1; el slot desaparece si llega a 0).
   - Desequipar: hacer click en un slot de equipo del `EquipmentPanel` (UI del equipment, hoy no existe) para mandarlo al inventario.
6. **Separacion de responsabilidades** — `InventoryUI` (logica de abrir/cerrar/click) + `InventorySlotUI` (presentacion de un stack) + el modelo ya existente `PlayerInventory`.
7. **`EquipmentPanel`** — segunda parte de la UI: muestra los 7 slots de `PlayerEquipment`; click en un slot ocupado → desequipar (vuelve al inventario). Conecta visualmente inventario ↔ equipment.

### Verificacion
- Abrir con `I` → ver los stacks.
- Click en una poción → se usa, resta cantidad, HP sube.
- Click en un item de equipo → se equipa (pieza visual + stats en `=`), y desequipar lo devuelve al inventario.
- Cerrar pausa el juego; reabrir no duplica slots.

---

## 7. Orden de implementacion sugerido

1. ~~**Fase 6 — Drops**~~ — **HECHO** (2026-08-08).
2. ~~**Fase 7 — Afijos**~~ — **HECHO** (2026-08-08).
3. ~~**Fase 6b — Drops en el mundo (bolsa)**~~ — **HECHO** (2026-08-08).
4. **Fase 8 — UI** (depende de inventario + equipment; mas facil con los drops ya generando contenido).

> La UI (fase 8) era el pedido original del usuario ("UI que conecta el inventario con el equipment"), pero se dejo para despues de drops y afijos porque la UI va a necesitar resolver items con afijos y mostrar drops.

---

## 8. Archivos relevantes

- `Assets/Scripts/ClassJson/Items.cs` — `Item`, `ItemStack` (con `affixes`), `ItemAffix`, `AffixesConfig`, `ItemAffixConfig`, `EquippedAffixData`, enums.
- `Assets/Scripts/ClassJson/Game.cs` — `GameConfig.affixes` (path del config).
- `Assets/Scripts/ConfigBoostrap.cs` — `AffixesConfig` cargado.
- `Assets/Scripts/ClassJson/Enemies.cs` — `Enemy`, `DropTableConfig`, `ItemDrop`.
- `Assets/Scripts/Items/AffixService.cs` — roll aleatorio + aplicacion de afijos a stats.
- `Assets/Scripts/Items/PlayerInventory.cs` — modelo del inventario, agrupa por `itemId + affixKey`.
- `Assets/Scripts/Items/MobDropHandler.cs` — `RollDrops(Mob, Player)` genera afijos y spawnea `WorldDrop`.
- `Assets/Scripts/Items/WorldDrop.cs` — bolsa procedural + `DropRegistry` (raycast de click, nearest dentro de radio).
- `Assets/Scripts/Items/PlayerEquipment.cs` — `Equip(itemId, affixes)`, `TotalStats` con afijos, save/load de afijos.
- `Assets/Scripts/Player/Player.cs` — `Initialize` (orden: Resources → Equipment → Inventory → Skills), `RestoreInventory`.
- `Assets/Scripts/Player/PlayerMovement.cs` — `MoveTo`/`CancelMoveTo`/`HandleAutoMove` (auto-move a la bolsa).
- `Assets/Scripts/Player/PlayerCombat.cs` — `TryInteractWithDrop` (click sobre bolsa antes del ataque).
- `Assets/Scripts/Player/PlayerInputs.cs` — suscribe `Interact` (espacio) via `FindAction`.
- `Assets/Scripts/Save/PlayerSaveData.cs` + `GameSaveService.cs` — persistencia (`equippedItemAffixes`, `ItemStack.affixes`).
- `Assets/Scripts/Mob/Mob.cs` — `HandleDeath` (hook de drops), `EnemyConfig` accessor.
- `Assets/Scripts/Items/EquipmentDebugTester.cs` — teclas de debug (incluye `F4` bolsa de prueba).
- `Assets/InputSystem_Actions.inputactions` — accion `Interact` (espacio).
- `Assets/Assets/Resources/GameData/Config/enemies.json` — dropTables (zombie + skeleton).
- `Assets/Assets/Resources/GameData/Config/affixes.json` — pool de afijos (nuevo).
- `Assets/Assets/Resources/GameData/Config/game.json` — key `affixes` agregada.
- `Assets/Assets/Resources/GameData/Config/localization.json` — keys `affix.*`.
