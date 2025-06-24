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
    { animator = GetComponent<Animator>(); }


    // 어떤 Animation을 재생할지 체크


    public bool isMove
    {
        set
        { animator.SetBool("IsMove", value); }
    }

    public bool isJump
    {
        set
        { animator.SetBool("IsJump", value); }
    }

    public bool isAttack
    {
        set
        { if (value) { animator.SetTrigger("IsAttack"); } }
    }

    public bool isDie { protected get; set; } = false;
    //  animatior.SetBool("IsDie", isDie);  // <- 아직 사망모션이 없는 것 같아서 잠시 제외


    // 레이어 0번에서
    // 현재 재생되고 있는 애니메이션 이름 확인
    public virtual bool CheckAnimationName(string animationStateName)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName(animationStateName) && state.normalizedTime < 1.0f;
    }

}