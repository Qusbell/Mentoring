using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("이동 방향 설정")]
    public Vector3 moveDirection = Vector3.right; // 기본 이동 방향

    [Header("이동 속도 설정")]
    public float moveSpeed = 2f; // 이동 속도

    [Header("레이캐스트 거리 설정")]
    public float raycastDistance = 0.5f; // 충돌 감지 거리

    private bool isMoving = false; // 현재 이동 중인지 여부

    void OnEnable()
    {
        isMoving = true; // 오브젝트 활성화 시 이동 시작
    }

    void Update()
    {
        if (!isMoving) return;  // 이동 중이 아니면 레이캐스트를 실행하지 않음

        // 레이캐스트는 이동 방향으로만 감지
        if (Physics.Raycast(transform.position, moveDirection.normalized, out RaycastHit hit, raycastDistance))
        {
            if (hit.collider.CompareTag("CollidedCube"))
            {
                isMoving = false; // 멈춤
                gameObject.tag = "CollidedCube"; // 자신의 태그도 변경
                return;
            }
        }

        // 충돌 없으면 계속 이동
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    // 디버그용: 씬에서 레이 방향을 보여줌
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // 레이캐스트는 이동 방향으로만 감지
        Gizmos.DrawLine(transform.position, transform.position + moveDirection.normalized * raycastDistance);
    }
}
