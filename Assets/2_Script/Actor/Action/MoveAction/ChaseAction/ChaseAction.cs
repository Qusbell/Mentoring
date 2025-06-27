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
    // ----- ai 부분 -----

    // 네비게이션 AI
    protected NavMeshAgent nav;

    // 추적할 대상
    protected Transform target;


    // 생성
    protected override void Awake()
    {
        base.Awake();
        // 네비게이션 초기화
        nav = GetComponent<NavMeshAgent>();
        // 이동속도 설정
        nav.speed = moveSpeed;

        // 위치, 회전 자동 업데이트 비활성
        nav.updatePosition = false;
        nav.updateRotation = false;
    }

    protected virtual void Start()
    { SetTarget(TargetManager.instance.Targeting()); }


    // 타겟 설정
    public void SetTarget(Transform p_target)
    { target = p_target; }


    // target이 입력한 거리 이내에 있는지 확인
    public bool InDistance(int distance)
    { return (this.transform.position - target.position).sqrMagnitude <= distance * distance; }

    public bool InDistance(float distance)
    { return (this.transform.position - target.position).sqrMagnitude <= distance * distance; }


    // 목적지 갱신
    void UpdateDestination()
    {
        if (target != null) { nav.SetDestination(target.position); }
        else
        {
            Debug.Log("target 부재 중 : " + gameObject.name);
            nav.SetDestination(this.transform.position);
        }
    }

    public override void Move()
    {
        // 타겟이 존재하는 경우에만 move
        if(target != null)
        { base.Move(); }
    }

    // 다음 이동 방향
    void UpdateNextMoveDirection()
    { moveVec = nav.desiredVelocity.normalized; }

    // 네비게이션 위치와 자신 위치 동기화
    void UpdateMyPositionOnNav()
    { if (nav.isOnNavMesh) { nav.nextPosition = rigid.position; } }


    // 회전 속도
    [SerializeField] protected float rotationSpeed = 3f;

    // 다음 진행 방향을 향해 회전 (느리게)
    protected override void Turn()
    {
        Vector3 direction = moveVec;
        if (moveVec == Vector3.zero)
        {
            direction = target.position - transform.position;
            direction.y = 0;
        }
        else
        { direction = moveVec; }

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void Update()
    {
        UpdateDestination();
        UpdateNextMoveDirection(); // 다음 방향 설정
        UpdateMyPositionOnNav();   // 네비게이션 갱신
        Turn();                    // 회전
    }
}