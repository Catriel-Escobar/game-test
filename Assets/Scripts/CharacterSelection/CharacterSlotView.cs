using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSlotView : MonoBehaviour
{
    [Header("Occupied Info")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _playTimeText;
    [SerializeField] private Image _portraitImage;

    [Header("Empty State")]
    [SerializeField] private TextMeshProUGUI _emptyText;
    [SerializeField] private TextMeshProUGUI _createButton;

    [Header("Selection")]
    [SerializeField] private Image _selectionBorder;

    [Header("Button")]
    [SerializeField] private Button _slotButton;

    private int _slotIndex;
    private CharacterData _characterData;

    public event Action<int> OnClicked;
    public event Action OnCreateClicked;
    public CharacterData CharacterData => _characterData;

    private void Awake()
    {
        if (_slotButton != null)
            _slotButton.onClick.AddListener(HandleClick);

        if (_createButton != null)
        {
            Button btn = _createButton.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(() => OnCreateClicked?.Invoke());
        }
    }

    private void OnDestroy()
    {
        if (_slotButton != null)
            _slotButton.onClick.RemoveListener(HandleClick);
    }

    public void SetIndex(int index)
    {
        _slotIndex = index;
    }

    public void Setup(CharacterData data, LocalizationConfig localization)
    {
        _characterData = data;

        bool hasCharacter = data != null;

        if (_nameText != null) _nameText.gameObject.SetActive(hasCharacter);
        if (_levelText != null) _levelText.gameObject.SetActive(hasCharacter);
        if (_playTimeText != null) _playTimeText.gameObject.SetActive(hasCharacter);
        if (_portraitImage != null) _portraitImage.gameObject.SetActive(hasCharacter);

        if (_emptyText != null)
        {
            _emptyText.gameObject.SetActive(!hasCharacter);
            _emptyText.text = localization.Get("char.emptySlot");
        }
        if (_createButton != null)
        {
            _createButton.gameObject.SetActive(!hasCharacter);
            _createButton.text = localization.Get("char.create");
        }

        if (hasCharacter)
        {
            if (_nameText != null) _nameText.text = data.name;
            if (_levelText != null) _levelText.text = $"{localization.Get("char.level")} {data.level}";
            if (_playTimeText != null) _playTimeText.text = $"{localization.Get("char.playTime")} {FormatPlayTime(data.playTime)}";
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (_selectionBorder != null)
            _selectionBorder.gameObject.SetActive(selected);
    }

    public void SetPortrait(Sprite sprite)
    {
        if (_portraitImage != null && sprite != null)
            _portraitImage.sprite = sprite;
    }

    private void HandleClick()
    {
        if (_characterData != null)
            OnClicked?.Invoke(_slotIndex);
    }

    private string FormatPlayTime(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
    }
}
