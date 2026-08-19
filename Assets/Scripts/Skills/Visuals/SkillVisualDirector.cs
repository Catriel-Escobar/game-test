using System.Collections;
using UnityEngine;

public class SkillVisualDirector : MonoBehaviour
{
    public void PlayTrigger(SkillCastContext context, string trigger)
    {
        if (context?.Skill?.visuals == null) return;

        for (int i = 0; i < context.Skill.visuals.Length; i++)
        {
            SkillVisualDefinition visual = context.Skill.visuals[i];
            if (visual == null || string.IsNullOrEmpty(visual.type)) continue;
            if (ResolveTrigger(visual) != trigger) continue;

            StartCoroutine(PlayVisual(context, visual, Vector3.zero));
        }
    }

    public void PlayHit(SkillCastContext context, Vector3 hitPoint)
    {
        if (context?.Skill?.visuals == null) return;

        for (int i = 0; i < context.Skill.visuals.Length; i++)
        {
            SkillVisualDefinition visual = context.Skill.visuals[i];
            if (visual == null || visual.type != "hit") continue;

            StartCoroutine(PlayVisual(context, visual, hitPoint));
        }
    }

    private static string ResolveTrigger(SkillVisualDefinition visual)
    {
        if (!string.IsNullOrEmpty(visual.trigger))
            return visual.trigger;

        return visual.type switch
        {
            "cast" => "cast",
            _ => "resolve"
        };
    }

    private IEnumerator PlayVisual(SkillCastContext context, SkillVisualDefinition visual, Vector3 hitPoint)
    {
        SkillVisualContext visualContext = new SkillVisualContext
        {
            Player = context.Player,
            Skill = context.Skill,
            Visual = visual,
            Origin = context.Origin,
            Center = context.Center,
            Direction = context.Direction,
            HitPoint = hitPoint
        };

        ISkillVisual skillVisual = SkillVisualFactory.Create(visual);
        if (skillVisual == null)
        {
            Debug.LogWarning($"[Skills][VFX] Tipo de visual desconocido: {visual.type}");
            yield break;
        }

        yield return skillVisual.Play(visualContext);
    }
}
