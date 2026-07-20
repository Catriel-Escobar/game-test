# Plan: Sistema de Números de Daño en Pantalla

## Estado Actual

- **Sin números de daño** — no existe ningún sistema de floating text ni damage popup
- El único feedback visual al hacer daño es la barra de vida del enemigo que se actualiza
- La UI actual usa **Screen Space Overlay** (canvas normal, no world space)
- `EnemyHealthBarUI` posiciona sus elementos con `Camera.WorldToScreenPoint()` en `LateUpdate()`
- `DamageData` ya trae: `FinalDamage`, `BaseDamage`, `IsCritical`, `DamageType`
- El flujo de daño es: `PlayerAttackHitbox` → `CombatService.Attack()` → `Mob.TakeDamage()` → `MobResources.TakeDamage()`

---

## Objetivo

Mostrar números de daño flotantes sobre los enemigos cuando reciben daño del jugador, con:
- Números que suben y se desvanecen (float up + fade out)
- Color diferenciado: blanco normal, rojo crítico
- Object pooling para no instanciar/destruir cada golpe
- Soporte para daño del jugador Y daño de mobs al jugador (futuro)

---

## Decisiones

| Decisión | Respuesta |
|----------|-----------|
| Dónde instanciar | **Screen Space** (misma canvas que health bars, posicionamiento con WorldToScreenPoint) |
| Pooling | **Sí** — pool de TextMeshProUGUI objects reutilizables |
| Dónde hookear | **MobResources.TakeDamage()** — es el punto central donde llega todo daño |
| Animación | **Coroutine** — move Y upward + fade alpha + scale punch |
| Críticos | **Color rojo** + tamaño más grande |
| Config | **Serializada en Inspector** del manager (no JSON, es puramente visual) |

---

## Cambios Propuestos

### 1. Nuevo Script: `DamageNumberManager.cs`

**Archivo**: `Assets/Scripts/UI/Damage/DamageNumberManager.cs`

Singleton que maneja el pool y spawning de números de daño.

```
Responsabilidades:
- Singleton Instance
- Pool de GameObjects con TextMeshProUGUI
- Método Show(position, damage, isCritical, damageType)
- Crear números bajo un Canvas parent
- Reutilizar objetos del pool en vez de instanciar/destruir
```

**Pseudocódigo**:
```csharp
public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }

    [SerializeField] private int poolSize = 30;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatDuration = 0.8f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.red;
    [SerializeField] private float criticalScale = 1.5f;

    private Canvas canvas;
    private RectTransform canvasRect;
    private Camera mainCamera;
    private List<DamageNumberInstance> pool = new();

    void Awake() // crear pool, buscar canvas

    public void Show(Vector3 worldPosition, int damage, bool isCritical)
    {
        // 1. Obtener objeto del pool (o crear si vacío)
        // 2. Calcular screen position con WorldToScreenPoint
        // 3. Setear texto (daño como string)
        // 4. Setear color (crítico = rojo, normal = blanco)
        // 5. Setear escala (crítico = 1.5x)
        // 6. Activar objeto
        // 7. Iniciar coroutine de animación (float up + fade)
    }
}
```

### 2. Nuevo Script: `DamageNumberInstance.cs`

**Archivo**: `Assets/Scripts/UI/Damage/DamageNumberInstance.cs`

Wrapper ligero para cada número del pool.

```
Responsabilidades:
- Referencia a TextMeshProUGUI
- Referencia a RectTransform
- Coroutine de animación
- Método Activate(worldPos) / Deactivate()
- Al terminar la animación, volver al pool
```

**Pseudocódigo**:
```csharp
public class DamageNumberInstance : MonoBehaviour
{
    private TextMeshProUGUI text;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    public void Setup(string text, Color color, float scale)
    {
        this.text.text = text;
        this.text.color = color;
        transform.localScale = Vector3.one * scale;
        canvasGroup.alpha = 1f;
    }

    public void Animate(Vector3 worldPos, float speed, float duration, Camera cam, System.Action onFinished)
    {
        // Coroutine:
        // 1. Posicionar en screen space con cam.WorldToScreenPoint(worldPos)
        // 2. Mover Y += speed * deltaTime durante duration
        // 3. Fade alpha de 1→0 durante duration
        // 4. Scale punch inicial (1.5x → 1x en 0.1s)
        // 5. Al terminar, onFinished() para devolver al pool
    }
}
```

### 3. Hook en `MobResources.cs`

