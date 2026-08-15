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

    [Header("Buttons")]
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _closeButton;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI _errorText;

    private CharacterSelectionService _service;
    private LocalizationConfig _localization;

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

    public void Initialize(CharacterSelectionService service, LocalizationConfig localization)
    {
        _service = service;
        _localization = localization;

        if (_modalPanel != null)
            _modalPanel.SetActive(false);
    }

    public void Open()
    {
        if (_nameInput != null)
            _nameInput.text = "";

        if (_errorText != null)
            _errorText.gameObject.SetActive(false);

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

        CharacterData newCharacter = _service.CreateCharacter(characterName);
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
}
