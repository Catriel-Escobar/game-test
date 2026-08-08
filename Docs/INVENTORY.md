# Spec: Sistema de Inventario

> Especificacion del inventario del player: modelo de datos, persistencia y las fases pendientes (drops, afijos/affixes y UI que conecta inventario con equipment). Actualizado: 2026-08-08.

---

## 1. Objetivo

El player posee items (no solo los que tiene equipados), organizados en stacks por `itemId`. Los items se obtienen por drops de mobs (pendiente), pueden llevar afijos que modifiquen sus stats (pendiente), y se ven/gestionan desde una UI que los conecta con el sistema de equipment ya existente (pendiente).

**Estado actual:** el modelo de datos + persistencia + consumo desde inventario estan **implementados y probados**. Las 3 fases siguientes (drops, afijos, UI) estan **pendientes**.

---

## 2. Estado actual — IMPLEMENTADO

### 2.1 Modelo de datos

- `Assets/Scripts/ClassJson/Items.cs` — nuevo `ItemStack`:
  ```csharp
  [Serializable] public class ItemStack
  {
      public string itemId;  // id del item en items.json
      public int count;      // cantidad en el stack
  }
  ```

### 2.2 `Assets/Scripts/Items/PlayerInventory.cs`

`MonoBehaviour` (se auto-agrega al Player si falta). Lista de `ItemStack` (`List<ItemStack>`), sin cap max por ahora.

**API publica:**
- `AddItem(string itemId, int count = 1)` — suma a un stack existente o crea uno nuevo; dispara `OnInventoryChanged`.
- `RemoveItem(string itemId, int count = 1) : bool` — resta; si llega a 0 elimina el stack; dispara evento.
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

## 3. Fase 6 — Drops de mobs (PENDIENTE)

### Objetivo
Al morir un mob, dropear items al inventario del player que le dio el golpe final (o el que tiene mas aggro — decidir).

### Puntos de diseño
1. **Config en `enemies.json`** — nueva seccion `dropTable` por enemigo:
   ```json
   "dropTable": {
       "goldChance": 1.0,
       "itemDrops": [
           { "itemId": "health_potion", "chance": 0.5, "minCount": 1, "maxCount": 1 },
           { "itemId": "warrior_helmet_t1", "chance": 0.1, "minCount": 1, "maxCount": 1 },
           { "itemId": "warrior_sword_t2", "chance": 0.02, "minCount": 1, "maxCount": 1 }
       ]
   }
   ```
   - `Enemy` (ClassJson/Enemies.cs): agregar `DropTableConfig dropTable`.
2. **Clases de config** en `Enemies.cs` (o archivo nuevo):
   ```csharp
   [Serializable] public class DropTableConfig { public float goldChance; public ItemDrop[] itemDrops; }
   [Serializable] public class ItemDrop { public string itemId; public float chance; public int minCount; public int maxCount; }
   ```
   > **Nota JsonUtility:** campos numericos faltantes quedan en 0; `chance` 0 = no dropea. `minCount/maxCount` 0 = 1.
3. **`MobDropHandler`** (o metodo en `Mob.HandleDeath`):
   - Para cada `itemDrop`, `Random.value <= chance` → `count = Random.Range(minCount, maxCount+1)` → `player.Inventory.AddItem(itemId, count)`.
   - El `player` destino: el `_lastAttacker` (ya se usa para XP en `Mob.HandleDeath:238`).
   - Log por consola: `[Drops] zombie dropeo health_potion x1` (patron de debug actual).
   - Opcional: notificacion flotante/UI (futuro).
4. **Hook de muerte:** en `Mob.HandleDeath()` (`Assets/Scripts/Mob/Mob.cs:227`), junto a `AddExperience`, llamar al handler de drops con `_lastAttacker as Player`.

### Verificacion
- Matar mobs (ya spawnen en la escena) → ver en consola que dropean.
- `;` en el tester para confirmar que los items llegan al inventario.
- Guardar/recargar → los drops persisten.

---

## 4. Fase 7 — Afijos / Affixes (PENDIENTE)

### Objetivo
Items con modificadores extra aleatorios (ej. +5 FUE, +10 armor, +20 HP) que se generan en el drop o se agregan manualmente.

