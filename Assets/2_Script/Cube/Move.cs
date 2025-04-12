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
    public float raycastDistance = 0.4f; // 충돌 감지 거리

    private bool isMoving = false; // 현재 이동 중인지 여부

    // 레이캐스트 방향 배열 (6방향: 앞, 뒤, 좌, 우, 위, 아래)
    private readonly Vector3[] directions = new Vector3[]
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        Vector3.up,
        Vector3.down
    };

    void OnEnable()
    {
        isMoving = true; // 오브젝트 활성화 시 이동 시작
    }

    void Update()
    {
        if (!isMoving) return;

        // 모든 방향으로 Ray를 쏴서 충돌 검사
        foreach (var dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, raycastDistance))
            {
                if (hit.collider.CompareTag("CollidedCube"))
                {
                    isMoving = false; // 멈춤
                    gameObject.tag = "CollidedCube"; // 자신의 태그도 변경
                    return;
                }
            }
        }

        // 충돌 없으면 계속 이동
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    // 디버그용: 씬에서 레이 방향을 보여줌
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var dir in directions)
        {
            Gizmos.DrawLine(transform.position, transform.position + dir * raycastDistance);
        }
    }
}
