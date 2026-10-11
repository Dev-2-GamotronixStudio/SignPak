using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [Serializable]
    public struct SignEntry
    {
        public string glossId;          // e.g., "THANK_YOU"
        public string textMatch;        // e.g., "thank you"
        public AnimationClip clip;      // The humanoid animation clip
        public float playbackSpeed;     // Default 1.0f
    }

    [CreateAssetMenu(fileName = "SignDatabase", menuName = "SignPAK/Sign Database")]
    public class SignDatabaseSO : ScriptableObject
    {
        public AnimationClip idleClip;
        public List<SignEntry> signs = new List<SignEntry>();

        private Dictionary<string, SignEntry> _lookupByGloss;
        private Dictionary<string, SignEntry> _lookupByText;

        public SignDatabaseSO (Dictionary <string, SignEntry> lookupByGloss)
        {
            _lookupByGloss = lookupByGloss;
        }

        public void Initialize()
        {
            _lookupByGloss = new Dictionary<string, SignEntry>(StringComparer.OrdinalIgnoreCase);
            _lookupByText = new Dictionary<string, SignEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in signs)
            {
                if (!string.IsNullOrEmpty(entry.glossId))
                    _lookupByGloss[entry.glossId] = entry;

                if (!string.IsNullOrEmpty(entry.textMatch))
                    _lookupByText[entry.textMatch] = entry;
            }
        }

        public bool TryGetClipByText(string text, out AnimationClip clip, out float speed)
        {
            if (_lookupByText == null) Initialize();

            if (_lookupByText != null && _lookupByText.TryGetValue(text, out var entry))
            {
                clip = entry.clip;
                speed = entry.playbackSpeed > 0 ? entry.playbackSpeed : 1.0f;
                return true;
            }

            clip = null;
            speed = 1.0f;
            return false;
        }
    }
}