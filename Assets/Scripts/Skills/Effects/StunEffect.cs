using System.Collections;
using UnityEngine;

public class StunEffect : ISkillEffect
{
    private readonly SkillEffectDefinition _definition;

    public StunEffect(SkillEffectDefinition definition)
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
            if (mob.IsDead || mob.IsStunned) continue;

            StatusEffectManager.Instance?.Add(new StunStatusEffect(mob, _definition.duration));
        }

        yield break;
    }
}
