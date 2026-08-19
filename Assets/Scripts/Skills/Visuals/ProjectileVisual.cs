using System.Collections;
using UnityEngine;

public class ProjectileVisual : SkillVisualBase
{
    public ProjectileVisual(SkillVisualDefinition definition) : base(definition)
    {
    }

    public override IEnumerator Play(SkillVisualContext context)
    {
        if (Definition.delay > 0f)
            yield return new WaitForSeconds(Definition.delay);

        Vector3 start = ResolvePosition(context);
        Vector3 end = context.Center.sqrMagnitude > 0.001f
            ? context.Center
            : start + (context.Direction.sqrMagnitude > 0.001f ? context.Direction.normalized : Vector3.forward) * 5f;

        if ((end - start).sqrMagnitude < 0.001f)
            yield break;

        GameObject instance = SkillVfxSpawner.Spawn(Definition.prefab, start, Quaternion.identity, 0f);
        if (instance == null)
            yield break;

        float travel = Definition.travelTime > 0f
            ? Definition.travelTime
            : Mathf.Clamp(Vector3.Distance(start, end) / 12f, 0.2f, 2f);

        instance.transform.LookAt(end);

        float t = 0f;
        while (t < 1f && instance != null)
        {
            t += Time.deltaTime / travel;
            instance.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        if (instance != null)
            Object.Destroy(instance);
    }
}
