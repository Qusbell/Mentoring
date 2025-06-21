using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class ChaseAction : MoveAction
{
    // 추적할 대상
    protected Transform target;

    // 네비게이션 AI
    protected NavMeshAgent nav;


    // 생성
    protected override void Awake()
    {
        base.Awake();

        // 네비게이션 초기화
        nav = GetComponent<NavMeshAgent>();
        // 이동속도 설정
        nav.speed = moveSpeed;

        // 타겟 설정
        target = TargetManager.instance.target;
    }


    // ===== 이동 =====

    // 목표를 향해 이동
    public override void Move()
    {
        // target 존재하지 않음 예외
        if (target == null)
        {
            isMove = false;
            nav.isStopped = true;
            return;
        }

        // 목표물 설정
        nav.SetDestination(target.position);
        nav.isStopped = false;

        // 이동 중인지 확인
        CheckMove();

        // 제자리 상태일 때 회전
        if (!isMove)
        { TurnToTarget(); }
    }

    // move 상태 확인
    void CheckMove()
    {
        isMove = nav.hasPath &&
            !nav.pathPending &&
            nav.stoppingDistance < nav.remainingDistance;
    }


    // 회전 속도
    [SerializeField] protected float rotationSpeed = 3f;

    // 대상을 향해 회전
    protected void TurnToTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}