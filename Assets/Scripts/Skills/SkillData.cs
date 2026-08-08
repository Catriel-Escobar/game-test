using System;

[Serializable]
public class SkillDefinition
{
    public string id;
    public string nameKey;
    public string descriptionKey;
    public string classId;
    public int requiresLevel;
    public int manaCost;
    public float cooldown;
    public float castTime;
    public string animationId;
    public string targeting;
    public SkillEffectDefinition[] effects;
}

[Serializable]
public class SkillEffectDefinition
{
    public string type;
    public int damageType;
    public float damageMultiplier;
    public float radius;
    public float range;
    public string statId;
    public float percent;
    public float duration;
    public float distance;
    public float speed;
    public float slowPercent;
    public string impactVfx;
    public float impactVfxDuration = 1f;
}
