using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;


public class Cube : MonoBehaviour
{
    // 움직이는 큐브인가
    [SerializeField]
    bool isMoving = false;

    // 어느 방향으로 움직일 것인가
    [SerializeField]
    Vector3 moveVec;

    // 이동 속도
    [SerializeField]
    float moveSpeed;



    void FixedUpdate()
    {
        Move();
    }


    // 이동
    // 위치 += 방향 * 스피드
    void Move()
    {
        if (isMoving)
        {
            // 현재 위치 += 방향 * 이동 간격 * 이동 간격 보정
           transform.position += moveVec * moveSpeed * Time.fixedDeltaTime;
        }
    }



    // 콜라이더가 감지되면
    // Cube <-> Cube: 멈춤
    // Cube <-> 플레이어 등 유닛: 데미지
    // 당장은 구분없이 그냥 무조건 멈춤
    void OnCollisionEnter(Collision collision)
    {
        // 움직임 멈춤
        isMoving = false;
    }
}
