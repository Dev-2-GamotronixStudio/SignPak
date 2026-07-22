using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationQueuePlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private Coroutine _queueCoroutine;

    /// <summary>
    /// Accepts a list or array of animation state names and plays them sequentially.
    /// </summary>
    public void PlayAnimationSequence(IEnumerable<string> animationSequence, float fadeDuration = 0.15f)
    {
        // Stop any sequence currently playing before starting a new one
        if (_queueCoroutine != null)
        {
            StopCoroutine(_queueCoroutine);
        }

        _queueCoroutine = StartCoroutine(PlaySequenceRoutine(new Queue<string>(animationSequence), fadeDuration));
    }

    private IEnumerator PlaySequenceRoutine(Queue<string> sequence, float fadeDuration)
    {
        while (sequence.Count > 0)
        {
            string animName = sequence.Dequeue();
            int animHash = Animator.StringToHash(animName);

            // 1. Trigger the crossfade into the next animation
            animator.CrossFade(animHash, fadeDuration);

            // 2. Wait 1 frame so the Animator updates to the newly playing state
            yield return null;

            // 3. Get state info for the current state on Layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 4. Calculate exact duration remaining for this clip
            // stateInfo.length is full duration in seconds; normalizedTime tracks progress
            float remainingNormalizedTime = 1f - (stateInfo.normalizedTime % 1f);
            float clipRemainingTime = remainingNormalizedTime * stateInfo.length;

            // Subtract fade duration so the NEXT transition starts before this clip hard-ends
            float waitTime = Mathf.Max(0f, clipRemainingTime - fadeDuration);

            // 5. Wait until this animation completes before looping to the next one
            yield return new WaitForSeconds(waitTime);
        }

        _queueCoroutine = null;
    }
}