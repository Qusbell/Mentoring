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
    public Transform target;

    // 길
    private NavMeshPath navPath;

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

        navPath = new NavMeshPath();
    }

    protected void Start()
    {
        SetTarget(TargetManager.Instance.target);
        Timer.Instance.StartEndlessTimer(this, "시작", 0.2f, UpdateDestination); // 목적지 업데이트
    }


    // 타겟 설정
    public void SetTarget(Transform p_target)
    { target = p_target; }


    // target이 입력한 거리 이내에 있는지 확인
    public bool InDistance(int distance)
    { return (this.transform.position - target.position).sqrMagnitude <= distance * distance; }

    public bool InDistance(float distance)
    { return (this.transform.position - target.position).sqrMagnitude <= distance * distance; }


    // 목적지 갱신 (기본 버전)
    // void UpdateDestination()
    // {
    //     if (target == null || nav == null || !nav.isOnNavMesh) { return; }
    //     nav.SetDestination(target.position);
    // }

    // 목적지 갱신
    //  private void UpdateDestination()
    //  {
    //      if (target == null || nav == null || !nav.isOnNavMesh) { return; }
    //  
    //      NavMeshPath navPath = new NavMeshPath();
    //  
    //      if (NavMesh.CalculatePath(nav.transform.position, target.position, NavMesh.AllAreas, navPath)
    //          && navPath.status == NavMeshPathStatus.PathComplete)
    //      { nav.SetPath(navPath); }
    //      
    //  }

    // 즉각 반응 AI
    private void UpdateDestination()
    {
        if (target == null || nav == null || !nav.isOnNavMesh) { return; }
    
        NavMeshHit hit;
        Vector3 targetPosOnNav = nav.transform.position;
    
        // target 주변 최대 30유닛 반경 내에서
        // NavMesh 위의 가장 가까운 점을 찾음
        if (NavMesh.SamplePosition(target.position, out hit, 30.0f, NavMesh.AllAreas))
        { targetPosOnNav = hit.position; }
    
        // 목적지로 보정된 targetPosOnNav를 넣어 경로 계산
        if (NavMesh.CalculatePath(nav.transform.position, targetPosOnNav, NavMesh.AllAreas, navPath)
            && navPath.status == NavMeshPathStatus.PathComplete)
        { nav.SetPath(navPath); }
    }


    public override void Move()
    {
        // 타겟이 존재하고,
        // 길이 존재하는 경우에만 move
        if (target != null && nav.hasPath && nav.pathStatus == NavMeshPathStatus.PathComplete)
        { base.Move(); }
    }


    // 다음 이동 방향
    void UpdateNextMoveDirection()
    { moveVec = nav.desiredVelocity.normalized; }


    // 네비게이션 위치와 자신 위치 동기화
    void UpdateMyPositionOnNav()
    {
        // nav상 위치와 transform 위치의 괴리
        float gapDistance = (nav.nextPosition - transform.position).sqrMagnitude;

        // 자신이 navMesh 위에 없는 경우
        // <- 수정 예정
        if (!nav.isOnNavMesh || 0.1f < gapDistance)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(this.transform.position, out hit, 1f, NavMesh.AllAreas))
            { nav.Warp(hit.position); } // 강제로 navMesh 위로 되돌아오기
        }

        if (nav.isOnNavMesh) { nav.nextPosition = rigid.position; }
    }



    // 회전 속도
    [SerializeField] protected float rotationSpeed = 3f;

    // 다음 진행 방향을 향해 회전 (느리게)
    public override void Turn()
    {
        Vector3 direction = moveVec;

        // moveVec이 0인 경우의 회전
        if (moveVec == Vector3.zero)
        {
            direction = target.position - transform.position;
            direction.y = 0;
        }

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }


    public bool IsFacingTarget(float maxDistance = 100f, float tolerance = 0.99f)
    {
        Vector3 toTarget = (target.position - transform.position);
        toTarget.y = 0f; // y값 무시
        toTarget.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f; // y값 무시
        forward.Normalize();

        float dot = Vector3.Dot(forward, toTarget);

        return dot >= tolerance && isClearToTarget(maxDistance);
    }

    // target까지 장애물 여부 판별
    private bool isClearToTarget(float rayDistance = 100f)
    {
        if (target == null) { return false; }

        Vector3 directionToTarget = (target.position + Vector3.down * 2f - transform.position).normalized;
        Ray ray = new Ray(transform.position + Vector3.up * 2f, directionToTarget);

        // "Cube"와 "Target" 레이어만 포함하는 레이어마스크 생성
        int cubeLayer = 1 << LayerMask.NameToLayer("Cube");
        int playerLayer = 1 << LayerMask.NameToLayer("Player");
        int layerMask = cubeLayer | playerLayer;

        // 디버그용 레이 시각화
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);

        // 지정한 레이어만 Raycast 대상으로 판별
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
        {
            if ((playerLayer & (1 << hit.transform.gameObject.layer)) != 0)
            { return true; }
        }

        return false;
    }



    protected void Update()
    {
        UpdateNextMoveDirection(); // 다음 방향 설정
        UpdateMyPositionOnNav();   // 자신 위치 갱신
    }
}