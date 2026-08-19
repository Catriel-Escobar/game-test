public static class SkillEffectFactory
{
    public static ISkillEffect Create(SkillEffectDefinition definition)
    {
        if (definition == null) return null;

        return definition.type switch
        {
            "damage_aoe" => new DamageAreaEffect(definition),
            "self_buff" => new SelfBuffEffect(definition),
            "dash" => new DashEffect(definition),
            "stun" => new StunEffect(definition),
            "slow" => new SlowEffect(definition),
            "debuff" => new DebuffEffect(definition),
            "shield" => new ShieldEffect(definition),
            _ => null,
        };
    }
}
