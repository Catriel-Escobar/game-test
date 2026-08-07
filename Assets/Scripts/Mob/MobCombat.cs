using System;
using UnityEngine;

public class MobCombat : MonoBehaviour
{
    [SerializeField] private MobAnimationController animation;

    private Mob _owner;

    public bool IsAttacking { get; private set; }
    public Attack CurrentAttack { get; private set; }

    public event Action<bool> OnAttackStateChanged;

    private void Awake()
    {
        _owner = GetComponent<Mob>();

        if (animation == null)
            animation = GetComponent<MobAnimationController>();
    }

    public bool TryBeginAttack(Attack attack)
    {
        if (attack == null || IsAttacking) return false;

        CurrentAttack = attack;
        animation?.PlayAttack();
        return true;
    }

    public void SetAttackActive(bool active)
    {
        if (IsAttacking == active) return;

        IsAttacking = active;

        if (!active)
            CurrentAttack = null;

        OnAttackStateChanged?.Invoke(IsAttacking);
    }

    private void OnDisable()
    {
        if (IsAttacking)
            SetAttackActive(false);
    }
}
