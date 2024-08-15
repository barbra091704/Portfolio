using System.Collections;
using System.Collections.Generic;
using Core;
using Unity.Netcode;
using UnityEngine;

public class AnimationMove : NetworkBehaviour, IInteractable
{
    public NetworkVariable<bool> isMoving = new(false);
    public NetworkVariable<bool> openState = new(false);
    public Animator animator;
    public bool canMove = true;
    public string OpenStateName;
    public string CloseStateName;
    public float animationTime;

    public void Interact<T>(RaycastHit hit, NetworkObject player, T type = default)
    {
        if (!isMoving.Value && canMove)
        {
            StopCoroutine(DoAnimation());
            StartCoroutine(DoAnimation());
        }  
    }

    private IEnumerator DoAnimation()
    {
        SetIsMovingRpc(true);

        if (openState.Value)
        {
            animator.Play(CloseStateName);
        }
        else
        {
            animator.Play(OpenStateName);
        }

        SetOpenStateRpc(!openState.Value);

        yield return new WaitForSeconds(animationTime);

        SetIsMovingRpc(false);
    }

    [Rpc(SendTo.Server)]
    public void SetIsMovingRpc(bool i)
    {
        isMoving.Value = i;
    }
    [Rpc(SendTo.Server)]
    public void SetOpenStateRpc(bool i)
    {
        openState.Value = i;
    }
}
