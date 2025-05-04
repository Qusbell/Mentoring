using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;



// 공격 행동이 존재한다고 가정하고 작성됨
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AttackAction))]
public class MonsterChaseAction : MoveAction
{
    // 추적할 대상
    [SerializeField] protected Transform target;

    // 회전 속도
    [SerializeField] protected float rotationSpeed = 3f;

    // 네비게이션 AI
    protected NavMeshAgent nav;


    // 생성
    protected override void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();

        // 네비게이션 초기화
        nav = GetComponent<NavMeshAgent>();
        // 이동속도 및 공격 사거리 설정
        nav.speed = moveSpeed;
        nav.stoppingDistance = GetComponent<AttackAction>().attackRange;
    }


    // 목표를 향해 이동
    public override void Move()
    {
        // target 존재하지 않음 예외
        if (target == null)
        { nav.isStopped = true; return; }

        // 목표물 설정
        nav.SetDestination(target.position);


        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
        {
            TurnToTarget();
            // <- 다른 컴포넌트에 알림 메서드
        }
    }


    // 대상을 향해 회전
    protected void TurnToTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}