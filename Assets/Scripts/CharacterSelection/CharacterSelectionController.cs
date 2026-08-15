using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelectionController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private CharacterSlotView _slotPrefab;
    [SerializeField] private Transform _slotsParent;
    [SerializeField] private int _maxSlots = 4;

    [Header("Buttons")]
    [SerializeField] private Button _createCharacterButton;
    [SerializeField] private Button _deleteCharacterButton;
    [SerializeField] private Button _playButton;

    [Header("Modals")]
    [SerializeField] private CreateCharacterModal _createModal;
    [SerializeField] private DeleteCharacterModal _deleteModal;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _createButtonText;
    [SerializeField] private TextMeshProUGUI _deleteButtonText;
    [SerializeField] private TextMeshProUGUI _playButtonText;

    [Header("Language")]
    [SerializeField] private TMP_Dropdown _languageDropdown;

    [Header("Scene")]
    [SerializeField] private string _gameSceneName = "OutdoorsScene";

    private CharacterSelectionService _service;
    private LocalizationConfig _localization;
    private CharacterData _selectedCharacter;
    private int _selectedIndex = -1;
    private List<CharacterSlotView> _slots = new List<CharacterSlotView>();
    private SaveManager _saveManager;

    private readonly List<string> _languageCodes = new List<string> { "en", "es" };

    private void Start()
    {
        if (ConfigBoostrap.Current == null)
        {
            var config = new ConfigBoostrap();
            config.Initialize();
        }

        _service = new CharacterSelectionService(new SaveManager());
        _saveManager = new SaveManager();
        _localization = ConfigBoostrap.Current.LocalizationConfig;

        InitializeSlots();
        InitializeModals();
        InitializeLanguageDropdown();
        SubscribeEvents();

        RefreshSlots();
        UpdateButtons();
        UpdateTexts();

        if (SelectedCharacterManager.Instance == null)
        {
            GameObject managerObj = new GameObject("SelectedCharacterManager");
            managerObj.AddComponent<SelectedCharacterManager>();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void InitializeSlots()
    {
        _slots.Clear();

        for (int i = 0; i < _maxSlots; i++)
        {
            CharacterSlotView slot = Instantiate(_slotPrefab, _slotsParent);
            slot.SetIndex(i);
            slot.OnClicked += OnSlotClicked;
            slot.OnCreateClicked += OnCreateCharacterClicked;
            _slots.Add(slot);
        }
    }

    private void InitializeModals()
    {
        if (_createModal != null)
        {
            Debug.Log("Inicializando create modal");
            _createModal.Initialize(_service, _localization);
            _createModal.OnCharacterCreated += OnCharacterCreated;
        }

        if (_deleteModal != null)
        {
            Debug.Log("Inicializando delete modal");
            _deleteModal.Initialize(_localization);
            _deleteModal.OnConfirmed += OnDeleteConfirmed;
        }
    }

    private void InitializeLanguageDropdown()
    {
        if (_languageDropdown == null) return;

        _languageDropdown.ClearOptions();
        _languageDropdown.AddOptions(new List<string> { "English", "Español" });

        string saved = GameSettingsManager.GetLanguage(_localization.defaultLanguage);
        int index = _languageCodes.IndexOf(saved);
        if (index < 0) index = 0;

        _languageDropdown.SetValueWithoutNotify(index);
        _localization.SetLanguage(_languageCodes[index]);

        _languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);
    }

    private void OnLanguageDropdownChanged(int index)
    {
        if (index < 0 || index >= _languageCodes.Count) return;

        string code = _languageCodes[index];
        _localization.SetLanguage(code);
        GameSettingsManager.SetLanguage(code);
    }

    private void SubscribeEvents()
    {
        if (_createCharacterButton != null)
            _createCharacterButton.onClick.AddListener(OnCreateCharacterClicked);
        if (_deleteCharacterButton != null)
            _deleteCharacterButton.onClick.AddListener(OnDeleteCharacterClicked);
        if (_playButton != null)
            _playButton.onClick.AddListener(OnPlayClicked);

        if (_localization != null)
            _localization.OnLanguageChanged += UpdateTexts;
    }

    private void UnsubscribeEvents()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].OnClicked -= OnSlotClicked;
            _slots[i].OnCreateClicked -= OnCreateCharacterClicked;
        }

        if (_createModal != null)
            _createModal.OnCharacterCreated -= OnCharacterCreated;
        if (_deleteModal != null)
            _deleteModal.OnConfirmed -= OnDeleteConfirmed;

        if (_createCharacterButton != null)
            _createCharacterButton.onClick.RemoveListener(OnCreateCharacterClicked);
        if (_deleteCharacterButton != null)
            _deleteCharacterButton.onClick.RemoveListener(OnDeleteCharacterClicked);
        if (_playButton != null)
            _playButton.onClick.RemoveListener(OnPlayClicked);

        if (_localization != null)
            _localization.OnLanguageChanged -= UpdateTexts;

        if (_languageDropdown != null)
            _languageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownChanged);
    }

    private void RefreshSlots()
    {
        List<CharacterData> characters = _service.GetCharacters();

        for (int i = 0; i < _slots.Count; i++)
        {
            CharacterData data = i < characters.Count ? characters[i] : null;
            _slots[i].Setup(data, _localization);
        }

        ClearSelection();
    }

    private void UpdateButtons()
    {
        if (_createCharacterButton != null)
            _createCharacterButton.interactable = _service.CanCreateCharacter();
        if (_deleteCharacterButton != null)
            _deleteCharacterButton.interactable = _selectedCharacter != null;
        if (_playButton != null)
            _playButton.interactable = _selectedCharacter != null;
    }

    private void UpdateTexts()
    {
        if (_localization == null) return;

        if (_titleText != null)
            _titleText.text = _localization.Get("char.title");
        if (_createButtonText != null)
            _createButtonText.text = _localization.Get("char.create");
        if (_deleteButtonText != null)
            _deleteButtonText.text = _localization.Get("char.delete");
        if (_playButtonText != null)
            _playButtonText.text = _localization.Get("char.play");

        for (int i = 0; i < _slots.Count; i++)
        {
            CharacterData data = _slots[i].CharacterData;
            _slots[i].Setup(data, _localization);
        }

        if (_selectedIndex >= 0 && _selectedIndex < _slots.Count)
            _slots[_selectedIndex].SetSelected(true);
    }

    private void ClearSelection()
    {
        _selectedCharacter = null;
        _selectedIndex = -1;

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetSelected(false);
        }
    }

    private void SelectSlot(int index)
    {
        if (_selectedIndex >= 0 && _selectedIndex < _slots.Count)
        {
            _slots[_selectedIndex].SetSelected(false);
        }

        _selectedIndex = index;
        _selectedCharacter = _slots[index].CharacterData;
        _slots[index].SetSelected(true);

        UpdateButtons();
    }

    private void OnSlotClicked(int index)
    {
        SelectSlot(index);
    }

    private void OnCreateCharacterClicked()
    {
        Debug.Log(_createModal != null);
        Debug.Log($"es posible crear un character ? {!_service.CanCreateCharacter()}");
        if (!_service.CanCreateCharacter()) return;
        if (_createModal != null) _createModal.Open();
    }

    private void OnDeleteCharacterClicked()
    {
        if (_selectedCharacter == null) return;
        if (_deleteModal != null) _deleteModal.Open(_selectedCharacter.name);
    }

    private void OnPlayClicked()
    {
        if (_selectedCharacter == null) return;

        SelectedCharacterManager.Instance?.SetCharacter(_selectedCharacter);
        SceneManager.LoadScene(_gameSceneName);
    }

    private void OnCharacterCreated()
    {
        RefreshSlots();

        List<CharacterData> characters = _service.GetCharacters();
        if (characters.Count > 0)
        {
            SelectSlot(characters.Count - 1);
        }

        UpdateButtons();
    }

    private void OnDeleteConfirmed()
    {
        if (_selectedCharacter == null) return;

        _saveManager.DeleteGameplay(_selectedCharacter.id);
        _service.DeleteCharacter(_selectedCharacter.id);
        RefreshSlots();
        UpdateButtons();
    }
}
