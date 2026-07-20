# Sistema de Números de Daño en Pantalla

## Estado Implementado

- **Pool de 30 instancias** de TextMeshProUGUI reutilizables, creadas dinámicamente en `Awake()`
- **Screen Space Overlay** — posicionamiento con `Camera.WorldToScreenPoint()`
- **Animación coroutine**: float up + fade alpha + scale punch (1.5x → 1x en 0.1s)
- **Críticos soportados**: color rojo `rgb(255, 51, 51)`, escala 1.5x, texto con "!"
- **Outline negro** de 0.25 de ancho, habilitado con `OUTLINE_ON` keyword
- **Pool dinámico**: si se agotan las 30 instancias, crea más automáticamente
- **Config serializada en Inspector** del manager (no JSON)

---

## Flujo de Daño

```
Player ataca enemigo
  → PlayerAttackHitbox.OnTriggerEnter()
    → CombatService.Attack(player, mob, attack)
      → Mob.TakeDamage(damageData)
        → _resources.TakeDamage(damage, damageData.IsCritical)
          → Actualiza HP
          → DamageNumberManager.Show(position, damage, isCritical)
            → Obtener del pool
            → WorldToScreenPoint(position)
            → Activar animación (float + fade + punch)
            → Al terminar → devolver al pool
          → OnHealthChanged → EnemyHealthBarUI se actualiza
```

---

## Archivos Creados

| Archivo | Descripción |
|---------|-------------|
| `Assets/Scripts/UI/Damage/DamageNumberManager.cs` | Singleton + pool + Show() |
| `Assets/Scripts/UI/Damage/DamageNumberInstance.cs` | Wrapper con animación coroutine |

## Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/Mob/MobResources.cs` | `TakeDamage(int, bool)` — llama a `DamageNumberManager.Show()` |
| `Assets/Scripts/Mob/Mob.cs` | Pasa `damageData.IsCritical` a `_resources.TakeDamage()` |

## Archivos sin Cambio

| Archivo | Razón |
|---------|-------|
| `CombatService.cs` | El daño ya llega a MobResources con IsCritical en DamageData |
| `PlayerAttackHitbox.cs` | Ya llama a CombatService correctamente |
| `EnemyHealthBarUI.cs` | Sistema independiente |
| `PlayerResourcesUI.cs` | Solo muestra HP/MP del jugador |

---

## Config del Manager (Inspector)

| Campo | Tipo | Default | Descripción |
|-------|------|---------|-------------|
| `poolSize` | int | 30 | Cantidad inicial de instancias en pool |
| `floatSpeed` | float | 2 | Velocidad de movimiento hacia arriba |
| `floatDuration` | float | 0.8 | Segundos antes de desaparecer |
| `normalColor` | Color | White | Color para golpes normales |
| `criticalColor` | Color | rgb(255,51,51) | Color para críticos |
| `criticalScale` | float | 1.5 | Escala para críticos |

---

## Setup en Unity

1. Crear un GameObject hijo del Canvas "UI Mobs" (o cualquier Canvas Screen Space Overlay)
2. Agregar componente `DamageNumberManager`
3. No requiere configuración额外 — el pool se crea solo en `Awake()`

---

## Detalles Técnicos

- **Outline**: `outlineWidth = 0.25f`, `outlineColor = black`, habilitado con `tmp.fontMaterial.EnableKeyword("OUTLINE_ON")`
- **TextMeshPro**: fontSize 36, Bold, NoWrap, Overflow, center aligned
- **Pool**: lista `List<DamageNumberInstance>`, se expande dinámicamente si se agota
- **Animación**: `WorldToScreenPoint` se recalcula cada frame para mantener posición relativa al mob
- **Scale punch**: de 1.5x a 1x en los primeros 0.1s de la animación
- **Fade**: alpha de 1→0 lineal durante `floatDuration`

---

## Mejoras Futuras

| Mejora | Descripción |
|--------|-------------|
| Daño mágico | Color diferente para `DamageType.Magical` |
| Números de curación | Verde, en vez de rojo/blanco |
| Números sobre el jugador | Cuando los mobs lo golpean |
| Variación de posición | Offset X aleatorio para que no se superpongan |
