using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateCharacterModal : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _modalPanel;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TMP_Dropdown _classDropdown;

    [Header("Buttons")]
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _closeButton;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI _errorText;

    private CharacterSelectionService _service;
    private LocalizationConfig _localization;
    private CharacterClassesConfig _classesConfig;

    public event Action OnCharacterCreated;
    public event Action OnCancelled;

    private void Awake()
    {
        if (_createButton != null)
            _createButton.onClick.AddListener(HandleCreate);
        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(HandleCancel);
        if (_closeButton != null)
            _closeButton.onClick.AddListener(HandleCancel);
    }

    private void OnDestroy()
    {
        if (_createButton != null)
            _createButton.onClick.RemoveListener(HandleCreate);
        if (_cancelButton != null)
            _cancelButton.onClick.RemoveListener(HandleCancel);
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(HandleCancel);
    }

    public void Initialize(CharacterSelectionService service, LocalizationConfig localization, CharacterClassesConfig classesConfig)
    {
        _service = service;
        _localization = localization;
        _classesConfig = classesConfig;

        PopulateClassDropdown();

        if (_modalPanel != null)
            _modalPanel.SetActive(false);
    }

    public void Open()
    {
        if (_nameInput != null)
            _nameInput.text = "";

        if (_errorText != null)
            _errorText.gameObject.SetActive(false);

        if (_classDropdown != null && _classesConfig?.classes != null && _classesConfig.classes.Length > 0)
            _classDropdown.value = 0;

        gameObject.SetActive(true);

        if (_modalPanel != null)
            _modalPanel.SetActive(true);
    }

    public void Close()
    {
        if (_modalPanel != null)
            _modalPanel.SetActive(false);

        gameObject.SetActive(false);
    }

    private void HandleCreate()
    {
        string characterName = _nameInput != null ? _nameInput.text : "";

        if (!_service.IsValidName(characterName))
        {
            ShowError(_localization.Get("char.nameEmpty"));
            return;
        }

        if (characterName.Trim().Length > _service.GetMaxNameLength())
        {
            ShowError(_localization.Get("char.nameTooLong"));
            return;
        }

        string classId = GetSelectedClassId();

        CharacterData newCharacter = _service.CreateCharacter(characterName, classId);
        if (newCharacter != null)
        {
            Close();
            OnCharacterCreated?.Invoke();
        }
    }

    private void HandleCancel()
    {
        Close();
        OnCancelled?.Invoke();
    }

    private void ShowError(string message)
    {
        if (_errorText != null)
        {
            _errorText.text = message;
            _errorText.gameObject.SetActive(true);
        }
    }

    private void PopulateClassDropdown()
    {
        if (_classDropdown == null || _classesConfig?.classes == null) return;

        _classDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        foreach (var entry in _classesConfig.classes)
        {
            string displayName = _localization.Get(entry.nameKey);
            options.Add(displayName);
        }

        _classDropdown.AddOptions(options);
    }

    private string GetSelectedClassId()
    {
        if (_classesConfig?.classes == null || _classesConfig.classes.Length == 0)
            return "warrior";

        int index = _classDropdown != null ? _classDropdown.value : 0;
        index = Mathf.Clamp(index, 0, _classesConfig.classes.Length - 1);
        return _classesConfig.classes[index].id;
    }
}
