using System.Collections;
using UnityEngine;

public class PersistentVisual : SkillVisualBase
{
    public PersistentVisual(SkillVisualDefinition definition) : base(definition)
    {
    }

    public override IEnumerator Play(SkillVisualContext context)
    {
        if (Definition.delay > 0f)
            yield return new WaitForSeconds(Definition.delay);

        Vector3 position = ResolvePosition(context);
        GameObject instance = SkillVfxSpawner.Spawn(Definition.prefab, position, Quaternion.identity, ResolveDestroyAfter(context));
        if (instance == null)
            yield break;

        if (Definition.followPlayer || Definition.anchor == "player")
            AttachFollow(context.Player != null ? context.Player.transform : null, instance);
    }
}
