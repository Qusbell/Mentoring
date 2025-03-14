using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float hAxis; // ←→ 좌우이동 입력
    float vAxis; // ↑↓ 앞뒤이동 입력

    // 이동할 방향(정규화 벡터)
    Vector3 direction;

    // 1초당 이동할 칸 수
    public float speed;

    // Update is called once per frame
    void Update()
    {
        // 이동 방향 결정
        InputWASD();
        // 정규화된(모든 방향으로 크기가 1인) 방향벡터
        direction = GetDirection();

        // 오브젝트 움직임
        MoveTransform();
    }



    // 이동 방향 입력
    void InputWASD()
    {
        // 입력(WASD, ↑↓←→)으로 방향 지정
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
    }

    // 방향 생성
    Vector3 GetDirection()
    { return new Vector3(hAxis, 0, vAxis).normalized; }



    // 위치 += 방향 * 스피드
    void MoveToDirection()
    {
        // transform: 해당 게임 오브젝트
        // .position: 게임 오브젝트의 위치
        // Time.deltaTime: 일관된 움직임 보장
        transform.position
            += direction       // 방향
            * speed            // 이동 간격
            * Time.deltaTime;  // 시간당 일관된 이동
    }

    // 진행 방향을 바라봄
    void LookAtDirection()
    {
        // 회전
        // 진행 방향을 바라봄
        transform.LookAt(transform.position + direction);
    }

    // transform 방식 이동/회전
    void MoveTransform()
    {
        // 방향을 향해서 이동
        MoveToDirection();
        // 방향을 향해서 회전
        LookAtDirection();
    }
}