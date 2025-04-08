using System.Collections;
using System.Collections.Generic;
// using System.Numerics; // <- Vector3 모호한 참조 오류
using Unity.VisualScripting;
using UnityEngine;

public class Player : Actor
{
    // 오브젝트에 대한 물리효과
    Rigidbody rigid;

    // 입력받는 방향
    protected float moveHorizontal;
    protected float moveVertical;
    // 입력 데드존
    [SerializeField]
    float inputDeadZone = 0.1f;

    // 현재 이 오브젝트의 위치 == transform.position
    Vector3 origin;
    // 다중 레이캐스트 (착지 판정)
    // 각 레이 사이의 간격
    float raySpacing;
    // 착지 시, 착지했는지 거리 판단
    float bottomRayDistance;
    // 앞쪽 거리 판단
    float frontRayDistance;

    // 점프 입력 여부
    bool isJumpKeyDown;
    // 점프 높이
    [SerializeField]
    float jumpHeight;

    // 공격했는가
    bool isAttack;


    // 생성 시 초기화
    void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();
        // null 초기화 방어
        if (rigid == null)
        {
            Debug.LogError("Rigidbody 컴포넌트 누락!", gameObject);
            enabled = false; // 생성 취소
        }

        // 레이캐스트 거리: 정육면체를 기준으로 설정됨

        // 바닥 레이 사이의 간격
        // 너무 넓으면, 다른 큐브와 걸치는 경우 2중 점프 등 문제 발생
        raySpacing = (transform.localScale.x + transform.localScale.z) * 0.22f;

        // 하드코딩. 나중에 수정
        // 착지 확인
        bottomRayDistance = transform.localScale.y * 1.05f;
        // Move 여부
        frontRayDistance = transform.localScale.z * 0.6f;
    }


    // 프레임당 업데이트
    void Update()
    {
        // 입력
        SetInput();

        // 공격
        Attack();

        // 점프
        Jump();
    }

    // 물리엔진과 함께 업데이트 (0.02s)
    void FixedUpdate()
    {
        // 입력된 방향으로
        // 오브젝트 움직임/회전
        Move();
        Turn();

        // 오브젝트 위치 갱신
        SetOrigin();
    }

    // 이동 방향 입력
    public void InputWASD()
    // 입력(WASD, ↑↓←→)으로 방향 지정
    // 정규화된(모든 방향으로 크기가 1인) 방향벡터 생성
    {
        // 방향 입력받음
        moveHorizontal = Input.GetAxisRaw("Horizontal"); // x축 (좌우)
        moveVertical = Input.GetAxisRaw("Vertical");     // z축 (앞뒤)

        if (Mathf.Abs(moveHorizontal) < inputDeadZone) { moveHorizontal = 0; }
        if (Mathf.Abs(moveVertical) < inputDeadZone) { moveVertical = 0; }

        // 방향 대입
        // 45도(쿼터뷰) 틀어진 방향
        moveVec = (Quaternion.Euler(0, 45, 0)  // 이동 방향을 y축 기준 45도 회전 (카메라 각도)
            * (new Vector3(moveHorizontal, 0, moveVertical)).normalized); // 입력된 방향벡터
    }

    // 점프 여부 입력
    public void InputJump()
    {
        if (Input.GetButtonDown("Jump"))
        { isJumpKeyDown = true; }
    }

    // 공격 입력
    public void InputAttack()
    {
        if (Input.GetMouseButtonDown(0) &&  // 좌클릭 누름
            Time.time >= nextAttackTime)    // 쿨타임 끝남
        {
            isAttack = true;
            nextAttackTime = Time.time + attackRate;  // 다음 공격 가능 시간 설정
        }
    }

    // 각종 입력 대응
    // WASD || ↑↓←→
    // Jump(Space Bar)
    // Attack(좌클릭)
    public void SetInput()
    { InputWASD(); InputJump(); InputAttack(); }

    // 오브젝트 위치 설정
    // 오브젝트의 중앙 위치
    public void SetOrigin()
    { origin = transform.position; }


    // 이동
    // 위치 += 방향 * 스피드
     public override void Move()
    {
        // 레이캐스트를 쏘고, 앞에 뭐가 없으면 이동
        if (moveVec != Vector3.zero &&
            !Physics.Raycast(origin, moveVec, frontRayDistance))
        {
            // 물리 방식 이동
            // 현재 위치
            // + 방향 * 이동 간격 * 이동 간격 보정
            rigid.MovePosition(rigid.position
                + moveVec * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // 회전
    // 진행 방향을 바라봄
    public void Turn()
    { transform.LookAt(transform.position + moveVec); }

    // 점프
    // 위치 += 위쪽 방향 * 점프높이
    // 힘을 가함 (물리효과)
    public void Jump()
    {
        // 점프를 입력했다면 && 착지 상태라면
        if (isJumpKeyDown && IsGrounded())
        {
            // 점프 시: 중력가속도 초기화
            rigid.velocity = Vector3.zero;
            // 위쪽 방향으로 jumpHeight만큼 힘을 가함
            rigid.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }

        // 점프 입력상태 초기화
        isJumpKeyDown = false;
    }


    // 착지 상태인지 판정
    public bool IsGrounded()
    {
        // 앞/뒤 레이캐스트
        return
            // 앞쪽 레이캐스트
            Physics.Raycast(origin + (transform.forward * raySpacing),
            Vector3.down,
            bottomRayDistance) ||

            // 뒤쪽 레이캐스트
            Physics.Raycast(origin - (transform.forward * raySpacing),
            Vector3.down,
            bottomRayDistance);
    }


    public override void Attack()
    {
        if (isAttack)
        {
            MeleeBasicAttack();
            isAttack = false;
        }
    }


    void MeleeBasicAttack()
    {
        // OverlapSphere를 사용하여 공격 범위 내의 모든 콜라이더를 찾음
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);

        // 감지한 모든 콜라이더에 대해서 판정
        foreach (Collider hitCollider in hitColliders)
        {
            // 타겟 태그를 가진 오브젝트인지 확인
            if (hitCollider.CompareTag(targetTag))
            {

                // 타겟 방향 벡터 계산
                Vector3 directionToTarget = hitCollider.transform.position - transform.position;
                directionToTarget.y = 0;  // Y축 값을 0으로 설정 (높이 차이 무시)

                // 자신의 전방 벡터와 타겟 방향 벡터 사이의 각도 계산
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                // 계산된 각도가 공격 각도의 절반보다 작은지 확인
                if (angleToTarget <= attackAngle / 2)
                {

                    // Actor 컴포넌트가 있는지 확인하고 데미지 처리
                    Actor targetActor = hitCollider.GetComponent<Actor>();
                    if (targetActor != null)
                    {
                        targetActor.TakeDamage(attackDamage);
                        // 디버그 시각화: 공격이 적중한 타겟까지 빨간색 선 그리기
                        Debug.DrawLine(transform.position, hitCollider.transform.position, Color.red, 1f);
                    }
                }
            }
        }
    } // MeleeBasicAttack


    // 시각화
    void OnDrawGizmos()
    {
        // 공격 범위를 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 공격 각도를 시각화
        Gizmos.color = Color.blue;
        Vector3 rightDir = Quaternion.Euler(0, attackAngle / 2, 0) * transform.forward;
        Vector3 leftDir = Quaternion.Euler(0, -attackAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, rightDir * attackRange);
        Gizmos.DrawRay(transform.position, leftDir * attackRange);
    }

}