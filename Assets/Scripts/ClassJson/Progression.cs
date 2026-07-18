using System;
using UnityEngine;

[Serializable]
public class ProgressionConfig
{
    public int maxLevel ;
    public double baseExperience;
    public ProgressionSegment[] segments;
    public int statPointsPerLevel ;
    public int skillPointsPerLevel ;

    [NonSerialized] private long[] _experiencePerLevel;

    public long[] ExperiencePerLevel
    {
        get
        {
            if (_experiencePerLevel == null)
                BuildExperienceTable();

            return _experiencePerLevel;
        }
    }

    public void BuildExperienceTable()
    {
        if (segments == null || segments.Length == 0)
        throw new Exception("ProgressionConfig requires at least one segment.");
        _experiencePerLevel = new long[maxLevel + 1];

        _experiencePerLevel[0] = 0;
        if (maxLevel >= 1)
            _experiencePerLevel[1] = 0;

        double currentXp = baseExperience;

        for (int level = 2; level <= maxLevel; level++)
        {
            _experiencePerLevel[level] =(long)Mathf.Round((float)currentXp);

            ProgressionSegment segment = GetSegmentForLevel(level);
            currentXp *= segment.multiplier;
        }
    }

    private ProgressionSegment GetSegmentForLevel(int level)
    {
        if (segments != null)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if (level <= segments[i].untilLevel)
                    return segments[i];
            }
        }

        return new ProgressionSegment
        {
            untilLevel = maxLevel,
            multiplier = 1.5f
        };
    }
}

[Serializable]
public class ProgressionSegment
{
    public int untilLevel;
    public float multiplier;
}