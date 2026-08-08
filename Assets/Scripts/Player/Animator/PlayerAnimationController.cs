using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private AnimationClip[] _attackClips;

    [SerializeField] private PlayerCombat combat;

    private Dictionary<string, AnimationClip> _clips;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _clips = new Dictionary<string, AnimationClip>();

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            _clips[clip.name] = clip;
        }

        if (combat == null)
            combat = GetComponent<PlayerCombat>();
    }

    public void OnAttackStart()
    {
        combat?.SetAttackActive(true);
    }

    public void OnAttackEnd()
    {
        combat?.SetAttackActive(false);
    }

    public void OnSwingStart()
    {
        combat?.BeginSwing();
    }

    public void OnSwingEnd()
    {
        combat?.EndSwing();
    }

    public void Move(float speed)
    {
        animator.SetFloat(AnimationHashes.Speed, speed);
    }

    public void OnHitAnimation()
    {
        // PlayUpperAnimation(
        //     AnimationHashes.HitTrigger,
        //     AnimationHashes.HitState);
    }


    public void AttacksMeeles(Attack attack, float attackSpeedMultiplier)
    {
        try
        {
            float speed = GetAnimationLength(attack.id) / attack.duration;
            speed *= attackSpeedMultiplier;
            animator.SetFloat(AnimationHashes.AttackSpeed, speed);
            FireTrigger(Animator.StringToHash(attack.id));
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
            throw;
        }
    }

    public void PlaySkillCast(string animationId)
    {
        if (string.IsNullOrEmpty(animationId)) return;

        if (!_clips.ContainsKey(animationId))
            Debug.LogWarning($"[Skills] Animación '{animationId}' no encontrada en los clips del Animator (verificar nombre del clip)");

        FireTrigger(Animator.StringToHash(animationId));
    }

    public int SelectHash(string param)
    {
        return param switch
        {
            "basic_attack" => Animator.StringToHash("basic_attack"),
            "heavy_attack" => Animator.StringToHash("heavy_attack"),
            _ => 0,
        };
    }


    private float GetAnimationLength(string animationId)
    {
        if (_clips.TryGetValue(animationId, out AnimationClip clip))
            return clip.length;

        throw new System.Exception($"Animation '{animationId}' not found.");
    }

    private void FireTrigger(int triggerHash)
    {
        animator.ResetTrigger(triggerHash);
        animator.SetTrigger(triggerHash);
    }
}
