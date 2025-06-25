using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ActorAnimation))]
[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DamageReaction))]
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


    // 공격 사거리 계산
    bool InAttackRange()
    { return (target.position - this.transform.position).sqrMagnitude <= attackAction.attackRange * attackAction.attackRange; }


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


    // 공격, 리로드 애니메이션 재생 추가 확인
    bool doAttack = false;
    bool doReload = false;

    // 공격 상태
    private void AttackStatus()
    {
        // 공격 가능하다면
        if (InAttackRange() && attackAction.isCanAttack)
        {
            attackAction.Attack();
            animatior.isAttack = true; // 어택 애니메이션 재생
        }

        if (animatior.CheckAnimationName("Attack"))
        { doAttack = true; }

        if (doAttack && !animatior.CheckAnimationName("Attack"))
        {
            doAttack = false;
            actionStatus = ReloadStatus; // 공격 후딜레이로 이행 }
        }
    }


    // 공격 후딜레이 애니메이션 재생
    private void ReloadStatus()
    {
        if (animatior.CheckAnimationName("Reload"))
        { doReload = true; }
        
        if (doReload && !animatior.CheckAnimationName("Reload"))
        {
            doReload = false;
            actionStatus = IdleStatus;
        }
    }
}