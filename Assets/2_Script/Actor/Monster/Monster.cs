using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Monster : Actor
{
    // 현재 수행 중인 행동
    protected Action actionStatus;

    // 타겟
    Transform target;

    // <- 스폰 애니메이션 재생 메서드
    // 애니메이션 재생이 끝난 후, MoveStatus로 전환


    protected override void Awake()
    {
        base.Awake();
        actionStatus = MoveStatus;
        // <- actionStatus = SpawnStatus (스폰 애니메이션 메서드 대입, 임시로 MoveStatus 대입 중)
    }

    private void Start()
    { target = TargetManager.instance.Targeting(); }

    private void Update()
    { actionStatus(); }


    // 공격 범위 내부라면
    // <- 정밀계산 필요 X. 이후 제곱 >= 제곱 비교 형태로 최적화 가능
    bool InAttackRange()
    { return attackAction.attackRange >= Vector3.Distance(target.position, this.transform.position); }


    // 이동 상태
    private void MoveStatus()
    {
        moveAction.Move();
        if (InAttackRange() && // <- InAttackRange 판정으로 바꾸기
            attackAction.isCanAttack)
        { actionStatus = AttackStatus; }
    }

    // 공격 상태
    private void AttackStatus()
    {
        // 공격 범위 내에 있다면
        if (InAttackRange())
        {
            attackAction.Attack(); // 공격
            // <- 공격 후딜레이 상태로 전환
        }

        // 공격 범위 내에 없고
        // <- attackAfterDelay 등 추가 조건
        else if (attackAction.isCanAttack) // 공격 쿨타임 이후부터 다시 이동 가능 (임시 조치)
        { actionStatus = MoveStatus; } // <- 다시 이동
    }



    // <- SpawnStatus

    // <- IdleStatus

    // <- ReloadStatus
}