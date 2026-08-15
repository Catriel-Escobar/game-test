using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellBookEntryUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text requirementText;
    [SerializeField] private GameObject lockedOverlay;

    public void Setup(SkillDefinition skill, bool isUnlocked, LocalizationConfig localization)
    {
        string name = localization?.Get(skill.nameKey) ?? skill.id;
        string description = localization?.Get(skill.descriptionKey) ?? skill.descriptionKey;

        if (nameText != null) nameText.text = name;
        if (descriptionText != null) descriptionText.text = description;

        if (requirementText != null)
            requirementText.gameObject.SetActive(false);

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        if (icon != null)
        {
            Sprite sprite = Resources.Load<Sprite>($"Skills/Icons/{skill.id}");
            if (sprite != null) icon.sprite = sprite;
        }
    }
}
