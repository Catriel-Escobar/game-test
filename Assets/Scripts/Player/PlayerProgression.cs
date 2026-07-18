using System;
using UnityEngine;

public class PlayerProgression
{
    public int Level { get; private set; } = 1;

    public double CurrentExperience { get; private set; }

    public int ExperienceToNextLevel => Level * 100;

    public event Action<double>OnCurrentExperienceChanged;
    public event Action<int,double,long>OnLevelChanged;

    public long[] ExperiencePerLevel;
    public int StatPointsPerLevel = 5;
    public int SkillPointsPerLevel = 1;
    public int MaxLevel;
    private readonly PlayerStats _stats;
       public PlayerProgression(PlayerStats stats)
    {
        _stats = stats;
    }

    public void Initialize(ProgressionConfig config)
    {
        StatPointsPerLevel = config.skillPointsPerLevel;
        SkillPointsPerLevel = config.skillPointsPerLevel;
        ExperiencePerLevel = config.ExperiencePerLevel;
        CurrentExperience = 0;
        MaxLevel = config.maxLevel;
    }
    public void AddExperience(float amount)
    {
        if(Level >= MaxLevel)
            return;
        
        CurrentExperience += amount;
        OnCurrentExperienceChanged.Invoke(CurrentExperience);
        while (Level < MaxLevel &&
       CurrentExperience >= ExperiencePerLevel[Level + 1])
        {
            CurrentExperience =0;

            Level++;

            // Otorgar puntos
            LevelUp();

            OnLevelChanged?.Invoke(Level,CurrentExperience, ExperiencePerLevel[Mathf.Min(Level + 1, MaxLevel)]);
        }
    }

    private void LevelUp()
    {

        _stats.AddStrength(1);
        _stats.AddVitality(2);
        _stats.AddIntelligence(1);
        _stats.AddDexterity(1);
    }
}