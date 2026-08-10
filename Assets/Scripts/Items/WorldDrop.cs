using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldDrop : MonoBehaviour
{
    public string ItemId { get; private set; }
    public int Count { get; private set; }
    public ItemAffix[] Affixes { get; private set; }

    private float _pickupRadius = 1.5f;
    public float PickupRadius => _pickupRadius;

    private float _rotationSpeed = 45f;
    private float _floatHeight = 0.15f;
    private float _floatSpeed = 2f;

    private bool _pickedUp;
    private float _baseY;

    public static WorldDrop Spawn(string itemId, int count, ItemAffix[] affixes, Vector3 position)
    {
        GameObject root;
        if (DropVisualConfig.DropPrefab != null)
            root = UnityEngine.Object.Instantiate(DropVisualConfig.DropPrefab);
        else
            root = CreateBagVisual();

        root.transform.position = position;

        ForceTriggers(root);

        WorldDrop drop = root.GetComponent<WorldDrop>();
        if (drop == null)
            drop = root.AddComponent<WorldDrop>();
        drop.Initialize(itemId, count, affixes);
        return drop;
    }

    private static void ForceTriggers(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
                colliders[i].isTrigger = true;
        }
    }

    private void Initialize(string itemId, int count, ItemAffix[] affixes)
    {
        ItemId = itemId;
        Count = Mathf.Max(1, count);
        Affixes = CloneAffixes(affixes);
        ApplyConfig();
        _baseY = transform.position.y;

        ItemNameLabelManager.Show(this, GetDisplayName(itemId));
    }

    private void ApplyConfig()
    {
        DropConfig config = ConfigBoostrap.Current?.DropConfig;
        if (config == null) return;

        _pickupRadius = config.pickupRadius;
        _rotationSpeed = config.rotationSpeed;
        _floatHeight = config.floatHeight;
        _floatSpeed = config.floatSpeed;
    }

    private static string GetDisplayName(string itemId)
    {
        Item item = ConfigBoostrap.Current?.ItemsConfig != null ? FindItemById(itemId, ConfigBoostrap.Current.ItemsConfig.items) : null;
        if (item == null || string.IsNullOrEmpty(item.displayNameKey)) return itemId;

        string name = ConfigBoostrap.Current?.LocalizationConfig?.Get(item.displayNameKey);
        return string.IsNullOrEmpty(name) ? itemId : name;
    }

    private static Item FindItemById(string itemId, Item[] items)
    {
        if (items == null) return null;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].id == itemId)
                return items[i];
        }

        return null;
    }

    public ItemRarity GetRarity()
    {
        return AffixService.RarityForAffixCount(Affixes != null ? Affixes.Length : 0);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);

        Vector3 position = transform.position;
        position.y = _baseY + Mathf.Sin(Time.time * _floatSpeed) * _floatHeight;
        transform.position = position;
    }

    private void OnEnable()
    {
        DropRegistry.Register(this);
    }

    private void OnDisable()
    {
        ItemNameLabelManager.Hide(this);
        DropRegistry.Unregister(this);
    }

    public bool TryPickup(Player player)
    {
        if (_pickedUp) return false;
        if (player == null || player.Inventory == null) return false;

        Item item = player.Equipment != null ? player.Equipment.FindItemById(ItemId) : null;
        if (item == null)
        {
            Debug.LogWarning($"[WorldDrop] Item '{ItemId}' no existe — drop no recogible.");
            return false;
        }

        if (!player.Inventory.AddItem(ItemId, Count, Affixes))
        {
            Debug.Log($"[WorldDrop] Inventario lleno: no se puede recoger '{ItemId}'.");
            return false;
        }
        _pickedUp = true;

        string affixText = Affixes != null && Affixes.Length > 0 ? $" con {Affixes.Length} afijos" : "";
        Debug.Log($"[WorldDrop] Recogida bolsa con '{ItemId}' x{Count}{affixText}");
        Destroy(gameObject);
        return true;
    }

    private static GameObject CreateBagVisual()
    {
        GameObject root = new GameObject("WorldDrop");

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "BagBody";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.28f, 0f);
        body.transform.localScale = new Vector3(0.6f, 0.45f, 0.42f);
        SetMaterialColor(body, new Color(0.55f, 0.34f, 0.16f));
        SetTrigger(body);

        GameObject knot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knot.name = "BagKnot";
        knot.transform.SetParent(root.transform, false);
        knot.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        knot.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        SetMaterialColor(knot, new Color(0.35f, 0.2f, 0.09f));
        SetTrigger(knot);

        return root;
    }

    private static void SetTrigger(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = true;
    }

    private static void SetMaterialColor(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = color;
    }

    private static ItemAffix[] CloneAffixes(ItemAffix[] affixes)
    {
        if (affixes == null || affixes.Length == 0) return null;
        ItemAffix[] clone = new ItemAffix[affixes.Length];
        for (int i = 0; i < affixes.Length; i++)
        {
            ItemAffix a = affixes[i];
            clone[i] = a != null ? new ItemAffix(a.stat, a.value, a.percent) : null;
        }

        return clone;
    }
}

public static class DropRegistry
{
    private static readonly List<WorldDrop> Drops = new List<WorldDrop>();

    public static int Count => Drops.Count;

    public static void Register(WorldDrop drop)
    {
        if (drop == null || Drops.Contains(drop)) return;
        Drops.Add(drop);
    }

    public static void Unregister(WorldDrop drop)
    {
        if (drop != null) Drops.Remove(drop);
    }

    public static WorldDrop FindDropAtScreenPoint(Vector2 screenPoint)
    {
        Camera camera = Camera.main;
        if (camera == null) return null;

        Ray ray = camera.ScreenPointToRay(screenPoint);
        RaycastHit[] hits = Physics.RaycastAll(ray, 200f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            WorldDrop drop = hits[i].collider.GetComponentInParent<WorldDrop>();
            if (drop != null) return drop;
        }

        return null;
    }

    public static WorldDrop FindNearestPickupable(Player player)
    {
        if (player == null) return null;

        Vector3 playerPosition = player.transform.position;
        WorldDrop best = null;
        float bestSqrDistance = float.MaxValue;

        for (int i = 0; i < Drops.Count; i++)
        {
            WorldDrop drop = Drops[i];
            if (drop == null) continue;

            float sqrDistance = (drop.transform.position - playerPosition).sqrMagnitude;
            if (sqrDistance > drop.PickupRadius * drop.PickupRadius) continue;
            if (sqrDistance < bestSqrDistance)
            {
                bestSqrDistance = sqrDistance;
                best = drop;
            }
        }

        return best;
    }
}
