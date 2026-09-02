using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using Data.ValueObject;
using Data.UnityObject;

public class PlayerAnimationController : MonoBehaviour
{
    #region Self Variables

    #region Serialized Variables

    [SerializeField] private Animator animator;

    #endregion
    #endregion

    public void OnChangeAnimation(PlayerAnimationStates nextAnimation)
    {
        OnResetAnimation(nextAnimation);
        animator.speed = 0.5f;
        animator.SetTrigger(nextAnimation.ToString());
    }

    public void OnResetAnimator()
    {
        animator.enabled = false;
        animator.enabled = true;
    }

    public void OnResetAnimation(PlayerAnimationStates animation)
    {
        animator.ResetTrigger(animation.ToString());
    }

    public void OnRestartLevel()
    {
        animator.speed = 0.5f;
    }
}
