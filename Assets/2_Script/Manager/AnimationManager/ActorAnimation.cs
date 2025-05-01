using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimation : AnimationManager
{
    public bool isMove { get; set; } = false;
    public bool isJump { get; set; } = false;
    public bool isDie { get; set; } = false;
    public bool isAttack { get; set; } = false;


    protected override void Awake()
    { base.Awake(); }


    protected override void SetAnimation()
    {
        animator.SetBool("IsMove", isMove);
        animator.SetBool("IsJump", isJump);
        //  animatior.SetBool("IsDie", isDie);  // <- 아직 사망모션이 없는 것 같아서 잠시 제외

        animator.SetBool("IsAttack", isAttack);
        isAttack = false;
    }
}