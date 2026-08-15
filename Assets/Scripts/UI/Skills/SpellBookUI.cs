using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpellBookUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private SpellBookEntryUI entryPrefab;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Key toggleKey = Key.B;

    private Player _player;
    private LocalizationConfig _localization;
    private bool _isOpen;

    public void Initialize(Player player)
    {
        _player = player;
        _localization = ConfigBoostrap.Current.LocalizationConfig;

        if (titleText != null)
            titleText.text = _localization?.Get("spellbook.title") ?? "Spellbook";
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null || panel == null) return;
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        if (_isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (panel == null || _player == null) return;
        BuildEntries();
        _isOpen = true;
        panel.SetActive(true);
    }

    public void Close()
    {
        if (panel == null) return;
        _isOpen = false;
        panel.SetActive(false);
    }

    private void BuildEntries()
    {
        if (entryContainer == null || entryPrefab == null) return;

        for (int i = entryContainer.childCount - 1; i >= 0; i--)
            Destroy(entryContainer.GetChild(i).gameObject);

        SkillDefinition[] activeSkills = _player.Skills.GetActiveSkills();
        for (int i = 0; i < activeSkills.Length; i++)
        {
            SpellBookEntryUI entry = Instantiate(entryPrefab, entryContainer);
            entry.gameObject.SetActive(true);
            entry.Setup(activeSkills[i], true, _localization);
        }
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }
}
