using System;
using System.Diagnostics;

public class PlayerStats
{
    public int Strength { get; private set; }
    public int Vitality { get; private set; }
    public int Intelligence { get; private set; }
    public int Dexterity { get; private set; }

    public PlayerStats(
        int strength,
        int vitality,
        int intelligence,
        int dexterity)
    {
        Strength = strength;
        Vitality = vitality;
        Intelligence = intelligence;
        Dexterity = dexterity;
    }

    public PlayerStats()
    {
    }

    public void AddStrength(int amount)
    {
        Strength += amount;
    }

    public void AddVitality(int amount)
    {
        Vitality += amount;
    }

    public void AddIntelligence(int amount)
    {
        Intelligence += amount;
    }

    public void AddDexterity(int amount)
    {
        Dexterity += amount;
    }

    internal void Initialize(PlayerStatsConfig baseStats)
    {
        Strength = baseStats.strength;
        Vitality = baseStats.vitality;
        Intelligence = baseStats.intelligence;
        Dexterity = baseStats.dexterity;
    }
}