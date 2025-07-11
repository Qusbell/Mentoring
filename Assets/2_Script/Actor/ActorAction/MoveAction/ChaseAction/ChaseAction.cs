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
    { SetTarget(TargetManager.instance.target); }


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
        // nav상 위치와 transform 위치의 괴리
        float gapDistance = (nav.nextPosition - transform.position).sqrMagnitude;

        if (!nav.isOnNavMesh || 0.1f < gapDistance)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(this.transform.position, out hit, 1f, NavMesh.AllAreas))
            {
                //Debug.Log(this.gameObject.name + "네비메쉬로 정상 되돌아옴");
                nav.Warp(hit.position);
            }
        }

        if (target != null && nav.isOnNavMesh)
        { nav.SetDestination(target.position); }
    }


    public override void Move()
    {
        // 타겟이 존재하고,
        // 길이 존재하는 경우에만 move
        if (target != null && nav.hasPath && nav.pathStatus == NavMeshPathStatus.PathComplete)
        {
            isMove = true;
            base.Move();
        }
        else
        { isMove = false; }
    }

    // 다음 이동 방향
    void UpdateNextMoveDirection()
    { moveVec = nav.desiredVelocity.normalized; }

    // 네비게이션 위치와 자신 위치 동기화
    void UpdateMyPositionOnNav()
    { 
        if (nav.isOnNavMesh) { nav.nextPosition = rigid.position; }
    }

    public bool isCanChase
    {
        get { return CanChaseTarget(); }
    }

    // 대상을 추격할 수 있는 상태인지 확인하는 메서드
    protected bool CanChaseTarget()
    {
        if (nav == null || target == null)
        { return false; }

        // NavMeshAgent가 활성화되어 있고, 경로가 유효한지 확인
        if (!nav.isActiveAndEnabled)
        { return false; }

        NavMeshPath path = new NavMeshPath();
        bool hasPath = nav.CalculatePath(target.position, path);

        // 경로가 존재하고, 경로 상태가 완성되었으며, 경로 길이가 0보다 크면 추격 가능
        if (hasPath && path.status == NavMeshPathStatus.PathComplete && path.corners.Length > 1)
        { return true; }

        return false;
    }



    // 회전 속도
    [SerializeField] protected float rotationSpeed = 3f;

    // 다음 진행 방향을 향해 회전 (느리게)
    public override void Turn()
    {
        Vector3 direction = moveVec;
        if (moveVec == Vector3.zero)
        {
            direction = target.position - transform.position;
            direction.y = 0;
        }

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void Update()
    {
        UpdateDestination();       // 목적지 확인
        UpdateNextMoveDirection(); // 다음 방향 설정
        UpdateMyPositionOnNav();   // 자신 위치 갱신
    }
}