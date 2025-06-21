using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Monster : Actor
{
    // 현재 수행 중인 행동
    protected Action actionStatus;

    // <- 스폰 애니메이션 재생 메서드


    protected override void Awake()
    {
        base.Awake();
        // <- actionStatus = Spawn (스폰 애니메이션 메서드 대입)
    }


    private void Update()
    {
        actionStatus();
    }

    // 이동 상태
    private void MoveStatus()
    {
        moveAction.Move();
        if (!moveAction.isMove &&
            attackAction.isCanAttack)
        { actionStatus = AttackStatus; }
    }

    // 공격 상태
    private void AttackStatus()
    {
        attackAction.Attack();
        if (attackAction.isCanAttack) // 공격 쿨타임 이후부터 다시 이동 가능 (임시 조치)
        { actionStatus = MoveStatus; }
    }
}