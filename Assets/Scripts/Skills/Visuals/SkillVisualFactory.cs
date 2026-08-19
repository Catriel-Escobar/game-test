public static class SkillVisualFactory
{
    public static ISkillVisual Create(SkillVisualDefinition definition)
    {
        if (definition == null) return null;

        return definition.type switch
        {
            "cast" => new CastVisual(definition),
            "projectile" => new ProjectileVisual(definition),
            "impact" => new ImpactVisual(definition),
            "hit" => new ImpactVisual(definition),
            "persistent" => new PersistentVisual(definition),
            _ => null,
        };
    }
}
