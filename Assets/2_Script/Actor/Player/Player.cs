using System.Collections;
using System.Collections.Generic;
// using System.Numerics; // <- Vector3 모호한 참조 오류
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(JumpAction))]
[RequireComponent(typeof(BasicWeaponAttack))]
[RequireComponent(typeof(DamageReaction))]
public class Player : Actor
{
    protected InputManager input;
    protected JumpAction jumpAction;
    protected DodgeAction dodgeAction;

    // 생성 초기화
    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<InputManager>();
        jumpAction = GetComponent<JumpAction>();
        dodgeAction = GetComponent<DodgeAction>();

         // <- 기본 공격
    }



    protected bool IsAttackAnimationPlaying()
    {
        return animator.CheckAnimationName("Attack") ||
            animator.CheckAnimationName("Attack_Dodge") ||
            animator.CheckAnimationName("Attack_Jump");
    }



    // 프레임당 업데이트
    protected virtual void Update()
    {
        // ----- 입력 -----
        input.SetInput();

        // ----- 이동 -----
        if (input.isMoveKeyDown && !dodgeAction.isDodge)
        {
            moveAction.moveVec = input.moveVec;
            moveAction.isMove = true;
            moveAction.Move();
            moveAction.Turn();
        }
        else { moveAction.isMove = false; }


        // ----- 점프 -----
        if (input.isJumpKeyDown)
        { jumpAction.Jump(); }


        // ----- 닷지 -----
        if (input.isDodgeKeyDown && dodgeAction.isCanDash)
        {
            // 닷지 시 공격 활성화
            nowAttackKey = AttackName.Player_WhenDodge;
            attackAction.Attack();

            // Debug.Log("닷지");
            animator.PlayAnimation("DoDodge");  // 애니메이션 트리거
            dodgeAction.Dodge();
        }


        // ----- 공격 -----
        if (input.isAttackKeyDown) // 키 누름 체크
        {
            // 닷지 중 공격 최우선
            if (dodgeAction.isDodge)
            { nowAttackKey = AttackName.Player_DodgeComboAttack; }

            // 그 후 점프 중 공격 여부 확인
            else if (jumpAction.isJump)
            { nowAttackKey = AttackName.Player_JumpComboAttack; }

            // Basic 공격
            else
            { nowAttackKey = AttackName.Player_BasicAttack; }

            // 공격 가능한 상태라면 : 실제 공격 발생
            if (attackAction.isCanAttack && !IsAttackAnimationPlaying())
            {
                animator.PlayAnimation("DoAttack");  // 애니메이션 트리거
                attackAction.Attack();
            }
        }

        // ----- bool 애니메이션 처리 -----
        animator.PlayAnimation("IsMove", moveAction.isMove);
        animator.PlayAnimation("IsJump", jumpAction.isJump);
        animator.PlayAnimation("IsDodge", dodgeAction.isDodge);
    }
}