using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class AlphabetSequenceTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SignDatabaseSO database;
    [SerializeField] private SignPlaybackController playbackController;
    [Header("Sequence Range")]
    [SerializeField] private int startGlossId = 1;
    [SerializeField] private int endGlossId = 26;
    [Header("Playback Settings")]
    [SerializeField, Min(0)] private float delayBetweenSigns = 0.3f;

    private int _currentManualIndex;
    private string _statusText = "Choose a letter, or press Space to play A-Z.";
    private string _input = "ABC";
    private bool _editingText;
    private Coroutine _sequenceRoutine;

    private void Start()
    {
        _currentManualIndex = startGlossId - 1;
        if (database != null) database.Initialize();
        if (playbackController == null)
            _statusText = "Assign the character's SignPlaybackController in the Inspector.";
    }

    private void Update()
    {
        if (_editingText) return;
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.spaceKey.wasPressedThisFrame) PlayAllSequentially();
        if (keyboard.rightArrowKey.wasPressedThisFrame) PlayNext();
        if (keyboard.leftArrowKey.wasPressedThisFrame) PlayPrevious();
        if (keyboard.escapeKey.wasPressedThisFrame) StopTesting();
#elif ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space)) PlayAllSequentially();
        if (Input.GetKeyDown(KeyCode.RightArrow)) PlayNext();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) PlayPrevious();
        if (Input.GetKeyDown(KeyCode.Escape)) StopTesting();
#endif
    }

    private bool CanPlay()
    {
        if (!Application.isPlaying || database == null || playbackController == null ||
            !playbackController.isActiveAndEnabled)
        {
            _statusText = "Enter Play mode and assign an active character and database.";
            return false;
        }
        if (!playbackController.IsReady)
        {
            _statusText = "Character playback setup is invalid. See Console for details.";
            return false;
        }
        return true;
    }

    [ContextMenu("Play All (A-Z)")]
    public void PlayAllSequentially()
    {
        if (!CanPlay()) return;
        StopTesting();
        _sequenceRoutine = StartCoroutine(PlayFullSequenceRoutine());
    }

    private IEnumerator PlayFullSequenceRoutine()
    {
        for (int id = startGlossId; id <= endGlossId; id++)
        {
            if (!database.TryGetClipByGloss(id.ToString(), out var clip)) continue;
            _currentManualIndex = id;
            _statusText = "Playing " + clip.name + " on " + playbackController.TargetAnimator.name;
            playbackController.PlaySignSequence(new List<AnimationClip> { clip });
            while (playbackController.IsPlaying) yield return null;
            if (delayBetweenSigns > 0) yield return new WaitForSeconds(delayBetweenSigns);
        }
        _statusText = "Alphabet sequence completed.";
        _sequenceRoutine = null;
    }

    public void PlayNext()
    {
        int next = _currentManualIndex + 1;
        if (next > endGlossId || next < startGlossId) next = startGlossId;
        PlaySingleById(next);
    }

    public void PlayPrevious()
    {
        int previous = _currentManualIndex - 1;
        if (previous < startGlossId || previous > endGlossId) previous = endGlossId;
        PlaySingleById(previous);
    }

    public void PlaySingleById(int id)
    {
        if (!CanPlay()) return;
        StopTesting();
        if (!database.TryGetClipByGloss(id.ToString(), out var clip))
        {
            _statusText = "No sign for ID " + id;
            return;
        }
        _currentManualIndex = id;
        _statusText = "Playing " + clip.name + " on " + playbackController.TargetAnimator.name;
        playbackController.PlaySignSequence(new List<AnimationClip> { clip });
    }

    public void PlayTypedLetters()
    {
        if (!CanPlay()) return;
        var clips = new List<AnimationClip>();
        foreach (char letter in _input)
        {
            if (char.IsWhiteSpace(letter)) continue;
            if (!database.TryGetClipByText(letter.ToString(), out var clip, out _))
            {
                _statusText = "No alphabet clip for: " + letter;
                return;
            }
            clips.Add(clip);
        }
        if (clips.Count == 0) return;
        StopTesting();
        _statusText = "Playing letters: " + _input.ToUpperInvariant();
        playbackController.PlaySignSequence(clips);
    }

    public void StopTesting()
    {
        if (_sequenceRoutine != null) StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = null;
        if (playbackController != null) playbackController.StopPlayback();
    }

    private void OnDisable() => StopTesting();

    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.box) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
        GUILayout.BeginArea(new Rect(20, 20, 540, 255), GUI.skin.box);
        GUILayout.Box("PSL Alphabet Tester (A-Z)\n" + _statusText, style);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous (Left)")) PlayPrevious();
        if (GUILayout.Button("Play All (Space)")) PlayAllSequentially();
        if (GUILayout.Button("Next (Right)")) PlayNext();
        if (GUILayout.Button("Stop (Esc)")) StopTesting();
        GUILayout.EndHorizontal();
        for (int row = 0; row < 3; row++)
        {
            GUILayout.BeginHorizontal();
            for (int column = 0; column < 9; column++)
            {
                int index = row * 9 + column;
                if (index >= 26) break;
                if (GUILayout.Button(((char)('A' + index)).ToString(), GUILayout.Height(28)))
                    PlaySingleById(index + 1);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("AlphabetInput");
        _input = GUILayout.TextField(_input);
        if (GUILayout.Button("Play Letters", GUILayout.Width(100)))
        {
            GUI.FocusControl(null);
            PlayTypedLetters();
        }
        GUILayout.EndHorizontal();
        _editingText = GUI.GetNameOfFocusedControl() == "AlphabetInput";
        GUILayout.EndArea();
    }
}
