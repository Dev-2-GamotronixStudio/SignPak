using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ScriptableObjects;
using UnityEngine;

public class TextToSignParser : MonoBehaviour
{
    [SerializeField] private SignDatabaseSO database;

    public List<AnimationClip> ParseTextToClips(string rawInput)
    {
        List<AnimationClip> resolvedClips = new List<AnimationClip>();
        if (string.IsNullOrWhiteSpace(rawInput)) return resolvedClips;

        // Strip punctuation and split into lowercase tokens
        string cleaned = Regex.Replace(rawInput.Trim().ToLower(), @"[^\w\s]", "");
        string[] words = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        int i = 0;
        while (i < words.Length)
        {
            bool matched = false;

            // 1. Try matching 3-word phrase
            if (i + 2 < words.Length)
            {
                string trigram = $"{words[i]} {words[i + 1]} {words[i + 2]}";
                if (database.TryGetClipByText(trigram, out var clip, out _))
                {
                    resolvedClips.Add(clip);
                    i += 3;
                    continue;
                }
            }

            // 2. Try matching 2-word phrase (e.g. "thank you", "good morning")
            if (i + 1 < words.Length)
            {
                string bigram = $"{words[i]} {words[i + 1]}";
                if (database.TryGetClipByText(bigram, out var clip, out _))
                {
                    resolvedClips.Add(clip);
                    i += 2;
                    continue;
                }
            }

            // 3. Fallback to single word
            if (database.TryGetClipByText(words[i], out var singleClip, out _))
            {
                resolvedClips.Add(singleClip);
                matched = true;
            }
            else
            {
                // Optional: Fall back to fingerspelling letters if word is unknown
                Debug.LogWarning($"[TextToSignParser] No sign found for token: {words[i]}");
            }

            i++;
        }

        return resolvedClips;
    }
}