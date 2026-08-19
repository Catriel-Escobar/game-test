using System.Collections;
using UnityEngine;

public class ImpactVisual : SkillVisualBase
{
    public ImpactVisual(SkillVisualDefinition definition) : base(definition)
    {
    }

    public override IEnumerator Play(SkillVisualContext context)
    {
        if (Definition.delay > 0f)
            yield return new WaitForSeconds(Definition.delay);

        Vector3 position = ResolvePosition(context);
        SkillVfxSpawner.Spawn(Definition.prefab, position, Quaternion.identity, ResolveDestroyAfter(context));
    }
}
