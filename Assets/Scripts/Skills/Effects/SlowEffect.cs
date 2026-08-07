using System.Collections;
using UnityEngine;

public class SlowEffect : ISkillEffect
{
    private readonly SkillEffectDefinition _definition;

    public SlowEffect(SkillEffectDefinition definition)
    {
        _definition = definition;
    }

    public IEnumerator Apply(SkillCastContext context)
    {
        float radius = Mathf.Max(0.1f, _definition.radius);
        Collider[] hits = Physics.OverlapSphere(context.Center, radius);

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].TryGetComponent<Mob>(out Mob mob)) continue;
            if (mob.IsDead) continue;

            StatusEffectManager.Instance?.Add(new SlowStatusEffect(mob, _definition.duration, _definition.slowPercent));
        }

        yield break;
    }
}
