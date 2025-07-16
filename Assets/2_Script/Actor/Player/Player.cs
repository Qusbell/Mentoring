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
    protected DashAction dashAction;

    // 생성 초기화
    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<InputManager>();
        jumpAction = GetComponent<JumpAction>();
        dashAction = GetComponent<DashAction>();
    }

    [SerializeField] protected int slowPercentOnAttack = 30; // 공격 중 슬로우 강도
    [SerializeField] protected float slowTimeOnAttack = 2;  // 공격 중 슬로우 시간

    // 프레임당 업데이트
    protected virtual void Update()
    {
        // 이동
        moveAction.moveVec = input.moveVec;
        moveAction.Move();
        animator.PlayAnimation("IsMove", moveAction.isMove);

        // 점프
        if (input.isJumpKeyDown) { jumpAction.Jump(); }
        animator.PlayAnimation("IsJump", jumpAction.isJump);

        // 공격
        if (input.isAttackKeyDown && attackAction.isCanAttack)
        {
            animator.PlayAnimation("DoAttack");
            attackAction.Attack();
            // moveAction.Slow(slowPercentOnAttack, slowTimeOnAttack);
        }

        if (input.isDashKeyDown) { dashAction.Dash(); }
        // <- 대시 애니메이션
    }
}