using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    private int upperLayerIndex;
    private Coroutine upperLayerCoroutine;

    [SerializeField] private AnimationClip[] _attackClips;

    private Dictionary<string, AnimationClip> _clips;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        upperLayerIndex = animator.GetLayerIndex("UpperLayer");
        _clips = new Dictionary<string, AnimationClip>();
      foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
    {
        Debug.Log(clip.name);
        _clips[clip.name] = clip;
    }
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
                PlayUpperAnimation(Animator.StringToHash(attack.id),Animator.StringToHash(attack.id));
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
        {
            Debug.LogWarning($"[Skills] Animación '{animationId}' no encontrada en el Animator");
            return;
        }

        PlayUpperAnimation(
            Animator.StringToHash(animationId),
            Animator.StringToHash(animationId));
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
    private void PlayUpperAnimation(int triggerHash, int stateHash)
    {
        // Si ya hay una animación reproduciéndose en esta capa,
        // la cancelamos para que la nueva tenga prioridad.
        if (upperLayerCoroutine != null)
            StopCoroutine(upperLayerCoroutine);

        upperLayerCoroutine = StartCoroutine(
            PlayUpperAnimationRoutine(triggerHash, stateHash));
    }

    private IEnumerator PlayUpperAnimationRoutine(int triggerHash, int stateHash)
    {
        animator.SetLayerWeight(upperLayerIndex, 1f);

        animator.ResetTrigger(triggerHash);
        animator.SetTrigger(triggerHash);

        // Esperar un frame para que el Animator procese el trigger.
        yield return null;

        // Esperar hasta entrar al estado.
        while (animator.GetCurrentAnimatorStateInfo(upperLayerIndex).fullPathHash != stateHash)
        {
            yield return null;
        }

        // Esperar hasta que termine la animación.
        while (true)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(upperLayerIndex);

            if (stateInfo.fullPathHash != stateHash)
                break;

            if (stateInfo.normalizedTime >= 1f)
                break;

            yield return null;
        }

        animator.SetLayerWeight(upperLayerIndex, 0f);
        upperLayerCoroutine = null;
    }
}