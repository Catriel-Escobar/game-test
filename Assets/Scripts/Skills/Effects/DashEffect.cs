using System.Collections;
using UnityEngine;

public class DashEffect : ISkillEffect
{
    private readonly SkillEffectDefinition _definition;

    public DashEffect(SkillEffectDefinition definition)
    {
        _definition = definition;
    }

    public IEnumerator Apply(SkillCastContext context)
    {
        Player player = context.Player;
        if (player == null || player.Movement == null) yield break;

        Vector3 direction = context.Direction;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
            direction = player.transform.forward;

        float distance = Mathf.Max(0.1f, _definition.distance);
        float speed = _definition.speed > 0f ? _definition.speed : 12f;

        float stunDuration = 0f;
        float stunRadius = 1.2f;
        FindStunParams(context.Skill, ref stunDuration, ref stunRadius);

        player.Movement.BeginDash(direction, speed);

        float traveled = 0f;
        while (traveled < distance && player != null)
        {
            float step = speed * Time.deltaTime;
            player.Movement.DashStep();
            traveled += step;

            if (stunDuration > 0f)
            {
                Collider[] hits = Physics.OverlapSphere(player.transform.position, stunRadius);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (!hits[i].TryGetComponent<Mob>(out Mob mob)) continue;
                    if (mob.IsDead || mob.IsStunned) continue;

                    StatusEffectManager.Instance?.Add(new StunStatusEffect(mob, stunDuration));
                }
            }

            yield return null;
        }

        player.Movement.EndDash();
        context.Center = player.transform.position;
    }

    private void FindStunParams(SkillDefinition skill, ref float duration, ref float radius)
    {
        if (skill?.effects == null) return;

        for (int i = 0; i < skill.effects.Length; i++)
        {
            SkillEffectDefinition effect = skill.effects[i];
            if (effect.type == "stun")
            {
                if (effect.duration > duration) duration = effect.duration;
                if (effect.radius > 0f) radius = effect.radius;
            }
        }
    }
}
