using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills
{
    private const int MaxActiveSkills = 3;

    public SkillsConfig SkillsConfig { get; private set; }
    private PlayerEquipment _equipment;
    private readonly List<string> _activeSkillIds = new List<string>();

    public event Action OnSkillsChanged;
    public event Action<SkillDefinition> OnSkillGained;

    public void Initialize(SkillsConfig skillsConfig, PlayerEquipment equipment)
    {
        SkillsConfig = skillsConfig;
        _equipment = equipment;

        if (_equipment != null)
            _equipment.OnEquipmentChanged += HandleEquipmentChanged;

        RefreshActiveSkills();
    }

    public void Dispose()
    {
        if (_equipment != null)
            _equipment.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    private void HandleEquipmentChanged()
    {
        RefreshActiveSkills();
    }

    private void RefreshActiveSkills()
    {
        List<string> previous = new List<string>(_activeSkillIds);

        _activeSkillIds.Clear();
        if (_equipment != null)
        {
            AddSlotSkills(EquipmentSlot.Weapon, _activeSkillIds);
            AddSlotSkills(EquipmentSlot.OffHand, _activeSkillIds);
        }

        OnSkillsChanged?.Invoke();

        for (int i = 0; i < _activeSkillIds.Count; i++)
        {
            if (!previous.Contains(_activeSkillIds[i]))
                OnSkillGained?.Invoke(GetSkill(_activeSkillIds[i]));
        }
    }

    private void AddSlotSkills(EquipmentSlot slot, List<string> target)
    {
        if (_equipment == null) return;

        Item item = _equipment.GetItemInSlot(slot);
        if (item?.skillIds == null) return;

        for (int i = 0; i < item.skillIds.Length && target.Count < MaxActiveSkills; i++)
        {
            string skillId = item.skillIds[i];
            if (string.IsNullOrEmpty(skillId) || target.Contains(skillId)) continue;
            target.Add(skillId);
        }
    }

    public string[] GetEquippedSkillIds()
    {
        return _activeSkillIds.ToArray();
    }

    public SkillDefinition[] GetActiveSkills()
    {
        List<SkillDefinition> result = new List<SkillDefinition>();
        for (int i = 0; i < _activeSkillIds.Count; i++)
        {
            SkillDefinition definition = GetSkill(_activeSkillIds[i]);
            if (definition != null) result.Add(definition);
        }

        return result.ToArray();
    }

    public SkillDefinition GetSkill(string skillId)
    {
        if (SkillsConfig?.skills == null || string.IsNullOrEmpty(skillId)) return null;

        for (int i = 0; i < SkillsConfig.skills.Length; i++)
        {
            if (SkillsConfig.skills[i].id == skillId)
                return SkillsConfig.skills[i];
        }

        return null;
    }

    public bool IsActiveSkill(string skillId)
    {
        return _activeSkillIds.Contains(skillId);
    }

    public void DebugPrintSkills()
    {
        Debug.Log($"[Skills] Skills activas del equipo: {_activeSkillIds.Count}");
        SkillDefinition[] activeSkills = GetActiveSkills();
        for (int i = 0; i < activeSkills.Length; i++)
            Debug.Log($"[Skills]  - {activeSkills[i].id}");
    }
}
