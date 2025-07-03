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



    public RuntimeAnimatorController baseController; // 원본 Animator Controller
    public AnimationClip newAttackClip; // 교체할 애니메이션 클립



    // 초기화
    protected virtual void Awake()
    {
        // 애니메이터 컴포넌트 get
        animator = GetComponent<Animator>();


        // 1. AnimatorOverrideController 생성 및 원본 컨트롤러 할당
        AnimatorOverrideController overrideController = new AnimatorOverrideController(baseController);

        // 2. 기존 애니메이션 클립과 교체할 클립 매핑
        overrideController["Attack"] = newAttackClip; // "Attack"은 원본 컨트롤러의 상태(State) 또는 클립 이름

        // 3. Animator에 오버라이드 컨트롤러 적용
        animator.runtimeAnimatorController = overrideController;
    }

    // 레이어 0번에서
    // 현재 재생되고 있는 애니메이션 이름 확인
    public virtual bool CheckAnimationName(string animationStateName)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName(animationStateName);
    }

    // SetBool 재생
    public virtual void PlayAnimation(string animationName, bool p_bool)
    { animator.SetBool(animationName, p_bool); }

    // SetTrigger 재생
    public virtual void PlayAnimation(string animationName)
    { animator.SetTrigger(animationName); }


    
}