using System.Collections;
using UnityEngine;

public class ShieldEffect : ISkillEffect
{
    private readonly SkillEffectDefinition _definition;

    public ShieldEffect(SkillEffectDefinition definition)
    {
        _definition = definition;
    }

    public IEnumerator Apply(SkillCastContext context)
    {
        Player player = context.Player;
        if (player == null) yield break;

        float percent = Mathf.Clamp01(_definition.percent);
        float duration = Mathf.Max(0.1f, _definition.duration);

        StatusEffectManager.Instance?.Add(
            new DamageReductionStatusEffect(player, percent, duration));

        yield break;
    }
}
