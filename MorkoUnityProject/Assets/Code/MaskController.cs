﻿using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MaskController : MonoBehaviourPun
{
    public List<Transform> characterTransforms;
    public NavMeshAgent navMeshAgent;
    
    [Space]
    [Header("Mask Settings")]
    public float secondsBeforeMaskMovesAtStart;
    public float secondsBeforeMaskMovesToNewTarget;
    public float afterCollisionFreezeTime;
    public float startMovementSpeed;
    public float changingMovementSpeed;
    public float acceleration;
    public float jumpMinDistanceFromCharacter;
    [Range(0, 1)]
    public float jumpInterpolationCutOff = 0.9f;
    public float jumpParabolaSize;
    public Transform jumpSmokeEffectOnCollision;
    
    [Space]
    [Header("Animation Speeds")]
    public float moveSpeed = 1f;
    public float walkSpeed = 1f;
    public float runSpeed = 1f;
    public float jumpSpeed = 3f;
    public float breathingSpeed = 1f;

    [Space]
    [Header("Morko Info")]
    [SerializeField]
    public Transform currentMorko;
    [SerializeField]
    public Transform nextMorko;
    private Transform normal;

    private Animator animator;
    private bool startWaitDurationWaited = false;
    private bool lookingForStartingMorko = false;
    private bool maskMovingToNewMorko = false;
    private bool maskJumpingOn = false;
    private bool maskJumpingOff = false;
    private bool maskIsBeingPutOn = false;

    private Vector3 startJumpingPosition;
    private Vector3 targetJumpingPosition;

    private Transform morkoHeadJoint;
    private Transform morkoNeckJoint;

    private PlayerController currenMorkoController;

    public bool startMaskControllerInStartMethod = true;
    public Vector3 maskOnToNeckOffset;
    public Vector3 maskOnToNeckRotation;

    public enum AnimatorBooleans
    {
        Breathing,
        Idle,
        Move,
        Walk,
        WalkSidewaysLeft,
        WalkSidewaysRight,
        WalkBackwards,
        Run
    }

    private void Awake()
    {
        GameManager.RegisterMask(this);
    }

    private void Start()
    {
        navMeshAgent = transform.GetComponent<NavMeshAgent>();
        navMeshAgent.Warp(transform.position);
        animator = transform.GetComponent<Animator>();

        if (startMaskControllerInStartMethod)
            this.InvokeAfter (FindStartingCharacter, secondsBeforeMaskMovesAtStart);
    }

    private void Update()
    {
        if (photonView.IsMine == false)
            return;
        
        if (lookingForStartingMorko && startWaitDurationWaited && !maskJumpingOn && !maskIsBeingPutOn)
            FindStartingCharacter();
        
        else if (maskMovingToNewMorko && !maskJumpingOn && !maskJumpingOff && !maskIsBeingPutOn)
            CheckMaskDistanceFromCharacter(nextMorko);

        else if (maskJumpingOn)
            JumpToHead(nextMorko);
        
        else if (maskJumpingOff)
            JumpOffHead(Vector3.zero);
        
        AnimatorState();
    }
    
    public void StartMaskController()
    {
        this.InvokeAfter (FindStartingCharacter, secondsBeforeMaskMovesAtStart);
    }

    private void FindStartingCharacter()
    {
        startWaitDurationWaited = true;
        lookingForStartingMorko = true;
        nextMorko = FindClosestCharacter();
        
        morkoNeckJoint = nextMorko.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1);
        morkoHeadJoint = morkoNeckJoint.GetChild(0);
        
        MoveMaskToTarget(nextMorko, startMovementSpeed);
        CheckMaskDistanceFromCharacter(nextMorko);
    }

    private Transform FindClosestCharacter()
    {
        float distance = float.MaxValue;
        Transform closestCharacter = characterTransforms[Random.Range(0, characterTransforms.Count)];
        
        foreach (var c in characterTransforms)
        {
            var characterDistance = Vector3.Distance(transform.position, c.position);
            if (characterDistance < distance)
            {
                distance = characterDistance;
                closestCharacter = c;
            }
        }
        return closestCharacter;
    }
    
    public void SwitchMorko(Transform oldMorko, Transform newMorko)
    {
        nextMorko = newMorko;
        currentMorko = null;
        currenMorkoController.isMorko = false;
        currenMorkoController = null;
        
        maskJumpingOn = false;
        maskIsBeingPutOn = false;
        
        nextMorko.GetComponent<Character>().FreezeForSeconds(afterCollisionFreezeTime);
        JumpOffHead(nextMorko.position);
    }
    
    private void MoveMaskToTarget(Transform target, float speed)
    {
        maskMovingToNewMorko = true;
        
        animator.applyRootMotion = true;
        AnimatorState(AnimatorBooleans.Move);
        
        navMeshAgent.enabled = true;
        navMeshAgent.speed = speed;
        navMeshAgent.acceleration = acceleration;
        navMeshAgent.destination = target.position;
    }
    
    private void CheckMaskDistanceFromCharacter(Transform target)
    {
        var distance = Vector3.Distance(transform.position, target.position);
        if (distance <= jumpMinDistanceFromCharacter)
            JumpToHead(target);
    }
    
    private void JumpToHead(Transform toMorko)
    {
        navMeshAgent.enabled = false;

        if (!maskJumpingOn)
            startJumpingPosition = transform.localPosition;
        
        Vector3 toMorkoHeadPosition = morkoHeadJoint.position;
        
        float lengthToTarget = Vector3.Distance(startJumpingPosition, toMorkoHeadPosition);
        float currentLengthToTarget = Vector3.Distance(transform.position, toMorkoHeadPosition);
        float interpolation = (lengthToTarget - currentLengthToTarget) / lengthToTarget;
        
        toMorkoHeadPosition = new Vector3(toMorkoHeadPosition.x, toMorkoHeadPosition.y + (jumpParabolaSize - interpolation * jumpParabolaSize), toMorkoHeadPosition.z);

        transform.position = Vector3.MoveTowards(transform.position, toMorkoHeadPosition, Time.deltaTime * jumpSpeed);
        animator.Play("Attack", 0, interpolation);
        
        maskJumpingOn = true;

        if (interpolation >= jumpInterpolationCutOff)
        {
            maskJumpingOn = false;
            maskIsBeingPutOn = true;
            Instantiate(jumpSmokeEffectOnCollision, transform.position, Quaternion.identity);
            animator.Play("Idle");
            MaskOnNewMorko();
        }
    }
    
    private void JumpOffHead(Vector3 targetLocation)
    {
        navMeshAgent.enabled = false;

        if (!maskJumpingOff)
        {
            MaskOffMorko();
            animator.applyRootMotion = true;
            
            startJumpingPosition = transform.localPosition;

            float distanceToTarget = Vector2.Distance(new Vector2(startJumpingPosition.x, startJumpingPosition.z), new Vector2(targetLocation.x, targetLocation.z));
            if (distanceToTarget <= jumpMinDistanceFromCharacter)
            {
                JumpToHead(nextMorko);
                return;
            }
            
            Vector3 direction = Vector3.Normalize(targetLocation - startJumpingPosition);
            targetJumpingPosition = startJumpingPosition + direction * jumpMinDistanceFromCharacter;
            targetJumpingPosition = new Vector3(targetJumpingPosition.x, -5f, targetJumpingPosition.z);
        }
        
        float lengthToTarget = Vector3.Distance(startJumpingPosition, targetJumpingPosition);
        float currentLengthToTarget = Vector3.Distance(transform.position, targetJumpingPosition);
        float interpolation = (lengthToTarget - currentLengthToTarget) / lengthToTarget;

        transform.position = Vector3.MoveTowards(transform.position, targetJumpingPosition, Time.deltaTime * jumpSpeed);
        animator.Play("Roar", 0, interpolation);
        
        maskJumpingOff = true;

        if (interpolation >= jumpInterpolationCutOff)
        {
            maskJumpingOff = false;
            animator.Play("Idle");
            this.InvokeAfter(() => MoveMaskToTarget(nextMorko, changingMovementSpeed), secondsBeforeMaskMovesToNewTarget);
        }
    }
    
    public void MaskOffMorko()
    {
        transform.parent = null;
        var direction = (nextMorko.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        morkoHeadJoint.localScale = Vector3.one;
        
        morkoNeckJoint = nextMorko.transform.GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(1);
        morkoHeadJoint = morkoNeckJoint.GetChild(0);
    }
    
    public void MaskOnNewMorko()
    {
        navMeshAgent.enabled = false;

        lookingForStartingMorko = false;
        maskMovingToNewMorko = false;
        maskJumpingOn = false;

        animator.applyRootMotion = false;
        morkoHeadJoint.localScale = Vector3.zero;
        transform.forward = nextMorko.forward;
        transform.parent = morkoNeckJoint;
        transform.localPosition = maskOnToNeckOffset;
        transform.forward = nextMorko.forward;
        transform.localRotation = Quaternion.Euler(maskOnToNeckRotation);

        AnimatorState(AnimatorBooleans.Idle);
        
        currentMorko = nextMorko;
        currenMorkoController = currentMorko.GetComponent<PlayerController>();
        currenMorkoController.isMorko = true;
    }
    
    private void ResetAnimatorTriggers()
    {
        animator.ResetTrigger("Roar");
        animator.ResetTrigger("JumpToHead");
        animator.ResetTrigger("JumpOffHead");
    }

    public void AnimatorState(AnimatorBooleans state = default)
    {
        ResetAnimatorTriggers();
        animator.SetBool("Idle", false);
        animator.SetBool("Breathing", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetBool("Move", false);
        animator.SetBool("WalkSidewaysLeft", false);
        animator.SetBool("WalkSidewaysRight", false);
        animator.SetBool("WalkBackwards", false);

        if (currenMorkoController)
        {
            var morkoAnimation = currenMorkoController.currentAnimation;
            switch (morkoAnimation)
            {
                case PlayerController.AnimatorState.Idle:
                    animator.speed = breathingSpeed;
                    animator.SetBool("Idle", true);
                    break;
                case PlayerController.AnimatorState.Walk:
                    animator.speed = walkSpeed;
                    animator.SetBool("Walk", true);
                    break;
                case PlayerController.AnimatorState.WalkSidewaysLeft:
                    animator.speed = runSpeed;
                    animator.SetBool("WalkSidewaysLeft", true);
                    break;
                case PlayerController.AnimatorState.WalkSidewaysRight:
                    animator.speed = runSpeed;
                    animator.SetBool("WalkSidewaysRight", true);
                    break;
                case PlayerController.AnimatorState.WalkBackwards:
                    animator.speed = runSpeed;
                    animator.SetBool("WalkBackwards", true);
                    break;
                case PlayerController.AnimatorState.Run:
                    animator.speed = runSpeed;
                    animator.SetBool("Run", true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            switch (state)
            {
                case AnimatorBooleans.Breathing:
                    animator.speed = breathingSpeed;
                    animator.SetBool("Breathing", true);

                    break;
                case AnimatorBooleans.Move:
                    animator.speed = moveSpeed;
                    animator.SetBool("Move", true);

                    break;
                default:
                    animator.speed = breathingSpeed;
                    animator.SetBool("Breathing", true);
                    break;
            } 
        }
    }
}