using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Collider hitboxCollider;

    private readonly HashSet<Collider> _alreadyHit = new HashSet<Collider>();
    private CombatService _combatService;
    private Player _player;

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

        if (playerCombat != null)
            _player = playerCombat.GetComponent<Player>();
    }

    void Start() {
       playerCombat.OnAttackStateChanged += SetActiveHitbox;
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
        if (playerCombat == null || !playerCombat.IsAttacking)
            return;

        if (_alreadyHit.Contains(other))
            return;
        if (other.TryGetComponent<Player>(out var player))
            return;

        if (!other.TryGetComponent<ICombatEntity>(out var damageable))
            return;

        Attack attack = playerCombat.CurrentAttack;
        if (attack == null || _player == null)
            return;

        _alreadyHit.Add(other);

        _combatService.Attack(
            _player,
            damageable,
            attack);
    }
}
   