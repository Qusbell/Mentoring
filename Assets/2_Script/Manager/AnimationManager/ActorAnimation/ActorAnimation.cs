using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimation : AnimationManager
{
    // 어떤 Animation을 재생할지 체크
    public bool isMove { protected get; set; } = false;
    public bool isJump { protected get; set; } = false;
    public bool isAttack { protected get; set; } = false;

    public bool isDie { protected get; set; } = false;


    protected override void Awake()
    { base.Awake(); }


    protected override void SetAnimation()
    {
        animator.SetBool("IsMove", isMove);
        animator.SetBool("IsJump", isJump); // <- 몬스터는 점프모션 없음
        //  animatior.SetBool("IsDie", isDie);  // <- 아직 사망모션이 없는 것 같아서 잠시 제외

        if (isAttack)
        {
            animator.SetTrigger("IsAttack");
            isAttack = false;
        }
    }
}