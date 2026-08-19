using UnityEngine;

public class DamageReductionStatusEffect : StatusEffect
{
    private readonly Player _target;
    private readonly float _percent;
    private GameObject _shieldVfx;
    private ShieldRipples _ripples;

    public DamageReductionStatusEffect(Player target, float percent, float duration) : base(duration)
    {
        _target = target;
        _percent = percent;
    }

    protected override void OnApply()
    {
        if (_target == null) return;

        _target.AddDamageReduction(_percent);

        _shieldVfx = SpawnShieldVfx(_target);
        _ripples = _shieldVfx != null ? _shieldVfx.GetComponent<ShieldRipples>() : null;
        if (_ripples != null)
            _target.OnDamageReduced += OnTargetDamaged;

        Debug.Log($"[Skills] Bastion activo en {_target.name}: -{_percent:P0} dano recibido");
    }

    protected override void OnExpire()
    {
        if (_target == null) return;

        _target.RemoveDamageReduction(_percent);

        if (_ripples != null)
            _target.OnDamageReduced -= OnTargetDamaged;

        if (_shieldVfx != null)
        {
            UnityEngine.Object.Destroy(_shieldVfx);
            _shieldVfx = null;
        }

        _ripples = null;

        Debug.Log($"[Skills] Bastion expirado en {_target.name}");
    }

    private void OnTargetDamaged(Vector3 hitPoint)
    {
        if (_ripples == null || _target == null) return;

        _ripples.PlayRipple();
    }

    private static GameObject SpawnShieldVfx(Player player)
    {
        if (player == null) return null;

        GameObject prefab = Resources.Load<GameObject>("VFX/ShieldVFX");
        if (prefab == null)
        {
            Debug.LogWarning("[Skills] No se encontro 'VFX/ShieldVFX' en Resources. Asignar el prefab del escudo.");
            return null;
        }

        GameObject instance = UnityEngine.Object.Instantiate(prefab, player.transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);

        ShieldFollow follow = instance.AddComponent<ShieldFollow>();
        follow.Init(player.transform);

        Collider collider = instance.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        Rigidbody body = instance.GetComponent<Rigidbody>();
        if (body != null)
            body.isKinematic = true;

        return instance;
    }
}
