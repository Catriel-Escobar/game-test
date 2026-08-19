using System.Collections;

public class DebuffEffect : ISkillEffect
{
    private readonly SkillEffectDefinition _definition;

    public DebuffEffect(SkillEffectDefinition definition)
    {
        _definition = definition;
    }

    public IEnumerator Apply(SkillCastContext context)
    {
        if (string.IsNullOrEmpty(_definition.statId)) yield break;

        StatusEffectManager.Instance?.Add(
            new DebuffStatusEffect(context.Player, _definition.statId, _definition.percent, _definition.duration));

        yield break;
    }
}
