using UnityEngine;

public class MoveDirectionTargeting : ISkillTargeting
{
    public void Resolve(SkillCastContext context)
    {
        context.Origin = context.Player.transform.position;

        Vector3 velocity = context.Player.Movement.Velocity;
        velocity.y = 0f;

        context.Direction = velocity.sqrMagnitude > 0.01f
            ? velocity.normalized
            : context.Player.transform.forward;

        context.Center = context.Origin + context.Direction;
    }
}
