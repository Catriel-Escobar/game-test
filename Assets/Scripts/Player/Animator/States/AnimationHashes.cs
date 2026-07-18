using UnityEngine;

public static class AnimationHashes
{
    // Parameters
    public static readonly int Speed = Animator.StringToHash("Speed");
    public static readonly int HitTrigger = Animator.StringToHash("Hit");

    // States
    public static readonly int HitState =
        Animator.StringToHash("UpperLayer.Hit");

    public static readonly int AttackSpeed  = Animator.StringToHash("AttackSpeed");

}