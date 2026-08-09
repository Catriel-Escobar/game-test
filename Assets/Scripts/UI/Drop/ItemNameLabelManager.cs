using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class ItemNameLabelManager
{
    private static readonly Dictionary<WorldDrop, ItemNameLabel> _labels = new Dictionary<WorldDrop, ItemNameLabel>();
    private static Canvas _cachedCanvas;
    private static Camera _cachedCamera;
    private static TMP_FontAsset _cachedFont;

    private static readonly Color CommonColor = new Color(0.85f, 0.85f, 0.85f);
    private static readonly Color UncommonColor = new Color(0.2f, 0.9f, 0.35f);
    private static readonly Color RareColor = new Color(0.3f, 0.55f, 1f);
    private static readonly Color EpicColor = new Color(0.65f, 0.35f, 1f);
    private static readonly Color LegendaryColor = new Color(1f, 0.6f, 0.1f);

    public static void Show(WorldDrop drop, string itemName)
    {
        if (drop == null || string.IsNullOrEmpty(itemName)) return;

        if (_labels.ContainsKey(drop)) return;

        ItemNameLabel label = CreateLabel(drop, itemName, ColorForRarity(drop.GetRarity()));
        if (label != null)
            _labels[drop] = label;
    }

    public static void Hide(WorldDrop drop)
    {
        if (drop == null || !_labels.TryGetValue(drop, out ItemNameLabel label)) return;

        _labels.Remove(drop);
        if (label != null)
            Object.Destroy(label.gameObject);
    }

    public static Color ColorForRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return CommonColor;
            case ItemRarity.Uncommon: return UncommonColor;
            case ItemRarity.Rare: return RareColor;
            case ItemRarity.Epic: return EpicColor;
            case ItemRarity.Legendary: return LegendaryColor;
            default: return CommonColor;
        }
    }

    private static ItemNameLabel CreateLabel(WorldDrop drop, string text, Color color)
    {
        RectTransform parent = DropVisualConfig.ItemNameLabelContainer;
        if (parent == null)
        {
            Canvas canvas = GetCanvas();
            if (canvas == null) return null;
            parent = (RectTransform)canvas.transform;
        }

        GameObject obj;
        if (DropVisualConfig.ItemNameLabelPrefab != null)
        {
            obj = Object.Instantiate(DropVisualConfig.ItemNameLabelPrefab, parent, false);
            if (obj.GetComponentInChildren<TMP_Text>() == null)
            {
                Object.Destroy(obj);
                return null;
            }
        }
        else
        {
            obj = CreateProceduralLabel(parent);
        }

        ItemNameLabel label = obj.GetComponent<ItemNameLabel>();
        if (label == null)
            label = obj.AddComponent<ItemNameLabel>();
        label.Initialize(drop, text, color);
        return label;
    }

    private static GameObject CreateProceduralLabel(RectTransform parent)
    {
        GameObject obj = new GameObject("ItemNameLabel");
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(240f, 40f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = GetFont();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.outlineWidth = 0.35f;
        tmp.outlineColor = new Color32(0, 0, 0, 255);
        tmp.fontMaterial.EnableKeyword("OUTLINE_ON");

        return obj;
    }

    private static Canvas GetCanvas()
    {
        if (_cachedCanvas != null && IsVisible(_cachedCanvas))
            return _cachedCanvas;

        _cachedCanvas = null;
        Canvas[] all = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].renderMode == RenderMode.ScreenSpaceOverlay && IsVisible(all[i]))
            {
                _cachedCanvas = all[i];
                break;
            }
        }

        return _cachedCanvas;
    }

    private static bool IsVisible(Canvas canvas)
    {
        if (canvas == null || !canvas.gameObject.activeInHierarchy) return false;
        CanvasGroup group = canvas.GetComponent<CanvasGroup>();
        return group == null || group.alpha > 0f;
    }

    private static TMP_FontAsset GetFont()
    {
        if (_cachedFont != null) return _cachedFont;

        TextMeshProUGUI[] texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].font != null
                && texts[i].GetComponentInParent<ItemNameLabel>() == null)
            {
                _cachedFont = texts[i].font;
                break;
            }
        }

        return _cachedFont;
    }
}
