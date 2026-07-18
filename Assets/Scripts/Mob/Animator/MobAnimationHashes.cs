using UnityEngine;

public static class MobAnimationHashes
{
    public static readonly int Speed = Animator.StringToHash("Speed");
    public static readonly int HitTrigger = Animator.StringToHash("Hit");
    public static readonly int AttackTrigger = Animator.StringToHash("Attack");
    public static readonly int DeathTrigger = Animator.StringToHash("Death");
}
