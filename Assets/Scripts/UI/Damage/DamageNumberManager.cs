using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }

    [SerializeField] private int poolSize = 30;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatDuration = 0.8f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private float criticalScale = 1.5f;

    private Canvas canvas;
    private RectTransform canvasRect;
    private Camera mainCamera;
    private List<DamageNumberInstance> pool = new();

    private void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        mainCamera = Camera.main;
        BuildPool();
    }

    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            pool.Add(CreateInstance());
        }
    }

    private DamageNumberInstance CreateInstance()
    {
        GameObject obj = new GameObject("DamageNumber");
        obj.transform.SetParent(canvasRect, false);

        RectTransform rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 60f);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);

        obj.AddComponent<CanvasGroup>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);

        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 36;
        tmp.fontStyle = FontStyles.Bold;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color32(0, 0, 0, 255);
        tmp.fontMaterial.EnableKeyword("OUTLINE_ON");

        DamageNumberInstance instance = obj.AddComponent<DamageNumberInstance>();
        obj.SetActive(false);
        return instance;
    }

    public void Show(Vector3 worldPosition, int damage, bool isCritical)
    {
        DamageNumberInstance instance = GetFromPool();
        if (instance == null) return;

        Color color = isCritical ? criticalColor : normalColor;
        float scale = isCritical ? criticalScale : 1f;
        string display = isCritical ? $"{damage}!" : damage.ToString();

        instance.gameObject.SetActive(true);
        instance.Setup(display, color, scale);
        instance.Animate(worldPosition, floatSpeed, floatDuration, mainCamera, () => ReturnToPool(instance));
    }

    private DamageNumberInstance GetFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeInHierarchy)
                return pool[i];
        }

        DamageNumberInstance newInstance = CreateInstance();
        pool.Add(newInstance);
        return newInstance;
    }

    private void ReturnToPool(DamageNumberInstance instance)
    {
        instance.Deactivate();
    }
}
