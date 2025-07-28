using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;


[RequireComponent(typeof(Animator))]
public class ActorAnimation : MonoBehaviour
{
    // 애니메이터
    protected Animator animator;

    // 초기화
    protected virtual void Awake()
    {
        // 애니메이터 컴포넌트 get
        animator = GetComponent<Animator>();

        this.enabled = false; // pause용(코루틴 대용)
    }
    

    AnimatorStateInfo animationState
    {
        get { return animator.GetCurrentAnimatorStateInfo(0); }
    }

    // 레이어 0번에서
    // 현재 재생되고 있는 애니메이션 이름 확인
    public virtual bool CheckAnimationName(string animationStateName)
    { return animationState.IsName(animationStateName); }


    public virtual bool CheckAnimationName(int layer, string animationStateName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.IsName(animationStateName);
    }


    public virtual bool CheckAnimationTime()
    { return animationState.normalizedTime < 1.0f; }

    // SetBool 재생
    public virtual void PlayAnimation(string animationName, bool p_bool)
    { animator.SetBool(animationName, p_bool); }

    // SetTrigger 재생
    public virtual void PlayAnimation(string animationName)
    { animator.SetTrigger(animationName); }




    // 점프 콤보 어택 애니메이션
    // 공중에서는 내려치는 시점에서 정지
    public void PauseWhenJump()
    {
        FootCollider foot = GetComponentInChildren<FootCollider>();

        if (foot != null)
        {
            animator.speed = 0f;

            // 점프 콤보 어택 시 사용할 액션
            System.Action resumeAction = null;
            resumeAction = () => { animator.speed = 1f; foot.ground.Remove(resumeAction); Debug.Log("착지"); };
            foot.ground.Add(resumeAction);
        }
    }


}