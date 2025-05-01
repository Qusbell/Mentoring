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
        //  animatior.SetBool("IsDie", isDie);  // <- ���� �������� ���� �� ���Ƽ� ��� ����

        animator.SetBool("IsAttack", isAttack);
        isAttack = false;
    }
}