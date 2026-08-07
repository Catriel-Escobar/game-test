public static class SkillTargetingFactory
{
    public static ISkillTargeting Create(string targeting)
    {
        return targeting switch
        {
            "self" => new SelfTargeting(),
            "mouse" => new MouseTargeting(),
            "move_dir" => new MoveDirectionTargeting(),
            _ => new SelfTargeting(),
        };
    }
}
