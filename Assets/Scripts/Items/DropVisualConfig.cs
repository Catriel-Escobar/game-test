using UnityEngine;

public static class DropVisualConfig
{
    public static GameObject DropPrefab { get; private set; }
    public static GameObject ItemNameLabelPrefab { get; private set; }
    public static RectTransform ItemNameLabelContainer { get; private set; }

    public static void Configure(GameObject dropPrefab, GameObject itemNameLabelPrefab, RectTransform itemNameLabelContainer)
    {
        DropPrefab = dropPrefab;
        ItemNameLabelPrefab = itemNameLabelPrefab;
        ItemNameLabelContainer = itemNameLabelContainer;
    }
}
