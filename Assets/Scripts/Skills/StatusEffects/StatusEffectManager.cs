using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }

    private readonly HashSet<StatusEffect> _active = new HashSet<StatusEffect>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        TickAll(Time.deltaTime);
    }

    public void Add(StatusEffect effect)
    {
        if (effect == null) return;

        effect.Apply();
        _active.Add(effect);
    }

    public static void Stun(Mob target, float duration)
    {
        if (target == null || target.IsDead || duration <= 0f) return;

        Instance?.Add(new StunStatusEffect(target, duration));
    }

    private void TickAll(float deltaTime)
    {
        if (_active.Count == 0) return;

        List<StatusEffect> expired = null;

        foreach (StatusEffect effect in _active)
        {
            if (!effect.Tick(deltaTime))
            {
                if (expired == null)
                    expired = new List<StatusEffect>();

                expired.Add(effect);
            }
        }

        if (expired == null) return;

        for (int i = 0; i < expired.Count; i++)
        {
            expired[i].Expire();
            _active.Remove(expired[i]);
        }
    }
}
