using UnityEngine;

public class StunStatusEffect : StatusEffect
{
    private readonly Mob _target;

    public StunStatusEffect(Mob target, float duration) : base(duration)
    {
        _target = target;
    }

    protected override void OnApply()
    {
        _target.AddStun();
        Debug.Log($"[Skills] Stun aplicado a {_target.name} por {Remaining:F1}s");
    }

    protected override void OnExpire()
    {
        _target.RemoveStun();
        Debug.Log($"[Skills] Stun expirado en {_target.name}");
    }
}
