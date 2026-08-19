using System;
using UnityEngine;

[Serializable]
public class SkillDefinition
{
    public string id;
    public string nameKey;
    public string descriptionKey;
    public int manaCost;
    public float cooldown;
    public float castTime;
    public string animationId;
    public string targeting;
    public SkillEffectDefinition[] effects;
    public SkillVisualDefinition[] visuals;
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
}

[Serializable]
public class SkillVisualDefinition
{
    public string type;       // cast | projectile | impact | hit | persistent
    public string trigger;    // cast | resolve | end
    public string prefab;     // path en Resources/ (ej: VFX/VFX_Impact01)
    public string anchor;     // origin | center | hitpoint | player
    public Vector3 offset;
    public float destroyAfter;
    public float delay;
    public bool followPlayer;
    public float travelTime;
}
