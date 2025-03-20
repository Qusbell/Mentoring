using System.Collections;
using System.Collections.Generic;
// using System.Numerics; // <- Vector3 모호한 참조 오류
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 오브젝트에 대한 물리효과
    Rigidbody rigid;

    // 방향 입력받음
    float moveHorizontal;
    float moveVertical;
    // 이동할 방향(정규화 벡터)
    Vector3 moveVec;
    // 공격 방향(정규화 벡터/마우스 포인터 지정)
    Vector3 attackVec;
    // 현재 이 오브젝트의 위치
    Vector3 origin;

    // 점프 입력 여부
    bool isJumpKeyDown;
    // 다중 레이캐스트 (착지 판정)
    // 각 레이 사이의 간격
    float raySpacing;
    // 착지 시, 착지했는지 거리 판단
    float rayDistance;

    // 1초당 이동할 칸 수 (WASD)
    [SerializeField] // 유니티 컴포넌트에서 수정
    float moveSpeed;
    // 점프 높이
    [SerializeField] // 유니티 컴포넌트에서 수정
    float jumpHeight;


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
        // 착지 확인 || Move 여부 간격
        // y 길이의 0.5
        rayDistance = transform.localScale.y * 0.5f;
    }


    // 프레임당 업데이트
    void Update()
    {
        // 입력
        SetInput();
    }

    // 물리엔진과 함께 업데이트 (0.02s)
    void FixedUpdate()
    {
        // 입력된 방향으로
        // 오브젝트 움직임/회전
        Move();
        Turn();
        // 점프
        Jump();

        // 오브젝트 위치 갱신
        SetOrigin();
    }


    // 이동 방향 입력
    void InputWASD()
    // 입력(WASD, ↑↓←→)으로 방향 지정
    // 정규화된(모든 방향으로 크기가 1인) 방향벡터 생성
    {
        // 방향 입력받음
        moveHorizontal = Input.GetAxisRaw("Horizontal"); // x축 (좌우)
        moveVertical = Input.GetAxisRaw("Vertical");     // z축 (앞뒤)

        // 방향 대입
        // 45도(쿼터뷰) 틀어진 방향
        moveVec = Quaternion.Euler(0, 45, 0)  // 이동 방향을 y축 기준 45도 회전 (카메라 각도) <- 하드코딩. 나중에 수정
            * (new Vector3(moveHorizontal, 0, moveVertical).normalized); // 입력된 방향벡터
    }

    // 점프 여부 입력
    void InputJump()
    {
        if (Input.GetButtonDown("Jump"))
        { isJumpKeyDown = true; }
    }

    // 각종 입력 대응
    // WASD || ↑↓←→
    // Jump(Space Bar)
    void SetInput()
    { InputWASD(); InputJump(); }

    // 오브젝트 위치 설정
    // 오브젝트의 중앙 위치
    void SetOrigin()
    { origin = transform.position; }


    // 이동
    // 위치 += 방향 * 스피드
    void Move()
    {
        // 레이캐스트를 쏘고, 앞에 뭐가 없으면 이동
        if (moveVec != Vector3.zero &&
            !Physics.Raycast(origin, moveVec, rayDistance))
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
    void Turn()
    { transform.LookAt(transform.position + moveVec); }

    // 점프
    // 위치 += 위쪽 방향 * 점프높이
    // 힘을 가함 (물리효과)
    void Jump()
    {
        // 점프를 입력했다면 && 착지 상태라면
        if (isJumpKeyDown && IsGrounded())
        {
            Debug.Log("Jump 실행"); // 점프 횟수 디버그

            // 점프 시: 중력가속도 초기화
            rigid.velocity = Vector3.zero;
            // 위쪽 방향으로 jumpHeight만큼 힘을 가함
            rigid.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }

        // 점프 입력상태 초기화
        isJumpKeyDown = false;
    }


    // 착지 상태인지 판정
    bool IsGrounded()
    {
        // 앞/중간/뒤 레이캐스트
        return
            // 앞쪽 레이캐스트
            Physics.Raycast(origin + (transform.forward * raySpacing),
            Vector3.down,
            rayDistance) ||

            // 중간 레이캐스트
            Physics.Raycast(origin,
            Vector3.down,
            rayDistance) ||

            // 뒤쪽 레이캐스트
            Physics.Raycast(origin - (transform.forward * raySpacing),
            Vector3.down,
            rayDistance);
    }
}