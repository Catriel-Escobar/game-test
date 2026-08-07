using System.Collections.Generic;
using UnityEngine;

public class SkillCooldownManager
{
    private readonly Dictionary<string, float> _cooldownEndTimes = new Dictionary<string, float>();

    public bool IsReady(string skillId, float cooldown)
    {
        if (cooldown <= 0f) return true;

        if (!_cooldownEndTimes.TryGetValue(skillId, out float endTime))
            return true;

        return Time.time >= endTime;
    }

    public void StartCooldown(string skillId, float cooldown)
    {
        if (cooldown <= 0f) return;

        _cooldownEndTimes[skillId] = Time.time + cooldown;
    }

    public float GetRemaining(string skillId)
    {
        if (!_cooldownEndTimes.TryGetValue(skillId, out float endTime))
            return 0f;

        return Mathf.Max(0f, endTime - Time.time);
    }
}
