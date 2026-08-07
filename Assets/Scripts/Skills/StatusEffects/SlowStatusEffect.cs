using UnityEngine;

public class SlowStatusEffect : StatusEffect
{
    private readonly Mob _target;
    private readonly float _slowPercent;

    public SlowStatusEffect(Mob target, float duration, float slowPercent) : base(duration)
    {
        _target = target;
        _slowPercent = slowPercent;
    }

    protected override void OnApply()
    {
        _target.AddSlow(_slowPercent);
        Debug.Log($"[Skills] Slow aplicado a {_target.name} ({_slowPercent:P0} por {Remaining:F1}s)");
    }

    protected override void OnExpire()
    {
        _target.RemoveSlow(_slowPercent);
        Debug.Log($"[Skills] Slow expirado en {_target.name}");
    }
}
