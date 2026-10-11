using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;

public class SignPlaybackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SignDatabaseSO database;
    [Tooltip("The character Animator. If empty, uses the Animator on this object or its children.")]
    [SerializeField] private Animator targetAnimator;
    [Header("Playback")]
    [SerializeField, Min(0)] private float crossFadeDuration = 0.18f;

    private const string PlaceholderName = "SignPlaceholder";
    private const string IdleState = "Base Layer.Idle";
    private const string SignState = "Base Layer.PerformSign";
    private AnimatorOverrideController _overrideController;
    private RuntimeAnimatorController _originalController;
    private AnimationClip _placeholder;
    private Coroutine _playbackRoutine;

    public bool IsReady { get; private set; }
    public bool IsPlaying { get; private set; }
    public AnimationClip CurrentClip { get; private set; }
    public Animator TargetAnimator => targetAnimator;

    private void Awake() => InitializePlayback();

    private bool InitializePlayback()
    {
        if (IsReady) return true;
        if (targetAnimator == null) targetAnimator = GetComponent<Animator>();
        if (targetAnimator == null) targetAnimator = GetComponentInChildren<Animator>(true);
        if (targetAnimator == null || targetAnimator.avatar == null ||
            !targetAnimator.avatar.isValid || !targetAnimator.avatar.isHuman)
            return Fail("Assign a character Animator with a valid Humanoid Avatar.");
        if (targetAnimator.runtimeAnimatorController == null)
            return Fail("Assign AnimationController to the character Animator.");

        var original = targetAnimator.runtimeAnimatorController;
        var overrides = new AnimatorOverrideController(original);
        var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrides.overridesCount);
        overrides.GetOverrides(pairs);
        AnimationClip placeholder = null;
        foreach (var pair in pairs)
        {
            if (pair.Key.name == PlaceholderName) placeholder = pair.Key;
        }
        if (placeholder == null)
        {
            Destroy(overrides);
            return Fail("PerformSign needs the SignPlaceholder clip in its Animator Controller.");
        }

        _originalController = original;
        _overrideController = overrides;
        _placeholder = placeholder;
        targetAnimator.runtimeAnimatorController = overrides;
        targetAnimator.applyRootMotion = false;
        targetAnimator.Rebind();
        if (!targetAnimator.HasState(0, Animator.StringToHash(IdleState)) ||
            !targetAnimator.HasState(0, Animator.StringToHash(SignState)))
        {
            targetAnimator.runtimeAnimatorController = original;
            Destroy(overrides);
            _overrideController = null;
            return Fail("Animator Controller needs Base Layer.Idle and Base Layer.PerformSign states.");
        }

        if (database != null)
        {
            database.Initialize();
            if (database.idleClip != null)
            {
                // The shared controller uses IdleStraightHands as the original idle motion.
                foreach (var pair in pairs)
                {
                    if (pair.Key.name == "IdleStraightHands")
                        overrides[pair.Key.name] = database.idleClip;
                }
            }
        }
        IsReady = true;
        targetAnimator.Play(IdleState, 0, 0);
        return true;
    }

    private bool Fail(string message)
    {
        Debug.LogError("[SignPlaybackController] " + message, this);
        return false;
    }

    public void PlaySignSequence(List<AnimationClip> clips)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || clips == null || clips.Count == 0)
            return;
        if (!InitializePlayback()) return;

        // Copy the sequence so changes to the caller's list cannot affect playback.
        var sequence = new List<AnimationClip>();
        foreach (var clip in clips)
        {
            if (clip == null) continue;
            if (!clip.humanMotion)
            {
                Debug.LogWarning("[SignPlaybackController] Skipping non-humanoid clip: " + clip.name, this);
                continue;
            }
            sequence.Add(clip);
        }
        if (sequence.Count == 0) return;
        StopPlayback(false);
        IsPlaying = true;
        _playbackRoutine = StartCoroutine(PlaybackRoutine(sequence));
    }

    public bool PlaySignByText(string text)
    {
        if (database == null || string.IsNullOrWhiteSpace(text)) return false;
        if (!database.TryGetClipByText(text.Trim(), out var clip, out _) || clip == null)
            return false;
        PlaySignSequence(new List<AnimationClip> { clip });
        return IsPlaying;
    }

    private IEnumerator PlaybackRoutine(List<AnimationClip> clips)
    {
        foreach (var clip in clips)
        {
            CurrentClip = clip;
            _overrideController[_placeholder.name] = clip;
            float speed = database != null ? database.GetPlaybackSpeed(clip) : 1f;
            targetAnimator.speed = speed;
            // Each sign restarts at frame zero, including repeated letters.
            targetAnimator.Play(SignState, 0, 0f);
            yield return new WaitForSeconds(Mathf.Max(clip.length / speed, 0.01f));
        }
        _playbackRoutine = null;
        IsPlaying = false;
        CurrentClip = null;
        targetAnimator.speed = 1f;
        targetAnimator.CrossFadeInFixedTime(IdleState, crossFadeDuration, 0, 0f);
    }

    public void StopPlayback(bool returnToIdle = true)
    {
        if (_playbackRoutine != null) StopCoroutine(_playbackRoutine);
        _playbackRoutine = null;
        IsPlaying = false;
        CurrentClip = null;
        if (targetAnimator == null) return;
        targetAnimator.speed = 1f;
        if (returnToIdle && IsReady && targetAnimator.isActiveAndEnabled)
            targetAnimator.CrossFadeInFixedTime(IdleState, crossFadeDuration, 0, 0f);
    }

    private void OnDisable() => StopPlayback();

    private void OnDestroy()
    {
        if (_overrideController == null) return;
        if (targetAnimator != null && targetAnimator.runtimeAnimatorController == _overrideController)
            targetAnimator.runtimeAnimatorController = _originalController;
        Destroy(_overrideController);
    }
}
