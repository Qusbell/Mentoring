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

    // [SerializeField] protected int slowPercentOnAttack = 30; // 공격 중 슬로우 강도
    // [SerializeField] protected float slowTimeOnAttack = 2;  // 공격 중 슬로우 시간

    // 프레임당 업데이트
    protected virtual void Update()
    {
        // ----- 입력 -----
        input.SetInput();

        // ----- 이동 -----
        moveAction.moveVec = input.moveVec;
        if (moveAction.isMove)
        {
            moveAction.Move();
            moveAction.Turn();
        }
        animator.PlayAnimation("IsMove", moveAction.isMove);


        // ----- 점프 -----
        if (input.isJumpKeyDown) { jumpAction.Jump(); }
        animator.PlayAnimation("IsJump", jumpAction.isJump);


        // ----- 닷지 -----
        if (input.isDodgeKeyDown && dodgeAction.isCanDash)
        {
            // 닷지 시 공격 활성화
            nowAttackKey = AttackName.Player_WhenDodge;
            attackAction.Attack();

            // Debug.Log("닷지");
            dodgeAction.Dodge();
            animator.PlayAnimation("DoDodge");
        }
        animator.PlayAnimation("IsDodge", dodgeAction.isDodge);


        // ----- 공격 -----
        if (input.isAttackKeyDown)
        {
            // 닷지공격 최우선
            if (dodgeAction.isDodge)
            { nowAttackKey = AttackName.Player_DodgeComboAttack; }

            // 그 후 점프공격 여부 확인
            else if (jumpAction.isJump)
            { nowAttackKey = AttackName.Player_JumpComboAttack; }

            // Basic 공격
            else
            { nowAttackKey = AttackName.Player_BasicAttack; }

            // 공격 가능한 상태라면 : 공격
            if (attackAction.isCanAttack)
            {
                animator.PlayAnimation("DoAttack");
                attackAction.Attack();
            }
            // moveAction.Slow(slowPercentOnAttack, slowTimeOnAttack); // <- 공격 중 슬로우 (였던 것)
        }
    }
}