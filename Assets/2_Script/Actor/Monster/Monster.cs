using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Monster : Actor
{
    // 네비게이션 ai
    protected NavMeshAgent nav;

    // 타겟 위치
    protected Transform target;


    protected override void Awake()
    {
        base.Awake();
        nav = GetComponent<NavMeshAgent>();

        // <- 타겟 (플레이어) 감지
    }


    protected virtual void Update()
    {
        moveAction.Move();

        // <- target이 존재하지 않는 동안, 무한 공격하는 문제
        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance) // <- 애니메이션 동안 적용 X
        {
            attackAction.Attack();
            // <- 애니메이션 적용
        }
    }
}