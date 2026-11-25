using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewReaction", menuName = "Beast Reaction")]
public class BeastReaction : ScriptableObject
{
    public ReactionType type;
    public BestData bA;
    public BestData bB;

    public float distance = 0.15f;
    public AudioClip clip;

    public bool CanReaction(BestData a, BestData b) 
    {
        return (a == bA && b == bB || a == bB && b == bA);
    }
}
