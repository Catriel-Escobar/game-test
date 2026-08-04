using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject noManaOverlay;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private TMP_Text manaCostText;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text lockedLevelText;

    [Header("Interaction")]
    [SerializeField] private Button button;

    private SkillDefinition _skill;
    private Action<SkillDefinition> _onClick;
    private bool _isUnlocked;

    public SkillDefinition Skill => _skill;

    private void Awake()
    {
        if (cooldownOverlay != null && cooldownOverlay.type != Image.Type.Filled)
            cooldownOverlay.type = Image.Type.Filled;
    }

    public void Setup(SkillDefinition skill, string key, Action<SkillDefinition> onClick)
    {
        _skill = skill;
        _onClick = onClick;

        if (keyLabel != null) keyLabel.text = key;
        if (manaCostText != null) manaCostText.text = skill.manaCost.ToString();
        if (icon != null)
        {
            Sprite sprite = Resources.Load<Sprite>($"Skills/Icons/{skill.id}");
            if (sprite != null) icon.sprite = sprite;
        }
        if (button != null) button.onClick.AddListener(OnButtonClicked);

        SetUnlocked(true);
        SetCanAfford(true);
        SetCooldown(0f, skill.cooldown);
    }

    private void OnButtonClicked()
    {
        if (!_isUnlocked) return;
        _onClick?.Invoke(_skill);
    }

    public void SetUnlocked(bool unlocked)
    {
        _isUnlocked = unlocked;
        if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
        if (lockedLevelText != null)
        {
            lockedLevelText.gameObject.SetActive(!unlocked);
            if (!unlocked && _skill != null)
                lockedLevelText.text = $"Lv. {_skill.requiresLevel}";
        }
    }

    public void SetCanAfford(bool canAfford)
    {
        if (noManaOverlay != null) noManaOverlay.SetActive(!canAfford && _isUnlocked);
    }

    public void SetCooldown(float remaining, float total)
    {
        float amount = total > 0f ? Mathf.Clamp01(remaining / total) : 0f;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = amount;

        if (cooldownText != null)
        {
            if (remaining > 0f)
            {
                cooldownText.text = remaining >= 10f
                    ? Mathf.CeilToInt(remaining).ToString()
                    : remaining.ToString("F1");
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
    }

    public void ResetSlot()
    {
        _skill = null;
        _isUnlocked = false;
        SetUnlocked(false);
        SetCanAfford(true);
        SetCooldown(0f, 0f);
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
        if (lockedOverlay != null) lockedOverlay.SetActive(false);
        if (noManaOverlay != null) noManaOverlay.SetActive(false);
    }
}
