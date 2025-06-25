using System.Collections;
using System.Collections.Generic;
// using System.Numerics; // <- Vector3 모호한 참조 오류
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(PlayerMove))]
[RequireComponent(typeof(InputManager))]
[RequireComponent(typeof(JumpAction))]
[RequireComponent(typeof(MeleeBasicAttack))]
[RequireComponent(typeof(DamageReaction))]
public class Player : Actor
{
    protected InputManager input;
    protected JumpAction jumpAction;

    // 생성 초기화
    protected override void Awake()
    {
        base.Awake();
        input = GetComponent<InputManager>();
        jumpAction = GetComponent<JumpAction>();
    }



    // 프레임당 업데이트
    protected virtual void Update()
    {
        // 이동
        moveAction.moveVec = input.moveVec;
        moveAction.Move();
        animatior.isMove = moveAction.isMove;

        // 점프
        if (input.isJumpKeyDown) { jumpAction.Jump(); }
        animatior.isJump = jumpAction.isJump;

        // 공격
        if (input.isAttackKeyDown)
        {
            animatior.isAttack = attackAction.isCanAttack;
            attackAction.Attack();
        }
    }
}