using UnityEngine;

public class SelfTargeting : ISkillTargeting
{
    public void Resolve(SkillCastContext context)
    {
        context.Origin = context.Player.transform.position;
        context.Center = context.Player.transform.position;
        context.Direction = context.Player.transform.forward;
    }
}
