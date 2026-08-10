using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class InventoryUIBuilder
{
    private const string PrefabDir = "Assets/Assets/Resources/Prefabs/UI/Items";

    [MenuItem("Tools/Inventory UI/Build All Prefabs")]
    public static void BuildAll()
    {
        BuildTooltip();
        BuildInventorySlot();
        BuildEquipmentSlot();
        BuildEquipmentPanel();
        BuildInventoryPanel();
        AssetDatabase.SaveAssets();
        Debug.Log("[InventoryUIBuilder] Prefabs generados.");
    }

    [MenuItem("Tools/Inventory UI/Build Tooltip")]
    public static void BuildTooltip()
    {
        EnsureFolder();

        GameObject root = new GameObject("ItemTooltip");
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 120f);

        CanvasGroup group = root.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);

        GameObject textObj = CreateTextChild(root.transform, "Content", new Vector2(220f, 110f));
        textObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        TMP_Text text = textObj.GetComponent<TMP_Text>();
        text.fontSize = 14f;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.enableWordWrapping = true;
        text.color = Color.white;

        ItemTooltipUI tooltip = root.AddComponent<ItemTooltipUI>();

        SavePrefab(root, "ItemTooltip.prefab");
    }

    [MenuItem("Tools/Inventory UI/Build Inventory Slot")]
    public static void BuildInventorySlot()
    {
        EnsureFolder();

        GameObject root = new GameObject("InventorySlot");
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(64f, 64f);

        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);

        Button button = root.AddComponent<Button>();
        button.targetGraphic = bg;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.25f);
        button.colors = colors;

        GameObject iconObj = CreateImageChild(root.transform, "Icon", new Vector2(48f, 48f));
        iconObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        GameObject countObj = CreateTextChild(root.transform, "Count", new Vector2(60f, 20f));
        countObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(2f, -20f);
        TMP_Text count = countObj.GetComponent<TMP_Text>();
        count.fontSize = 12f;
        count.alignment = TextAlignmentOptions.BottomRight;
        count.color = Color.white;

        GameObject borderObj = CreateImageChild(root.transform, "RarityBorder", new Vector2(64f, 64f));
        borderObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        borderObj.GetComponent<Image>().type = Image.Type.Sliced;

        InventorySlotUI slot = root.AddComponent<InventorySlotUI>();

        SerializedObject so = new SerializedObject(slot);
        so.FindProperty("_icon").objectReferenceValue = iconObj.GetComponent<Image>();
        so.FindProperty("_countText").objectReferenceValue = count;
        so.FindProperty("_rarityBorder").objectReferenceValue = borderObj.GetComponent<Image>();
        so.ApplyModifiedPropertiesWithoutUndo();

        SavePrefab(root, "InventorySlot.prefab");
    }

    [MenuItem("Tools/Inventory UI/Build Equipment Slot")]
    public static void BuildEquipmentSlot()
    {
        EnsureFolder();

        GameObject root = new GameObject("EquipmentSlot");
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(64f, 64f);

        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);

        Button button = root.AddComponent<Button>();
        button.targetGraphic = bg;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.25f);
        button.colors = colors;

        GameObject iconObj = CreateImageChild(root.transform, "Icon", new Vector2(48f, 48f));
        iconObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        GameObject emptyOverlay = CreateImageChild(root.transform, "Empty", new Vector2(64f, 64f));
        emptyOverlay.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        emptyOverlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        GameObject labelObj = CreateTextChild(root.transform, "Label", new Vector2(64f, 20f));
        labelObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -34f);
        TMP_Text label = labelObj.GetComponent<TMP_Text>();
        label.fontSize = 9f;
        label.alignment = TextAlignmentOptions.Bottom;
        label.color = new Color(1f, 1f, 1f, 0.7f);

        GameObject borderObj = CreateImageChild(root.transform, "RarityBorder", new Vector2(64f, 64f));
        borderObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        borderObj.GetComponent<Image>().type = Image.Type.Sliced;

        EquipmentSlotUI slot = root.AddComponent<EquipmentSlotUI>();

        SerializedObject so = new SerializedObject(slot);
        so.FindProperty("_icon").objectReferenceValue = iconObj.GetComponent<Image>();
        so.FindProperty("_slotLabel").objectReferenceValue = label;
        so.FindProperty("_rarityBorder").objectReferenceValue = borderObj.GetComponent<Image>();
        so.FindProperty("_emptyOverlay").objectReferenceValue = emptyOverlay.GetComponent<Image>();
        so.ApplyModifiedPropertiesWithoutUndo();

        SavePrefab(root, "EquipmentSlot.prefab");
    }

    [MenuItem("Tools/Inventory UI/Build Inventory Panel")]
    public static void BuildInventoryPanel()
    {
        EnsureFolder();

        GameObject root = new GameObject("InventoryPanel");
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400f, 500f);

        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.14f, 0.95f);

        GameObject titleObj = CreateTextChild(root.transform, "Title", new Vector2(360f, 30f));
        titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 230f);
        TMP_Text title = titleObj.GetComponent<TMP_Text>();
        title.fontSize = 20f;
        title.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject closeObj = CreateTextChild(root.transform, "CloseButton", new Vector2(30f, 30f));
        closeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(175f, 230f);
        TMP_Text closeText = closeObj.GetComponent<TMP_Text>();
        closeText.text = "X";
        closeText.fontSize = 18f;
        closeText.alignment = TextAlignmentOptions.Center;
        Button closeBtn = closeObj.AddComponent<Button>();

        GameObject containerObj = new GameObject("Slots");
        RectTransform container = containerObj.AddComponent<RectTransform>();
        container.SetParent(root.transform, false);
        container.anchorMin = new Vector2(0.5f, 1f);
        container.anchorMax = new Vector2(0.5f, 1f);
        container.pivot = new Vector2(0.5f, 1f);
        container.sizeDelta = new Vector2(360f, 400f);
        container.anchoredPosition = new Vector2(0f, -40f);

        GridLayoutGroup grid = containerObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(64f, 64f);
        grid.spacing = new Vector2(8f, 8f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        InventorySlotUI slotPrefab = AssetDatabase.LoadAssetAtPath<InventorySlotUI>($"{PrefabDir}/InventorySlot.prefab");
        ItemTooltipUI tooltipPrefab = AssetDatabase.LoadAssetAtPath<ItemTooltipUI>($"{PrefabDir}/ItemTooltip.prefab");

        ItemTooltipUI tooltipInstance = (ItemTooltipUI)PrefabUtility.InstantiatePrefab(tooltipPrefab);
        tooltipInstance.name = "Tooltip";
        tooltipInstance.transform.SetParent(root.transform, false);
        tooltipInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        InventoryUI inventoryUI = root.AddComponent<InventoryUI>();

        SerializedObject so = new SerializedObject(inventoryUI);
        so.FindProperty("_panel").objectReferenceValue = root;
        so.FindProperty("_slotContainer").objectReferenceValue = containerObj.transform;
        so.FindProperty("_slotPrefab").objectReferenceValue = slotPrefab;
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
        so.FindProperty("_titleText").objectReferenceValue = title;
        so.FindProperty("_tooltip").objectReferenceValue = tooltipInstance;
        so.ApplyModifiedPropertiesWithoutUndo();

        SavePrefab(root, "InventoryPanel.prefab");
    }

    [MenuItem("Tools/Inventory UI/Build Equipment Panel")]
    public static void BuildEquipmentPanel()
    {
        EnsureFolder();

        GameObject root = new GameObject("EquipmentPanel");
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(260f, 500f);

        Image bg = root.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.14f, 0.95f);

        GameObject titleObj = CreateTextChild(root.transform, "Title", new Vector2(240f, 30f));
        titleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 230f);
        TMP_Text title = titleObj.GetComponent<TMP_Text>();
        title.fontSize = 20f;
        title.alignment = TextAlignmentOptions.Midline;

        GameObject statsObj = CreateTextChild(root.transform, "TotalStats", new Vector2(240f, 130f));
        statsObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -180f);
        TMP_Text stats = statsObj.GetComponent<TMP_Text>();
        stats.fontSize = 13f;
        stats.alignment = TextAlignmentOptions.MidlineLeft;

        ItemTooltipUI tooltipPrefab = AssetDatabase.LoadAssetAtPath<ItemTooltipUI>($"{PrefabDir}/ItemTooltip.prefab");
        EquipmentSlotUI slotPrefab = AssetDatabase.LoadAssetAtPath<EquipmentSlotUI>($"{PrefabDir}/EquipmentSlot.prefab");

        ItemTooltipUI tooltipInstance = (ItemTooltipUI)PrefabUtility.InstantiatePrefab(tooltipPrefab);
        tooltipInstance.name = "Tooltip";
        tooltipInstance.transform.SetParent(root.transform, false);
        tooltipInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        EquipmentSlotUI[] slots = new EquipmentSlotUI[7];
        Vector2[] positions =
        {
            new Vector2(0f, 160f),    // Helmet
            new Vector2(0f, 80f),     // Chest
            new Vector2(-90f, 20f),   // Gloves
            new Vector2(-60f, -80f),  // Boots
            new Vector2(90f, 20f),    // Cape
            new Vector2(-110f, 120f), // Weapon
            new Vector2(110f, 120f)   // OffHand
        };

        for (int i = 0; i < slots.Length; i++)
        {
            GameObject slotObj = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab);
            slotObj.name = $"Slot_{i}";
            slotObj.transform.SetParent(root.transform, false);
            RectTransform slotRect = slotObj.GetComponent<RectTransform>();
            slotRect.anchoredPosition = positions[i];
            slotRect.anchorMin = new Vector2(0.5f, 0.5f);
            slotRect.anchorMax = new Vector2(0.5f, 0.5f);
            slots[i] = slotObj.GetComponent<EquipmentSlotUI>();
        }

        EquipmentUI equipmentUI = root.AddComponent<EquipmentUI>();

        SerializedObject so = new SerializedObject(equipmentUI);
        so.FindProperty("_panel").objectReferenceValue = root;
        so.FindProperty("_slots").arraySize = 7;
        for (int i = 0; i < slots.Length; i++)
            so.FindProperty("_slots").GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
        so.FindProperty("_titleText").objectReferenceValue = title;
        so.FindProperty("_totalStatsText").objectReferenceValue = stats;
        so.FindProperty("_tooltip").objectReferenceValue = tooltipInstance;
        so.ApplyModifiedPropertiesWithoutUndo();

        SavePrefab(root, "EquipmentPanel.prefab");
    }

    private static GameObject CreateTextChild(Transform parent, string name, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = size;

        TMP_Text text = obj.AddComponent<TextMeshProUGUI>();
        text.fontSize = 14f;
        text.color = Color.white;
        text.enableWordWrapping = false;
        return obj;
    }

    private static GameObject CreateImageChild(Transform parent, string name, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = size;

        Image image = obj.AddComponent<Image>();
        image.color = Color.white;
        image.raycastTarget = false;
        return obj;
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Assets/Resources/Prefabs/UI/Items"))
            AssetDatabase.CreateFolder("Assets/Assets/Resources/Prefabs/UI", "Items");
    }

    private static void SavePrefab(GameObject root, string fileName)
    {
        string path = $"{PrefabDir}/{fileName}";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[InventoryUIBuilder] Generado {path}");
    }
}