### Puntos de diseño
1. **Modelo** — nuevos tipos en `Items.cs`:
   ```csharp
   [Serializable] public class ItemAffix
   {
       public string affixId;   // ej: "strength_plus"
       public int value;        // ej: 5
       public float percent;    // ej: 0.1 (10% armor) — 0 si no aplica
   }

   [Serializable] public class ItemAffixConfig
   {
       public string id;
       public string displayNameKey; // ej: "De la Fuerza"
       public string stat;           // stat que modifica (strength, vitality, intelligence, dexterity, armor, damage, health, mana, critChance, critDamage...)
       public bool isPercent;        // true = porcentual
       public int tierCount;         // 0 = valor fijo; 1..N = numero de valores posibles
   }
   ```
2. **Datos** — nuevo `affixes.json` en `GameData/Config` con los afijos disponibles; `ItemsConfig` (o config nueva) los carga.
3. **En `Item`** — campo opcional `ItemAffix[] affixes`. Un item con afijos tiene stats que son **base + afijos**.
4. **Generacion aleatoria** — funcion (en `ItemFactory` o `AffixService`): dado un item base y un `affixCount` aleatorio (ej. 0-2 segun rareza), aplicar afijos de un pool (validando que no se repita stat).
5. **Uso en drops** — el `MobDropHandler` de la fase 6 genera los items con afijos antes de agregarlos al inventario:
   ```
   AddItem con afijos -> instancia del item con stats calculadas
   ```
   > **Decidir:** como el inventario guarda solo `itemId`, un item con afijos no puede representarse solo con el id. Opciones:
   > a) Stacks por `itemId` siguen igual, y los afijos se guardan en un mapa aparte (complejo).
   > b) **`ItemStack` gana `itemId` + `affixHash`** (hash de afijos) — los stacks distintos se separan; el `Item` final se resuelve en runtime.
   > c) Items afijados NO se stackean y se guardan como instancia (rompe el modelo de stacks).
   > **Recomendado:** (b). Requiere cambiar `PlayerInventory` para agrupar por `itemId+affixHash` y que `UseConsumable`/UI resuelvan el item con afijos.
6. **Aplicacion a stats** — `PlayerEquipment.TotalStats` debe sumar base + afijos de cada item equipado. El item con afijos se resuelve via `FindItemById` + aplicar afijos.

### Verificacion
- Dar un item con afijos via consola → `=` muestra stats que incluyen los afijos.
- Drops con afijos → se ven distintos stats entre drops del mismo item.

---

## 5. Fase 8 — UI del inventario (PENDIENTE)

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

## 6. Orden de implementacion sugerido

1. **Fase 6 — Drops** (depende solo de `PlayerInventory.AddItem` y `Mob.HandleDeath`).
2. **Fase 7 — Afijos** (depende de drops; impacta el modelo de stacks → decidir antes si los afijos entran en `ItemStack`).
3. **Fase 8 — UI** (depende de inventario + equipment; mas facil con los drops ya generando contenido).

> La UI (fase 8) era el pedido original del usuario ("UI que conecta el inventario con el equipment"), pero se dejo para despues de drops y afijos porque la UI va a necesitar resolver items con afijos y mostrar drops.

---

## 7. Archivos relevantes

- `Assets/Scripts/ClassJson/Items.cs` — `Item`, `ItemStack`, enums (agregar `ItemAffix` aqui).
- `Assets/Scripts/ClassJson/Enemies.cs` — `Enemy` (agregar `dropTable` aqui).
- `Assets/Scripts/Items/PlayerInventory.cs` — modelo del inventario (cambiar agrupacion si se adopta `affixHash`).
- `Assets/Scripts/Items/PlayerEquipment.cs` — `Equip`, `UseConsumable`, `FindItemById`, `TotalStats` (sumar afijos).
- `Assets/Scripts/Player/Player.cs` — `Initialize` (orden: Resources → Equipment → Inventory → Skills), `RestoreInventory`.
- `Assets/Scripts/Save/PlayerSaveData.cs` + `GameSaveService.cs` — persistencia (agregar `affixHash` a `ItemStack` si aplica).
- `Assets/Scripts/Mob/Mob.cs` — `HandleDeath` (hook de drops).
- `Assets/Assets/Resources/GameData/Config/enemies.json` — dropTables.
- `Assets/Scripts/UI/Menu/PauseMenu.cs` — patron de panel (CanvasGroup + timeScale).
- `Assets/Scripts/UI/Skills/SkillSlotUI.cs` — patron de slot.
- `Assets/InputSystem_Actions.inputactions` — agregar accion `Inventory`.
- `Assets/Scripts/Items/EquipmentDebugTester.cs` — tester (`[`, `]`, `\`, `;`).
- `Docs/ITEMS.md` — spec del sistema de items base.
