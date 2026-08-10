using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private InventorySlotUI _slotPrefab;
    [SerializeField] private Button _closeButton;
    [SerializeField] private TMP_Text _titleText;

    [Header("References")]
    [SerializeField] private ItemTooltipUI _tooltip;

    private Player _player;
    private PlayerInputs _playerInputs;
    private LocalizationConfig _localization;
    private static RectTransform s_panelRect;

    private bool _isOpen;
    private bool _subscribed;

    public bool IsOpen => _isOpen;

    public static bool IsPointerOverPanel(Vector2 screenPos)
    {
        return s_panelRect != null &&
               s_panelRect.gameObject.activeInHierarchy &&
               RectTransformUtility.RectangleContainsScreenPoint(s_panelRect, screenPos, null);
    }

    public void Initialize(Player player, PlayerInputs playerInputs)
    {
        _player = player;
        _playerInputs = playerInputs;
        _localization = ConfigBoostrap.Current?.LocalizationConfig;

        if (_panel != null)
        {
            s_panelRect = _panel.transform as RectTransform;
            _panel.SetActive(false);
        }

        if (_tooltip != null)
            _tooltip.Initialize(_localization);

        Subscribe();

        UpdateTexts();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        if (_subscribed || _player == null) return;
        _subscribed = true;

        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);

        if (_player.Inventory != null)
            _player.Inventory.OnInventoryChanged += OnInventoryChanged;
        if (_player.Equipment != null)
            _player.Equipment.OnEquipmentChanged += OnEquipmentChanged;
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;
        _subscribed = false;

        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(Close);

        if (_player != null)
        {
            if (_player.Inventory != null)
                _player.Inventory.OnInventoryChanged -= OnInventoryChanged;
            if (_player.Equipment != null)
                _player.Equipment.OnEquipmentChanged -= OnEquipmentChanged;
        }
    }

    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (_panel == null || _player == null || _isOpen) return;

        _isOpen = true;
        _panel.SetActive(true);
        BuildSlots();
    }

    public void Close()
    {
        if (_panel == null || !_isOpen) return;

        _isOpen = false;
        if (_tooltip != null) _tooltip.Hide();
        _panel.SetActive(false);
    }

    private void OnInventoryChanged()
    {
        if (_isOpen) BuildSlots();
    }

    private void OnEquipmentChanged()
    {
        if (_isOpen) BuildSlots();
    }

    private void BuildSlots()
    {
        if (_slotContainer == null || _slotPrefab == null) return;

        if (_slotContainer.gameObject.scene.name == null)
        {
            Debug.LogError($"[InventoryUI] '_slotContainer' apunta a un prefab asset (objeto persistente). " +
                           $"Arrastra el prefab InventoryPanel desde Project a la escena (no lo edites en Prefab Mode), " +
                           $"y asegúrate de que la referencia apunte al SlotContainer de la instancia en escena.", this);
            return;
        }

        for (int i = _slotContainer.childCount - 1; i >= 0; i--)
            Destroy(_slotContainer.GetChild(i).gameObject);

        if (_player?.Inventory == null) return;

        IReadOnlyList<ItemStack> stacks = _player.Inventory.Stacks;
        int capacity = _player.Inventory.Capacity;
        int total = Mathf.Max(capacity, stacks.Count);

        for (int i = 0; i < total; i++)
        {
            InventorySlotUI slot = Instantiate(_slotPrefab, _slotContainer);
            slot.gameObject.SetActive(true);

            if (i < stacks.Count)
            {
                ItemStack stack = stacks[i];
                if (stack == null || stack.count <= 0)
                {
                    slot.ResetSlot();
                    continue;
                }

                Item item = _player.Equipment.FindItemById(stack.itemId);
                if (item == null)
                {
                    slot.ResetSlot();
                    continue;
                }

                slot.Setup(stack, item, OnSlotClicked, OnSlotHovered, OnSlotExited, OnSlotDrop);
            }
            else
            {
                slot.ResetSlot();
            }
        }
    }

    private void OnSlotClicked(ItemStack stack)
    {
        if (_player?.Equipment == null || stack == null) return;

        Item item = _player.Equipment.FindItemById(stack.itemId);
        if (item == null) return;

        if (item.Type == ItemType.Equipment)
        {
            if (!_player.Equipment.EquipFromInventory(stack.instanceId))
                Debug.Log($"[InventoryUI] No se pudo equipar '{stack.itemId}'.");
        }
        else if (item.Type == ItemType.Consumable)
        {
            _player.Equipment.UseConsumable(stack.itemId);
        }
    }

    private void OnSlotHovered(ItemStack stack)
    {
        if (_tooltip == null || _player?.Equipment == null || stack == null) return;
        Item item = _player.Equipment.FindItemById(stack.itemId);
        if (item == null) return;
        _tooltip.Show(item, stack.affixes);
    }

    private void OnSlotExited()
    {
        if (_tooltip != null) _tooltip.Hide();
    }

    private void OnSlotDrop(ItemStack stack, Vector2 screenPos)
    {
        if (_player?.Inventory == null || stack == null) return;

        if (_panel != null && RectTransformUtility.RectangleContainsScreenPoint(
                (RectTransform)_panel.transform, screenPos, null))
            return;

        if (EventSystem.current != null)
        {
            var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, hits);
            if (hits.Count > 0) return;
        }

        Vector3 dropPos = _player.transform.position + _player.transform.forward * 1.5f + Vector3.up * 0.1f;
        WorldDrop.Spawn(stack.itemId, stack.count, stack.affixes, dropPos);
        _player.Inventory.RemoveByInstanceId(stack.instanceId);
    }

    private void UpdateTexts()
    {
        if (_titleText != null && _localization != null)
            _titleText.text = _localization.Get("inventory.title");
    }

    private void OnDisable()
    {
        Unsubscribe();
    }
}
