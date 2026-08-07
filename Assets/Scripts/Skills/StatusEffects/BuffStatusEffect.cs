using UnityEngine;

public class BuffStatusEffect : StatusEffect
{
    private readonly Player _target;
    private readonly string _statId;
    private readonly float _percent;

    public BuffStatusEffect(Player target, string statId, float percent, float duration) : base(duration)
    {
        _target = target;
        _statId = statId;
        _percent = percent;
    }

    protected override void OnApply()
    {
        _target.AddBuffMultiplier(_statId, _percent);
        Debug.Log($"[Skills] Buff {_statId} +{_percent:P0} aplicado a {_target.name} por {Remaining:F1}s");
    }

    protected override void OnExpire()
    {
        _target.RemoveBuffMultiplier(_statId, _percent);
        Debug.Log($"[Skills] Buff {_statId} expirado en {_target.name}");
    }
}
