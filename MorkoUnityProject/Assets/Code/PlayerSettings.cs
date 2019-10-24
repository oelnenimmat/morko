﻿using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "ScriptableObjects/PlayerSettings")]
public class PlayerSettings : ScriptableObject //, IPlayerFields
{
    [Range(1f, 10f)] public float walkSpeed = 5f;
    [Header("Set multiplier to 1 for no effect")]
    [Range(0.1f, 1f)] public float sneakMultiplier = 0.5f;
    [Range(1f, 2f)] public float runMultiplier = 1.5f;
    [Range(0f, 1f)] public float sideMultiplier = 1;
    [Range(0f, 1f)] public float sideRunMultiplier = 1;
    [Range(0f, 1f)] public float backwardMultiplier = 1f;
    [Range(0f, 1f)] public float backwardRunMultiplier = 1f;
    
    
    public float sneakSpeed => walkSpeed * sneakMultiplier;
    public float runSpeed => walkSpeed * runMultiplier;
    public float sideSpeed => walkSpeed * sideMultiplier;
    public float backwardSpeed => walkSpeed * backwardMultiplier;
    public float sideRunSpeed => runSpeed * sideMultiplier;
    public float backwardRunSpeed => runSpeed * backwardMultiplier;

    [Range(0f, 5f)] public float accelerationTime = 1.5f;
    [Range(0f, 5f)] public float decelerationTime = 1.5f;
    [Range(0f, 5f)] public float accelerationRunTime = 1f;
    [Range(0f, 5f)] public float decelerationRunTime = 3f;
    
    public float accelerateSneak => walkSpeed / (accelerationTime + sneakSpeed);
    public float accelerationWalk => walkSpeed / accelerationTime;
    public float accelerationRun => walkSpeed / accelerationRunTime;
    public float decelerationWalk => walkSpeed / decelerationTime;
    public float decelerationRun => walkSpeed / decelerationRunTime;
}