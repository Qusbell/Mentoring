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


    // 목적지 갱신 (이전 버전)
    //  void UpdateDestination()
    //  {
    //      if (target != null && nav.isOnNavMesh)
    //      { nav.SetDestination(target.position); }
    //  }

    // 목적지 갱신
    //  private void UpdateDestination()
    //  {
    //      if (target == null || nav == null || !nav.isOnNavMesh) { return; }
    //  
    //      NavMeshPath path = new NavMeshPath();
    //  
    //      if (NavMesh.CalculatePath(nav.transform.position, target.position, NavMesh.AllAreas, path)
    //          && path.status == NavMeshPathStatus.PathComplete)
    //      { nav.SetPath(path); }
    //      
    //  }


    private void UpdateDestination()
    {
        if (target == null || nav == null || !nav.isOnNavMesh) { return; }

        NavMeshHit hit;
        Vector3 targetPosOnNav = nav.transform.position; ;

        // target 주변 최대 20m 반경 내에서 NavMesh 가장 가까운 점을 찾음
        if (NavMesh.SamplePosition(target.position, out hit, 20.0f, NavMesh.AllAreas))
        { targetPosOnNav = hit.position; }

        NavMeshPath path = new NavMeshPath();

        // 목적지로 보정된 targetPosOnNav를 넣어 경로 계산
        if (NavMesh.CalculatePath(nav.transform.position, targetPosOnNav, NavMesh.AllAreas, path)
            && path.status == NavMeshPathStatus.PathComplete)
        {
            nav.SetPath(path);
        }
    }




    // 추격 가능 여부
    protected bool _isCanChase;

    public bool isCanChase
    {
        get
        {
            _isCanChase = IsCanChaseTarget();
            return _isCanChase;
        }
    }

    // 추적 가능 여부 <- 수정할 것, 현재 제대로 작동 X
    protected bool IsCanChaseTarget()
    {
        // 1. 타겟이 존재하는가?
        if (target == null)
        {
            // Debug.Log("target 미존재");
            return false;
        }

        // 2. 네비메쉬 위에 있는가?
        if (!nav.isOnNavMesh)
        {
            // Debug.Log("navMesh 위에 없음");
            return false;
        }

        // 3. 경로가 유효한가?
        if (!nav.hasPath)
        {
            // Debug.Log(this.gameObject.GetInstanceID() + " : 경로 미유효");
            return false;
        }

        // 추적 가능
        // Debug.Log("추적 가능");
        return true;
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
        // nav상 위치와 transform 위치의 괴리
        float gapDistance = (nav.nextPosition - transform.position).sqrMagnitude;

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



    public bool IsFacingTarget(float tolerance = 0.99f)
    {
        Vector3 toTarget = (target.position - transform.position);
        toTarget.y = 0f; // y값 무시
        toTarget.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0f; // y값 무시
        forward.Normalize();

        float dot = Vector3.Dot(forward, toTarget);
        return dot >= tolerance;
    }



    protected void Update()
    {
        UpdateNextMoveDirection(); // 다음 방향 설정
        UpdateMyPositionOnNav();   // 자신 위치 갱신
    }
}