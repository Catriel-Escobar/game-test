using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeleteCharacterModal : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _modalPanel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI _messageText;

    [Header("Buttons")]
    [SerializeField] private Button _deleteButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _closeButton;

    private LocalizationConfig _localization;

    public event Action OnConfirmed;
    public event Action OnCancelled;

    private void Awake()
    {
        if (_deleteButton != null)
            _deleteButton.onClick.AddListener(HandleDelete);
        if (_cancelButton != null)
            _cancelButton.onClick.AddListener(HandleCancel);
        if (_closeButton != null)
            _closeButton.onClick.AddListener(HandleCancel);
    }

    private void OnDestroy()
    {
        if (_deleteButton != null)
            _deleteButton.onClick.RemoveListener(HandleDelete);
        if (_cancelButton != null)
            _cancelButton.onClick.RemoveListener(HandleCancel);
        if (_closeButton != null)
            _closeButton.onClick.RemoveListener(HandleCancel);
    }

    public void Initialize(LocalizationConfig localization)
    {
        _localization = localization;

        if (_modalPanel != null)
            _modalPanel.SetActive(false);
    }

    public void Open(string characterName)
    {
        if (_messageText != null)
        {
            string confirmation = _localization.Get("char.confirmDelete");
            _messageText.text = $"{confirmation}\n<size=120%><b>{characterName}</b></size>";
        }

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

    private void HandleDelete()
    {
        Close();
        OnConfirmed?.Invoke();
    }

    private void HandleCancel()
    {
        Close();
        OnCancelled?.Invoke();
    }
}
