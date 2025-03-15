using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float hAxis;  // ←→ 좌우이동 입력
    float vAxis;  // ↑↓ 앞뒤이동 입력

    bool isJumpKeyDown;  // 점프 입력 여부


    // 다중 레이캐스트
    // 각 레이 사이의 간격
    float raySpacing;
    // 착지 시, 착지했는지 거리 판단
    float rayDistance;


    // 물리효과
    Rigidbody rigid;

    // 이동할 방향(정규화 벡터)
    Vector3 direction;

    // 1초당 이동할 칸 수 (WASD)
    public float speed;
    // 점프 높이
    public float jumpHeight;


    void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();

        // 레이 사이의 간격
        // 0.4는 좀 널널한 느낌
        // 0.3은 좀 빡빡한 느낌
        raySpacing = (transform.localScale.x + transform.localScale.z) * 0.4f;
        // 착지 확인 간격
        // y 길이의 0.5
        rayDistance = transform.localScale.y * 0.5f;
    }


    // Update is called once per frame
    void Update()
    {
        // 입력
        SetInput();
        // 정규화된(모든 방향으로 크기가 1인) 방향벡터 생성
        SetDirection();

        // 입력된 방향으로
        // 오브젝트 움직임/회전/점프
        Move();
        Turn();
        Jump();
    }


    // 이동 방향 입력
    void InputWASD()
    {
        // 입력(WASD, ↑↓←→)으로 방향 지정
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
    }

    // 점프 여부 입력
    void InputJump()
    { isJumpKeyDown = Input.GetButtonDown("Jump"); }
    
    
    // 각종 입력 대응
    void SetInput()
    {
        InputWASD();  // WASD 방향 입력
        InputJump();  // 점프 입력
    }


    // 방향 설정
    // 정규화된 방향
    void SetDirection()
    { direction = new Vector3(hAxis, 0, vAxis).normalized; }


    // 이동
    // 위치 += 방향 * 스피드
    void Move()
    {
        // transform: 해당 게임 오브젝트
        // .position: 게임 오브젝트의 위치
        // Time.deltaTime: 일관된 움직임 보장
        transform.position
            += direction       // 방향
            * speed            // 이동 간격
            * Time.deltaTime;  // 시간당 일관된 이동
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
            // 중력가속도 초기화
            rigid.velocity = Vector3.zero;
            // 위쪽 방향으로 jumpHeight만큼 힘을 가함
            rigid.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }

    }


    bool IsGrounded()
    {
        // 오브젝트 위치 (가운데)
        Vector3 origin = transform.position;

        // 앞/가운데/뒤 레이캐스트
        return
            // 앞쪽 레이캐스트
            Physics.Raycast(origin + (transform.forward * raySpacing),
            Vector3.down,
            rayDistance)||

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