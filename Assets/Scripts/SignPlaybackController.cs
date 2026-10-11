using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SignPlaybackController : MonoBehaviour
{
    [SerializeField] private SignDatabaseSO database;
    [SerializeField] private float crossFadeDuration = 0.18f; // Smooth blend transition

    private Animator _animator;
    private AnimatorOverrideController _overrideController;
    private Coroutine _playbackRoutine;

    private const string CLIP_PLACEHOLDER = "SignPlaceholder";
    private const string STATE_IDLE = "Idle";
    private const string STATE_SIGN = "PerformSign";

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        // Wrap runtime animator controller in an override controller
        _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;

        database.Initialize();
    }

    public void PlaySignSequence(List<AnimationClip> clips)
    {
        if (clips == null || clips.Count == 0) return;

        if (_playbackRoutine != null)
            StopCoroutine(_playbackRoutine);

        _playbackRoutine = StartCoroutine(PlaybackRoutine(clips));
    }

    private IEnumerator PlaybackRoutine(List<AnimationClip> clips)
    {
        foreach (var clip in clips)
        {
            if (clip == null) continue;

            // Swap the placeholder animation with the target sign clip
            _overrideController[CLIP_PLACEHOLDER] = clip;

            // Trigger smooth cross-fade to the sign state
            _animator.CrossFadeInFixedTime(STATE_SIGN, crossFadeDuration, 0, 0f);

            // Wait for clip duration minus cross-fade time to prevent dead frames
            float waitDuration = Mathf.Max(clip.length - crossFadeDuration, 0.05f);
            yield return new WaitForSeconds(waitDuration);
        }

        // Return smoothly to Idle when all signs finish
        _animator.CrossFadeInFixedTime(STATE_IDLE, crossFadeDuration, 0);
        _playbackRoutine = null;
    }
}