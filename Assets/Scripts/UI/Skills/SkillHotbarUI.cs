using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillHotbarUI : MonoBehaviour
{
    [SerializeField] private SkillSlotUI[] slots = new SkillSlotUI[3];
    [SerializeField] private string[] keyBindings = { "Q", "E", "R" };

    private Player _player;
    private SkillDefinition[] _classSkills;
    private readonly Dictionary<string, SkillSlotUI> _slotBySkill = new Dictionary<string, SkillSlotUI>();

    public void Initialize(Player player)
    {
        _player = player;
        if (player?.Skills == null) return;

        _classSkills = player.Skills.GetClassSkills();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            if (i < _classSkills.Length)
            {
                SkillDefinition skill = _classSkills[i];
                slots[i].Setup(skill, i < keyBindings.Length ? keyBindings[i] : "", OnSlotClicked);
                _slotBySkill[skill.id] = slots[i];
            }
            else
            {
                slots[i].ResetSlot();
                slots[i].gameObject.SetActive(false);
            }
        }

        player.Skills.OnSkillUnlocked += OnSkillUnlocked;
        player.Resources.OnManaChanged += OnManaChanged;
        player.Caster.OnCastCompleted += OnCastCompleted;

        RefreshSlots();
    }

    private void OnSlotClicked(SkillDefinition skill)
    {
        _player?.Caster?.TryCastSkill(skill.id);
    }

    private void OnSkillUnlocked(SkillDefinition skill)
    {
        if (_slotBySkill.TryGetValue(skill.id, out SkillSlotUI slot))
            slot.SetUnlocked(true);
        RefreshManaAffordability();
    }

    private void OnManaChanged(int currentMana, int maxMana)
    {
        RefreshManaAffordability();
    }

    private void OnCastCompleted(SkillDefinition skill)
    {
        RefreshCooldowns();
    }

    private void Update()
    {
        if (_player == null) return;
        RefreshCooldowns();
    }

    private void RefreshSlots()
    {
        if (_player?.Skills == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || slots[i].Skill == null) continue;
            slots[i].SetUnlocked(_player.Skills.IsUnlocked(slots[i].Skill.id));
        }

        RefreshCooldowns();
        RefreshManaAffordability();
    }

    private void RefreshCooldowns()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlotUI slot = slots[i];
            if (slot?.Skill == null) continue;

            float total = slot.Skill.cooldown;
            float remaining = _player.Caster.Cooldowns.GetRemaining(slot.Skill.id);
            slot.SetCooldown(remaining, total);
        }
    }

    private void RefreshManaAffordability()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlotUI slot = slots[i];
            if (slot?.Skill == null) continue;

            bool canAfford = _player.Resources.CurrentMana >= slot.Skill.manaCost;
            slot.SetCanAfford(canAfford);
        }
    }

    private void OnDisable()
    {
        if (_player == null) return;

        if (_player.Skills != null)
            _player.Skills.OnSkillUnlocked -= OnSkillUnlocked;
        if (_player.Resources != null)
            _player.Resources.OnManaChanged -= OnManaChanged;
        if (_player.Caster != null)
            _player.Caster.OnCastCompleted -= OnCastCompleted;
    }
}
