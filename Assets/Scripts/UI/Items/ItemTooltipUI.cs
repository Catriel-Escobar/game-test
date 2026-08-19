using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemTooltipUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TMP_Text _contentText;
    [SerializeField] private RectTransform _tooltipRect;

    private LocalizationConfig _localization;

    private void Awake()
    {
        if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        if (_contentText == null) _contentText = GetComponentInChildren<TMP_Text>();
        if (_tooltipRect == null) _tooltipRect = transform as RectTransform;

        if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        if (_canvasGroup != null) _canvasGroup.interactable = false;
        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
    }

    public void Initialize(LocalizationConfig localization)
    {
        _localization = localization;
    }

    public void Show(Item item, ItemAffix[] affixes)
    {
        if (item == null || _canvasGroup == null) return;

        if (_contentText != null)
            _contentText.text = ItemUICore.BuildTooltip(item, affixes, _localization);

        Canvas.ForceUpdateCanvases();
        if (_tooltipRect != null)
        {
            Vector2 size = new Vector2(
                _contentText != null ? _contentText.preferredWidth : 0f,
                _contentText != null ? _contentText.preferredHeight : 0f);
            _tooltipRect.sizeDelta = new Vector2(Mathf.Max(size.x + 20f, 160f), size.y + 16f);
        }

        PositionAtMouse();

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void Hide()
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void PositionAtMouse()
    {
        if (_tooltipRect == null) return;

        Vector2 screenPos = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        float offset = 16f;

        RectTransform parentRect = _tooltipRect.parent as RectTransform;
        if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, screenPos, null, out Vector2 localPoint))
        {
            _tooltipRect.localPosition = localPoint + new Vector2(offset, -offset);
        }
    }
}
