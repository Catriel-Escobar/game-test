using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EquipmentUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private EquipmentSlotUI[] _slots;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _totalStatsText;

    [Header("References")]
    [SerializeField] private ItemTooltipUI _tooltip;

    private static RectTransform s_panelRect;

    private Player _player;
    private LocalizationConfig _localization;
    private readonly Dictionary<EquipmentSlot, EquipmentSlotUI> _slotByEnum = new Dictionary<EquipmentSlot, EquipmentSlotUI>();
    private bool _isOpen;
    private bool _subscribed;

    public static bool IsPointerOverPanel(Vector2 screenPos)
    {
        return s_panelRect != null &&
               s_panelRect.gameObject.activeInHierarchy &&
               RectTransformUtility.RectangleContainsScreenPoint(s_panelRect, screenPos, null);
    }

    public void Initialize(Player player)
    {
        _player = player;
        _localization = ConfigBoostrap.Current?.LocalizationConfig;

        if (_tooltip != null)
            _tooltip.Initialize(_localization);

        if (_slots != null)
        {
            EquipmentSlot[] values = (EquipmentSlot[])System.Enum.GetValues(typeof(EquipmentSlot));
            for (int i = 0; i < _slots.Length; i++)
            {
                EquipmentSlotUI slot = _slots[i];
                if (slot == null) continue;

                EquipmentSlot enumValue = i < values.Length ? values[i] : (EquipmentSlot)i;
                slot.Setup(enumValue, _localization, OnSlotClicked, OnSlotHovered, OnSlotExited);
                _slotByEnum[enumValue] = slot;
            }
        }

        if (_panel != null)
        {
            s_panelRect = _panel.transform as RectTransform;
            _panel.SetActive(false);
        }

        Subscribe();

        UpdateTexts();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        if (_subscribed || _player?.Equipment == null) return;
        _subscribed = true;
        _player.Equipment.OnEquipmentChanged += OnEquipmentChanged;
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;
        _subscribed = false;

        if (_player?.Equipment != null)
            _player.Equipment.OnEquipmentChanged -= OnEquipmentChanged;
    }

    public bool IsOpen => _isOpen;

    public void Open()
    {
        if (_panel == null || _player == null || _isOpen) return;
        _isOpen = true;
        _panel.SetActive(true);
        RefreshAll();
    }

    public void Close()
    {
        if (_panel == null || !_isOpen) return;
        _isOpen = false;
        if (_tooltip != null) _tooltip.Hide();
        _panel.SetActive(false);
    }

    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    private void OnEquipmentChanged()
    {
        if (IsOpen) RefreshAll();
    }

    public void RefreshAll()
    {
        if (_player?.Equipment == null) return;

        EquipmentSlot[] values = (EquipmentSlot[])System.Enum.GetValues(typeof(EquipmentSlot));
        for (int i = 0; i < values.Length; i++)
        {
            EquipmentSlot enumValue = values[i];
            EquipmentSlotUI slot;
            if (_slotByEnum.TryGetValue(enumValue, out slot) && slot != null)
            {
                Item item = _player.Equipment.GetItemInSlot(enumValue);
                ItemAffix[] affixes = _player.Equipment.GetEquippedAffixesInSlot(enumValue);
                slot.Refresh(item, affixes);
            }
        }

        RefreshTotalStats();
    }

    private void RefreshTotalStats()
    {
        if (_totalStatsText == null) return;

        ItemStats stats = _player?.Equipment != null ? _player.Equipment.TotalStats : null;
        string text = ItemUICore.FormatTotalStats(stats);
        _totalStatsText.text = string.IsNullOrEmpty(text) ? "" : text;
    }

    private void OnSlotClicked(EquipmentSlot slot)
    {
        if (_player?.Equipment == null) return;
        _player.Equipment.Unequip(slot);
    }

    private void OnSlotHovered(EquipmentSlot slot)
    {
        if (_tooltip == null || _player?.Equipment == null) return;
        Item item = _player.Equipment.GetItemInSlot(slot);
        ItemAffix[] affixes = _player.Equipment.GetEquippedAffixesInSlot(slot);
        if (item == null) return;
        _tooltip.Show(item, affixes);
    }

    private void OnSlotExited()
    {
        if (_tooltip != null) _tooltip.Hide();
    }

    private void UpdateTexts()
    {
        if (_titleText != null && _localization != null)
            _titleText.text = _localization.Get("equipment.title");
    }

    private void OnDisable()
    {
        Unsubscribe();
    }
}
