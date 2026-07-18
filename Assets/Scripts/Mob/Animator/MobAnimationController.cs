using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MobAnimationController : MonoBehaviour
{
    private Animator animator;

    // [SerializeField] private AnimationClip[] _attackClips;
    // private int upperLayerIndex;
    // private Coroutine upperLayerCoroutine;
    // private Dictionary<string, AnimationClip> _clips;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // upperLayerIndex = animator.GetLayerIndex("UpperLayer");
        // _clips = new Dictionary<string, AnimationClip>();
        // foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        // {
        //     _clips[clip.name] = clip;
        // }
    }

    public void Move(float speed)
    {
        animator.SetFloat(MobAnimationHashes.Speed, speed);
    }

    public void PlayAttack()
    {
        animator.SetTrigger(MobAnimationHashes.AttackTrigger);
    }

    public void PlayHit()
    {
        animator.SetTrigger(MobAnimationHashes.HitTrigger);
    }

    public void PlayDeath(System.Action onComplete)
    {
        StartCoroutine(DeathRoutine(onComplete));
    }

    private System.Collections.IEnumerator DeathRoutine(
        System.Action onComplete)
    {
        animator.SetTrigger(MobAnimationHashes.DeathTrigger);

        yield return null;
        yield return null;

        float timeout = 5f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            AnimatorStateInfo stateInfo =
                animator.GetCurrentAnimatorStateInfo(0);

            if (!animator.IsInTransition(0)
                && stateInfo.normalizedTime >= 1f)
                break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        onComplete?.Invoke();
    }

    // public void PlayAttackUpper()
    // {
    //     PlayUpperAnimation(
    //         MobAnimationHashes.AttackTrigger,
    //         MobAnimationHashes.AttackTrigger);
    // }

    // public void PlayHitUpper()
    // {
    //     PlayUpperAnimation(
    //         MobAnimationHashes.HitTrigger,
    //         MobAnimationHashes.HitTrigger);
    // }

    // private void PlayUpperAnimation(int triggerHash, int stateHash)
    // {
    //     if (upperLayerCoroutine != null)
    //         StopCoroutine(upperLayerCoroutine);
    //
    //     upperLayerCoroutine = StartCoroutine(
    //         PlayUpperAnimationRoutine(triggerHash, stateHash));
    // }
    //
    // private IEnumerator PlayUpperAnimationRoutine(
    //     int triggerHash, int stateHash)
    // {
    //     animator.SetLayerWeight(upperLayerIndex, 1f);
    //
    //     animator.ResetTrigger(triggerHash);
    //     animator.SetTrigger(triggerHash);
    //
    //     yield return null;
    //
    //     while (animator.GetCurrentAnimatorStateInfo(upperLayerIndex)
    //         .fullPathHash != stateHash)
    //     {
    //         yield return null;
    //     }
    //
    //     while (true)
    //     {
    //         AnimatorStateInfo stateInfo =
    //             animator.GetCurrentAnimatorStateInfo(upperLayerIndex);
    //
    //         if (stateInfo.fullPathHash != stateHash)
    //             break;
    //
    //         if (stateInfo.normalizedTime >= 1f)
    //             break;
    //
    //         yield return null;
    //     }
    //
    //     animator.SetLayerWeight(upperLayerIndex, 0f);
    //     upperLayerCoroutine = null;
    // }
}
