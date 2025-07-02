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


    // <- 프로퍼티 방식 부활 생각?


    // 초기화
    protected virtual void Awake()
    { animator = GetComponent<Animator>(); }

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