using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class ActorAnimation : ActorAction
{
    // 애니메이터
    protected Animator animator;

    // 초기화
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }
    

    AnimatorStateInfo animationState
    {
        get { return animator.GetCurrentAnimatorStateInfo(0); }
    }

    // 레이어 0번에서
    // 현재 재생되고 있는 애니메이션 이름 확인
    public virtual bool CheckAnimationName(string animationStateName)
    { return animationState.IsName(animationStateName); }

    public virtual bool CheckAnimationTime()
    { return animationState.normalizedTime < 1.0f; }


    // 레이어까지 체크하는
    // 재생 중 애니메이션 이름 확인
    public virtual bool CheckAnimationName(int layer, string animationStateName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.IsName(animationStateName);
    }


    // === 애니메이션 재생 ===

    // SetBool 재생
    public virtual void PlayAnimation(string animationName, bool p_bool)
    { animator.SetBool(animationName, p_bool); }

    // SetTrigger 재생
    public virtual void PlayAnimation(string animationName)
    { animator.SetTrigger(animationName); }



    // === 점프 콤보 어택 애니메이션 이벤트 ===

    private bool isJumpAttackPlag = false;

    // 내려치는 시점 이전에 착지했는지 플래그
    public void CheckWhenJump()
    {
        if (!thisActor.isRand)
        {
            isJumpAttackPlag = true;
            System.Action action = null;
            action = () => { isJumpAttackPlag = false; };
            thisActor.foot.whenGroundEvent.Add(action, 1);
        }
    }

    // 공중에서는 내려치는 시점에서 정지
    public void PauseWhenJump()
    {
        if (isJumpAttackPlag)
        {
            // 애니메이션 멈춤
            animator.speed = 0f;
            // Debug.Log("점프어택 애니메이션 정지");

            // 착지 시 애니메이션 재개
            System.Action resumeAction = null;
            resumeAction = () => { animator.speed = 1f; };

            // 이벤트 추가
            thisActor.foot.whenGroundEvent.Add(resumeAction, 1);
        }
    }


}