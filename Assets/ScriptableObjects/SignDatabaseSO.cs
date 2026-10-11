using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [Serializable]
    public struct SignEntry
    {
        public string glossId;
        public string textMatch;
        public AnimationClip clip;
        public float playbackSpeed;
    }

    [CreateAssetMenu(fileName = "SignDatabase", menuName = "SignPAK/Sign Database")]
    public class SignDatabaseSO : ScriptableObject
    {
        public AnimationClip idleClip;
        public List<SignEntry> signs = new List<SignEntry>();
        private Dictionary<string, SignEntry> _lookupByGloss;
        private Dictionary<string, SignEntry> _lookupByText;

        private void OnEnable() => InvalidateLookup();
        private void OnValidate() => InvalidateLookup();

        private void InvalidateLookup()
        {
            _lookupByGloss = null;
            _lookupByText = null;
        }

        public void Initialize()
        {
            _lookupByGloss = new Dictionary<string, SignEntry>(StringComparer.OrdinalIgnoreCase);
            _lookupByText = new Dictionary<string, SignEntry>(StringComparer.OrdinalIgnoreCase);
            if (signs == null) return;
            foreach (var entry in signs)
            {
                if (!string.IsNullOrWhiteSpace(entry.glossId))
                    _lookupByGloss[entry.glossId.Trim()] = entry;
                if (!string.IsNullOrWhiteSpace(entry.textMatch))
                    _lookupByText[entry.textMatch.Trim()] = entry;
            }
        }

        public bool TryGetClipByText(string text, out AnimationClip clip, out float speed)
        {
            clip = null;
            speed = 1f;
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (_lookupByText == null) Initialize();
            if (!_lookupByText.TryGetValue(text.Trim(), out var entry) || entry.clip == null)
                return false;
            clip = entry.clip;
            speed = ValidSpeed(entry.playbackSpeed);
            return true;
        }

        public bool TryGetClipByGloss(string gloss, out AnimationClip clip)
        {
            clip = null;
            if (string.IsNullOrWhiteSpace(gloss)) return false;
            if (_lookupByGloss == null) Initialize();
            if (!_lookupByGloss.TryGetValue(gloss.Trim(), out var entry) || entry.clip == null)
                return false;
            clip = entry.clip;
            return true;
        }

        public float GetPlaybackSpeed(AnimationClip clip)
        {
            if (signs != null)
                foreach (var entry in signs)
                    if (entry.clip == clip) return ValidSpeed(entry.playbackSpeed);
            return 1f;
        }

        private static float ValidSpeed(float speed) =>
            speed > 0 && !float.IsNaN(speed) && !float.IsInfinity(speed) ? speed : 1f;
    }
}
