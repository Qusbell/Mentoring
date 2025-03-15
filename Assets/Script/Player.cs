using System.Collections;
using System.Collections.Generic;
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
    Vector3 direction;
    // 오브젝트 위치
    Vector3 origin;

    // 점프 입력 여부
    bool isJumpKeyDown;
    // 다중 레이캐스트 (착지 판정)
    // 각 레이 사이의 간격
    float raySpacing;
    // 착지 시, 착지했는지 거리 판단
    float rayDistance;


    // 1초당 이동할 칸 수 (WASD)
    public float speed;
    // 점프 높이
    public float jumpHeight;


    // 생성 시 초기화
    void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();
        // null 초기화 방어
        if (rigid == null)
        {
            Debug.LogError("Rigidbody 컴포넌트 누락!", gameObject);
            enabled = false;
        }

        // 레이 사이의 간격
        // 0.4는 좀 널널한 느낌
        // 0.3은 좀 빡빡한 느낌
        raySpacing = (transform.localScale.x + transform.localScale.z) * 0.4f;
        // 착지 확인 간격
        // y 길이의 0.5
        // 정육면체를 기준으로 설정됨
        rayDistance = transform.localScale.y * 0.5f;
    }


    // Update is called once per frame
    void Update()
    {
        // 입력
        SetInput();
        // 입력된 방향으로
        // 오브젝트 움직임/회전/점프
        Move();
        Turn();
        Jump();
    }


    // 이동 방향 입력
    void InputWASD()
    // 입력(WASD, ↑↓←→)으로 방향 지정
    // 정규화된(모든 방향으로 크기가 1인) 방향벡터 생성
    {
        // 방향 입력받음
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        // 방향 대입
        direction = new Vector3(moveHorizontal, 0, moveVertical).normalized;

        // 오브젝트 위치 갱신
        SetOrigin();
    }

    // 점프 여부 입력
    void InputJump()
    { isJumpKeyDown = Input.GetButtonDown("Jump"); }


    // 각종 입력 대응
    void SetInput()
    {
        InputWASD();  // WASD 입력
        InputJump();  // Jump 입력
    }

    // 이동
    // 위치 += 방향 * 스피드
    void Move()
    {
        // 이동하려는 방향으로
        // 레이캐스트를 쏘고
        // 뭐가 없으면 이동
        // test
        if (!Physics.Raycast(origin, direction, rayDistance))
        {
            // 물리 방식 이동
            // 현재 위치
            // + 방향 * 이동 간격 * 이동 간격 보정
            rigid.MovePosition(rigid.position
                + direction * speed * Time.deltaTime);
        }
    }

    // 회전
    // 진행 방향을 바라봄
    void Turn()
    { transform.LookAt(transform.position + direction); }


    // 점프
    // 위치 += 위쪽 방향 * 점프높이
    // 힘을 가함 (물리효과)
    void Jump()
    {
        // 점프를 입력했다면 && 착지 상태라면
        if (isJumpKeyDown && IsGrounded())
        {
            // 점프 시: 중력가속도 초기화
            rigid.velocity = Vector3.zero;
            // 위쪽 방향으로 jumpHeight만큼 힘을 가함
            rigid.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
    }

    // 오브젝트 위치 설정
    void SetOrigin()
    {
        // 오브젝트 위치 (가운데)
        origin = transform.position;
    }

    // 착지 상태인지 판정
    bool IsGrounded()
    {
        // 앞/가운데/뒤 레이캐스트
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