using System.Collections;

public class SelfBuffEffect : ISkillEffect
{
    private readonly SkillEffectDefinition _definition;

    public SelfBuffEffect(SkillEffectDefinition definition)
    {
        _definition = definition;
    }

    public IEnumerator Apply(SkillCastContext context)
    {
        StatusEffectManager.Instance?.Add(
            new BuffStatusEffect(context.Player, _definition.statId, _definition.percent, _definition.duration));

        yield break;
    }
}
