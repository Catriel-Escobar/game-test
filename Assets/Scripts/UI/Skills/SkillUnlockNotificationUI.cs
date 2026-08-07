using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUnlockNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private Image icon;
    [SerializeField] private float displayDuration = 3f;

    private LocalizationConfig _localization;
    private Coroutine _hideRoutine;

    public void Initialize(Player player)
    {
        _localization = ConfigBoostrap.Current.LocalizationConfig;
        if (player?.Skills != null)
            player.Skills.OnSkillUnlocked += OnSkillUnlocked;

        if (panel != null) panel.SetActive(false);
    }

    private void OnSkillUnlocked(SkillDefinition skill)
    {
        if (panel == null) return;

        if (titleText != null)
            titleText.text = _localization?.Get("skill.unlocked") ?? "New Skill!";
        if (skillNameText != null)
            skillNameText.text = _localization?.Get(skill.nameKey) ?? skill.id;
        if (icon != null)
        {
            Sprite sprite = Resources.Load<Sprite>($"Skills/Icons/{skill.id}");
            if (sprite != null) icon.sprite = sprite;
        }

        panel.SetActive(true);

        if (_hideRoutine != null)
            StopCoroutine(_hideRoutine);
        _hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        if (panel != null) panel.SetActive(false);
        _hideRoutine = null;
    }

    private void OnDisable()
    {
        if (panel != null) panel.SetActive(false);
    }
}
