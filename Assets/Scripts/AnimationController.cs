using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private AnimationQueuePlayer queuePlayer;

    private void Start()
    {
        // Example 1: Passing a string array
        string[] combo = { "Attack1", "Attack2", "Kick", "VictoryPose" };
        queuePlayer.PlayAnimationSequence(combo);

        // Example 2: Passing a List<string>
        List<string> cutscene = new List<string> { "Wave", "Talk", "Bow" };
        // queuePlayer.PlayAnimationSequence(cutscene);
    }
}

[Serializable]
public struct Animation
{
    private int animHash;
    public string animationName;
    public AnimationClip animation;
}