**Archivo**: `Assets/Scripts/Mob/MobResources.cs`

Agregar en `TakeDamage()`, después de aplicar daño:

```csharp
public void TakeDamage(int damage)
{
    if (IsDead) return;

    CurrentHp = Mathf.Max(CurrentHp - damage, 0);
    OnHealthChanged?.Invoke(CurrentHp, MaxHp);

    // Mostrar número de daño
    if (DamageNumberManager.Instance != null)
    {
        DamageNumberManager.Instance.Show(
            transform.position + Vector3.up * 2f,  // offset Y sobre el mob
            damage,
            false  // isCritical se pasará desde Mob.TakeDamage en futuro
        );
    }

    if (CurrentHp <= 0) Die();
    else OnHit?.Invoke();
}
```

**Nota**: Para soportar críticos, `Mob.TakeDamage(DamageData)` debería pasar `damageData.IsCritical` a través de un evento o parámetro adicional. Esto se puede hacer en una segunda iteración.

### 4. Nuevo GameObject en escena: `DamageNumberCanvas`

**Archivo**: `Assets/OutdoorsScene.unity`

Crear un Canvas Screen Space Overlay (o reutilizar el existente "UI Mobs"):
- Agregar componente `DamageNumberManager`
- Como hijo, un empty RectTransform como container del pool

**Alternativa más limpia**: Agregar `DamageNumberManager` al mismo Canvas "UI Mobs" que ya tiene `EnemyHealthBarManager`.

### 5. Integración con `GameBootstrap.cs`

**Archivo**: `Assets/Scripts/GameBoostrap.cs`

No es estrictamente necesario si el `DamageNumberManager` se coloca en la escena y se auto-registra en `Awake()`. Pero si se quiere crear dinámicamente:

```csharp
// En Awake(), después de crear SpawnerManager:
if (FindObjectOfType<DamageNumberManager>() == null)
{
    // Crear canvas y manager
}
```

---

## Flujo de Daño (con números)

```
Player ataca enemigo
  → PlayerAttackHitbox.OnTriggerEnter()
    → CombatService.Attack(player, mob, attack)
      → Mob.TakeDamage(damageData)
        → MobResources.TakeDamage(damage)
          → Actualiza HP
          → DamageNumberManager.Show(position, damage, isCritical)
            → Obtener del pool
            → WorldToScreenPoint(position)
            → Activar animación (float + fade)
            → Al terminar → devolver al pool
          → OnHealthChanged → EnemyHealthBarUI se actualiza
```

---

## Archivos a Crear

| Archivo | Descripción |
|---------|-------------|
| `Assets/Scripts/UI/Damage/DamageNumberManager.cs` | Singleton manager + pool |
| `Assets/Scripts/UI/Damage/DamageNumberInstance.cs` | Wrapper animación por número |

## Archivos a Modificar

| Archivo | Cambio |
|---------|--------|
| `Assets/Scripts/Mob/MobResources.cs` | Llamar a `DamageNumberManager.Show()` en `TakeDamage()` |
| `Assets/OutdoorsScene.unity` | Agregar `DamageNumberManager` al Canvas "UI Mobs" (o crear canvas nuevo) |

## Archivos sin Cambio

| Archivo | Razón |
|---------|-------|
| `CombatService.cs` | No se toca — el daño ya llega a MobResources |
| `PlayerAttackHitbox.cs` | No se toca — ya llama a CombatService correctamente |
| `EnemyHealthBarUI.cs` | No se toca — sistema independiente |
| `PlayerResourcesUI.cs` | No se toca — solo muestra HP/MP del jugador |

---

## Orden de Implementación

1. **Crear `DamageNumberInstance.cs`** — wrapper con animación
2. **Crear `DamageNumberManager.cs`** — singleton + pool + Show()
3. **Modificar `MobResources.cs`** — llamar a Show() en TakeDamage()
4. **Configurar en escena** — agregar manager al Canvas
5. **Probar** — golpear un enemigo y ver los números flotar

---

## Mejoras Futuras (no en este plan)

| Mejora | Descripción |
|--------|-------------|
| Soporte críticos | Pasar `DamageData.IsCritical` desde `Mob.TakeDamage()` |
| Daño mágico | Color diferente para `DamageType.Magical` |
| Números de curación | Verde, en vez de rojo/blanco |
| Números sobre el jugador | Cuando los mobs lo golpean |
| Pool dinámico | Expandir pool si se quedan sin objetos |
| Variación de posición | Offset X aleatorio para que no se superpongan |
