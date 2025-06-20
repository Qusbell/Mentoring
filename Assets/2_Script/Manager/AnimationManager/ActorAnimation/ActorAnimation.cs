using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAnimation : AnimationManager
{
    // � Animation�� ������� üũ
    public bool isMove { protected get; set; } = false;
    public bool isJump { protected get; set; } = false;
    public bool isAttack { protected get; set; } = false;

    public bool isDie { protected get; set; } = false;


    protected override void Awake()
    { base.Awake(); }


    protected override void SetAnimation()
    {
        animator.SetBool("IsMove", isMove);
        animator.SetBool("IsJump", isJump); // <- ���ʹ� ������� ����
        //  animatior.SetBool("IsDie", isDie);  // <- ���� �������� ���� �� ���Ƽ� ��� ����

        if (isAttack)
        {
            animator.SetTrigger("IsAttack");
            isAttack = false;
        }
    }
}