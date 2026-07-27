using System;

[Serializable]
public class CharacterClassesConfig
{
    public CharacterClassEntry[] classes;
}

[Serializable]
public class CharacterClassEntry
{
    public string id;
    public string nameKey;
    public PlayerStatsConfig baseStats;
}
