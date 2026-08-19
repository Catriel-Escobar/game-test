using System.Collections;
using UnityEngine;

public abstract class SkillVisualBase : ISkillVisual
{
    protected readonly SkillVisualDefinition Definition;

    protected SkillVisualBase(SkillVisualDefinition definition)
    {
        Definition = definition;
    }

    public abstract IEnumerator Play(SkillVisualContext context);

    protected Vector3 ResolvePosition(SkillVisualContext context)
    {
        Vector3 basePosition = Definition.anchor switch
        {
            "center" => context.Center.sqrMagnitude > 0.001f ? context.Center : context.Origin,
            "hitpoint" => context.HitPoint,
            "player" => context.Player != null ? context.Player.transform.position : context.Origin,
            _ => context.Origin
        };

        return basePosition + Definition.offset;
    }

    protected float ResolveDestroyAfter(SkillVisualContext context)
    {
        if (Definition.destroyAfter > 0f) return Definition.destroyAfter;

        return Definition.type switch
        {
            "cast" => context.Skill != null ? Mathf.Max(0.1f, context.Skill.castTime) : 1f,
            "projectile" => Definition.travelTime > 0f ? Definition.travelTime : 1f,
            "hit" => 0.6f,
            "persistent" => 1f,
            _ => 1f
        };
    }

    protected void AttachFollow(Transform target, GameObject instance)
    {
        if (target == null || instance == null) return;

        TransformFollower follower = instance.AddComponent<TransformFollower>();
        follower.Init(target, Definition.offset);
    }
}
