using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _countText;
    [SerializeField] private Image _rarityBorder;

    private const float DragThreshold = 10f;

    private static InventorySlotUI s_draggingSlot;

    private ItemStack _stack;
    private Item _item;
    private Action<ItemStack> _onClick;
    private Action<ItemStack> _onHover;
    private Action _onExit;
    private Action<ItemStack, Vector2> _onDrop;
    private bool _interactable = true;
    private CanvasGroup _canvasGroup;
    private bool _dragging;
    private Vector2 _pressPosition;
    private GameObject _dragGhost;

    public ItemStack Stack => _stack;
    public Item Item => _item;

    public void Setup(ItemStack stack, Item item, Action<ItemStack> onClick, Action<ItemStack> onHover, Action onExit, Action<ItemStack, Vector2> onDrop)
    {
        _stack = stack;
        _item = item;
        _onClick = onClick;
        _onHover = onHover;
        _onExit = onExit;
        _onDrop = onDrop;

        ItemRarity rarity = stack != null ? stack.GetRarity() : (item != null ? item.Rarity : default);

        if (_icon != null) ItemUICore.SetIcon(_icon, item, rarity);

        if (_countText != null)
        {
            if (stack != null && stack.count > 1)
            {
                _countText.text = stack.count.ToString();
                _countText.gameObject.SetActive(true);
            }
            else
            {
                _countText.gameObject.SetActive(false);
            }
        }

        if (_rarityBorder != null)
        {
            Color color = ItemUICore.RarityColor(rarity);
            color.a = _rarityBorder.color.a > 0f ? _rarityBorder.color.a : 1f;
            _rarityBorder.color = color;
        }

        _interactable = stack != null;
        Button button = GetComponent<Button>();
        if (button != null) button.interactable = _interactable;
    }

    public void ResetSlot()
    {
        _stack = null;
        _item = null;
        _interactable = false;

        if (_icon != null) { _icon.sprite = null; _icon.color = new Color(1f, 1f, 1f, 0f); }
        if (_countText != null) _countText.gameObject.SetActive(false);
        if (_rarityBorder != null) _rarityBorder.color = new Color(0f, 0f, 0f, 0.2f);

        Button button = GetComponent<Button>();
        if (button != null) button.interactable = false;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        Vector2 currentPos = Mouse.current.position.ReadValue();
        bool leftHeld = Mouse.current.leftButton.isPressed;

        if (s_draggingSlot != null && s_draggingSlot != this)
            return;

        if (!leftHeld)
        {
            if (s_draggingSlot != this) return;

            if (_dragging)
            {
                EndDrag(currentPos);
            }
            else if (_stack != null && _interactable && _onClick != null && IsPointerOverMe(currentPos))
            {
                _onClick.Invoke(_stack);
            }

            s_draggingSlot = null;
            return;
        }

        if (s_draggingSlot == null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame &&
                IsPointerOverMe(currentPos) &&
                _stack != null && _interactable)
            {
                s_draggingSlot = this;
                _pressPosition = currentPos;
            }
            return;
        }

        if (!_dragging)
        {
            if (_stack == null || _onDrop == null) return;
            if ((currentPos - _pressPosition).sqrMagnitude >= DragThreshold * DragThreshold)
                StartDrag();
            return;
        }

        if (_dragGhost != null)
            _dragGhost.transform.position = currentPos;
    }

    private bool IsPointerOverMe(Vector2 screenPos)
    {
        RectTransform rect = transform as RectTransform;
        return rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, null);
    }

    private void OnDisable()
    {
        _dragging = false;
        if (s_draggingSlot == this) s_draggingSlot = null;
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        DestroyDragGhost();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (s_draggingSlot != null) return;
        if (!_interactable || _stack == null || _item == null) return;
        _onHover?.Invoke(_stack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (s_draggingSlot != null) return;
        _onExit?.Invoke();
    }

    private void StartDrag()
    {
        _dragging = true;
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _canvasGroup.alpha = 0.4f;
        CreateDragGhost();
    }

    private void EndDrag(Vector2 releasePos)
    {
        _dragging = false;
        if (s_draggingSlot == this) s_draggingSlot = null;
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        DestroyDragGhost();
        _onDrop?.Invoke(_stack, releasePos);
    }

    private void CreateDragGhost()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || _icon == null || _icon.sprite == null) return;

        _dragGhost = new GameObject("DragGhost", typeof(RectTransform));
        _dragGhost.transform.SetParent(canvas.transform, false);
        _dragGhost.transform.SetAsLastSibling();

        Image image = _dragGhost.AddComponent<Image>();
        image.sprite = _icon.sprite;
        image.raycastTarget = false;
        image.color = new Color(1f, 1f, 1f, _icon.color.a);

        RectTransform rect = (RectTransform)_dragGhost.transform;
        rect.sizeDelta = new Vector2(48f, 48f);
        rect.position = _pressPosition;
    }

    private void DestroyDragGhost()
    {
        if (_dragGhost != null) Destroy(_dragGhost);
        _dragGhost = null;
    }
}
