using System;

public class SkillUnlockService
{
    private readonly PlayerSkills _playerSkills;
    private readonly PlayerProgression _progression;

    public SkillUnlockService(PlayerSkills playerSkills, PlayerProgression progression)
    {
        _playerSkills = playerSkills;
        _progression = progression;
    }

    public void Initialize()
    {
        if (_progression == null) return;

        _progression.OnLevelChanged += OnLevelChanged;
        RefreshForLevel(_progression.Level);
    }

    public void Dispose()
    {
        if (_progression == null) return;

        _progression.OnLevelChanged -= OnLevelChanged;
    }

    private void OnLevelChanged(int level, double currentExperience, long experienceToNextLevel)
    {
        RefreshForLevel(level);
    }

    private void RefreshForLevel(int level)
    {
        SkillDefinition[] classSkills = _playerSkills.GetClassSkills();
        for (int i = 0; i < classSkills.Length; i++)
        {
            SkillDefinition skill = classSkills[i];
            if (skill.requiresLevel <= level && !_playerSkills.IsUnlocked(skill.id))
                _playerSkills.Unlock(skill);
        }
    }
}
