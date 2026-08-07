using System.Collections.Generic;
using UnityEngine;

public class MobAttackHitbox : MonoBehaviour
{
    [SerializeField] private MobCombat mobCombat;
    [SerializeField] private Collider hitboxCollider;

    private readonly HashSet<Collider> _alreadyHit = new HashSet<Collider>();
    private CombatService _combatService;
    private Mob _mob;

    private void Awake()
    {
        _combatService = new CombatService();

        if (hitboxCollider == null)
            hitboxCollider = GetComponent<Collider>();

        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
        }

        if (mobCombat == null)
            mobCombat = GetComponentInParent<MobCombat>();

        if (mobCombat != null)
            _mob = mobCombat.GetComponent<Mob>();
    }

    private void Start()
    {
        if (mobCombat != null)
            mobCombat.OnAttackStateChanged += SetActiveHitbox;
    }

    private void OnDestroy()
    {
        if (mobCombat != null)
            mobCombat.OnAttackStateChanged -= SetActiveHitbox;
    }

    public void SetActiveHitbox(bool active)
    {
        if (!active)
            _alreadyHit.Clear();

        if (hitboxCollider != null)
            hitboxCollider.enabled = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mobCombat == null || !mobCombat.IsAttacking)
            return;

        if (_alreadyHit.Contains(other))
            return;

        if (!other.TryGetComponent<Player>(out var player) || !player.IsAlive)
            return;

        Attack attack = mobCombat.CurrentAttack;
        if (attack == null || _mob == null)
            return;

        _alreadyHit.Add(other);

        _combatService.Attack(_mob, player, attack);
    }
}
