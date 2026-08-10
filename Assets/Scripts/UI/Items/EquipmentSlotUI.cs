using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _slotLabel;
    [SerializeField] private Image _rarityBorder;
    [SerializeField] private Image _emptyOverlay;

    private EquipmentSlot _slot;
    private Item _item;
    private Action<EquipmentSlot> _onClick;
    private Action<EquipmentSlot> _onHover;
    private Action _onExit;

    public EquipmentSlot Slot => _slot;
    public Item Item => _item;

    public void Setup(EquipmentSlot slot, LocalizationConfig localization, Action<EquipmentSlot> onClick, Action<EquipmentSlot> onHover, Action onExit)
    {
        _slot = slot;
        _onClick = onClick;
        _onHover = onHover;
        _onExit = onExit;

        if (_slotLabel != null)
            _slotLabel.text = ItemUICore.SlotName(slot, localization);
    }

    public void Refresh(Item item, ItemAffix[] affixes)
    {
        _item = item;
        ItemRarity rarity = ItemUICore.EffectiveRarity(item, affixes);

        if (_icon != null)
        {
            if (item != null)
            {
                ItemUICore.SetIcon(_icon, item, rarity);
                _icon.enabled = true;
            }
            else
            {
                _icon.sprite = null;
                _icon.enabled = false;
            }
        }

        if (_rarityBorder != null)
        {
            Color color = item != null
                ? ItemUICore.RarityColor(rarity)
                : new Color(1f, 1f, 1f, 0.1f);
            _rarityBorder.color = color;
        }

        if (_emptyOverlay != null)
            _emptyOverlay.gameObject.SetActive(item == null);

        Button button = GetComponent<Button>();
        if (button != null) button.interactable = item != null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_item == null) return;
        _onClick?.Invoke(_slot);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item == null) return;
        _onHover?.Invoke(_slot);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _onExit?.Invoke();
    }
}
