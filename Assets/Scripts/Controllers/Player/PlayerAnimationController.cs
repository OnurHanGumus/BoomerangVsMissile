using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class PlayerAnimationController : MonoBehaviour
{
    #region Self Variables

    #region Serialized Variables

    [SerializeField] private Animator animator;

    #endregion
    #region Private Variables

    #endregion
    #endregion

    public void OnChangeAnimation(PlayerAnimationStates nextAnimation)
    {
        animator.speed = 0.5f;
        animator.SetTrigger(nextAnimation.ToString());
        animator.speed = 0.5f;
    }
    public void OnChangeAnimationSpeed()
    {
        animator.speed += 0.5f;
    }
}
