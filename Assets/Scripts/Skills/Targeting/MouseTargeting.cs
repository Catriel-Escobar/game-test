using UnityEngine;
using UnityEngine.InputSystem;

public class MouseTargeting : ISkillTargeting
{
    public void Resolve(SkillCastContext context)
    {
        context.Origin = context.Player.transform.position;
        context.Center = GetMouseWorldPoint(context.Player);

        float range = GetEffectRange(context.Skill);
        if (range > 0f)
        {
            Vector3 offset = context.Center - context.Origin;
            offset.y = 0f;

            if (offset.sqrMagnitude > range * range)
                context.Center = context.Origin + offset.normalized * range;
        }

        context.Direction = (context.Center - context.Origin).normalized;
    }

    private float GetEffectRange(SkillDefinition skill)
    {
        if (skill?.effects == null) return 0f;

        float maxRange = 0f;
        for (int i = 0; i < skill.effects.Length; i++)
        {
            if (skill.effects[i].range > maxRange)
                maxRange = skill.effects[i].range;
        }

        return maxRange;
    }

    private Vector3 GetMouseWorldPoint(Player player)
    {
        Camera camera = Camera.main;
        if (camera == null)
            return player.transform.position + player.transform.forward;

        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.up, player.transform.position);

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return player.transform.position + player.transform.forward;
    }
}
