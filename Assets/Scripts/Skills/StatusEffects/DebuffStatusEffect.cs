using UnityEngine;

public class DebuffStatusEffect : StatusEffect
{
    private readonly Player _target;
    private readonly string _statId;
    private readonly float _percent;

    public DebuffStatusEffect(Player target, string statId, float percent, float duration) : base(duration)
    {
        _target = target;
        _statId = statId;
        _percent = percent;
    }

    protected override void OnApply()
    {
        if (_target == null) return;

        _target.AddBuffMultiplier(_statId, -_percent);
        Debug.Log($"[Skills] Debuff {_statId} -{_percent:P0} aplicado a {_target.name} por {Remaining:F1}s");
    }

    protected override void OnExpire()
    {
        if (_target == null) return;

        _target.RemoveBuffMultiplier(_statId, -_percent);
        Debug.Log($"[Skills] Debuff {_statId} expirado en {_target.name}");
    }
}
