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




    protected override void Awake()
    {
        base.Awake();
        actionStatus = SpawnState; // 생성부터 시작
    }

    private void Start()
    { target = TargetManager.instance.Targeting(); }

    private void Update()
    { actionStatus(); }

    // 공격 범위 내부 계산
    // <- 정밀계산 필요 X. 이후 제곱 >= 제곱 비교 형태로 최적화 가능
    bool InAttackRange()
    { return attackAction.attackRange >= Vector3.Distance(target.position, this.transform.position); }



    // 생성 상태
    private void SpawnState()
    {
        if (!animatior.CheckAnimationName("Spawn")) // 스폰 애니메이션 종료 시
        { actionStatus = IdleStatus; } // 대기
    }

    // 대기 상태
    private void IdleStatus()
    {
        if (InAttackRange())  // 공격 가능 상태라면
        { actionStatus = AttackStatus; } // 공격으로
        else
        { actionStatus = MoveStatus; }  // 아니면 이동
    }


    // 이동 상태
    private void MoveStatus()
    {
        Debug.Log("Move 상태");

        if (InAttackRange())
        {
            animatior.isMove = false;
            moveAction.isMove = false;
            actionStatus = AttackStatus;
        }
        else
        {
            animatior.isMove = true;
            moveAction.isMove = true;
            moveAction.Move();
        }
    }


    // 공격 상태
    private void AttackStatus()
    {
        // 공격 가능하다면
        if (attackAction.isCanAttack)
        {
            attackAction.Attack(); // 공격
            animatior.isAttack = true; // 어택 애니메이션 재생
        }

        // 공격 가능한 상태가 아니고
        // 공격 애니메이션 재생 중이 아니라면
        else if (!animatior.CheckAnimationName("Attack"))
        { actionStatus = ReloadStatus; } // 공격 후딜레이로 이행
    }


    // 공격 후딜레이 애니메이션 재생
    private void ReloadStatus()
    {
        if (!animatior.CheckAnimationName("Reload"))
        { actionStatus = IdleStatus; }
    }
}