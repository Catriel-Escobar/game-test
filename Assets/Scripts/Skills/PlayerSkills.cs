using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills
{
    public string ClassId { get; private set; }
    public SkillsConfig SkillsConfig { get; private set; }
    public HashSet<string> UnlockedSkillIds { get; private set; } = new HashSet<string>();

    public event Action<SkillDefinition> OnSkillUnlocked;

    public void Initialize(string classId, SkillsConfig skillsConfig, string[] unlockedSkillIds = null)
    {
        ClassId = classId;
        SkillsConfig = skillsConfig;

        UnlockedSkillIds = new HashSet<string>();
        if (unlockedSkillIds != null)
        {
            for (int i = 0; i < unlockedSkillIds.Length; i++)
                UnlockedSkillIds.Add(unlockedSkillIds[i]);
        }
    }

    public SkillDefinition[] GetClassSkills()
    {
        if (SkillsConfig?.skills == null || string.IsNullOrEmpty(ClassId))
            return Array.Empty<SkillDefinition>();

        List<SkillDefinition> result = new List<SkillDefinition>();
        for (int i = 0; i < SkillsConfig.skills.Length; i++)
        {
            SkillDefinition skill = SkillsConfig.skills[i];
            if (skill.classId == ClassId)
                result.Add(skill);
        }

        return result.ToArray();
    }

    public SkillDefinition GetSkill(string skillId)
    {
        if (SkillsConfig?.skills == null) return null;

        for (int i = 0; i < SkillsConfig.skills.Length; i++)
        {
            if (SkillsConfig.skills[i].id == skillId)
                return SkillsConfig.skills[i];
        }

        return null;
    }

    public bool IsUnlocked(string skillId)
    {
        return UnlockedSkillIds.Contains(skillId);
    }

    public string[] GetEquippedSkillIds()
    {
        SkillDefinition[] classSkills = GetClassSkills();
        List<string> equipped = new List<string>();

        for (int i = 0; i < classSkills.Length && equipped.Count < 3; i++)
        {
            if (IsUnlocked(classSkills[i].id))
                equipped.Add(classSkills[i].id);
        }

        return equipped.ToArray();
    }

    public void Unlock(SkillDefinition skill)
    {
        if (skill == null || UnlockedSkillIds.Contains(skill.id)) return;

        UnlockedSkillIds.Add(skill.id);
        Debug.Log($"[Skills] Desbloqueada: {skill.id} (clase {skill.classId}, requiere nivel {skill.requiresLevel})");
        OnSkillUnlocked?.Invoke(skill);
    }

    public void DebugPrintSkills()
    {
        Debug.Log($"[Skills] Clase: {ClassId} | Skills desbloqueadas: {UnlockedSkillIds.Count}");
        SkillDefinition[] classSkills = GetClassSkills();
        for (int i = 0; i < classSkills.Length; i++)
        {
            SkillDefinition skill = classSkills[i];
            string state = IsUnlocked(skill.id)
                ? "DESBLOQUEADA"
                : $"bloqueada (requiere nivel {skill.requiresLevel})";
            Debug.Log($"[Skills]  - {skill.id}: {state}");
        }
    }
}
