using System.Collections;
using UnityEngine;

// 큐브를 지정된 방향으로 이동시킴
public class CubeMover : MonoBehaviour
{
    private Vector3 direction;      // 이동할 방향
    private float speed;            // 이동 속도
    private bool isMoving = false;  // 현재 이동 중인지 여부

    private Vector3 startPosition;  // 처음 생성된 위치 저장용
    public float destroyDistance = 20f; // 이 거리 이상 이동하면 파괴

    // 큐브 이동을 설정하는 함수
    public void SetMovement(Vector3 dir, float spd)
    {
        direction = dir.normalized;     // 방향 벡터를 정규화 (속도 일관성 유지)
        speed = spd;                    // 이동 속도 설정
        isMoving = true;                // 이동 시작
        startPosition = transform.position; // 시작 위치 저장
    }

    void Update()
    {
        if (isMoving)
        {
            // 설정된 방향으로 프레임당 이동
            transform.Translate(direction * speed * Time.deltaTime);

            // 너무 멀리 이동하면 자동 파괴 (최적화 목적)
            if (Vector3.Distance(startPosition, transform.position) > destroyDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트의 태그를 가져옵니다
        string tag = collision.gameObject.tag;

        // 충돌 시 이동을 멈추게 할 대상의 태그 목록
        if (tag == "Cube" || tag == "Wall" || tag == "Obstacle")
        {
            // 지정된 태그 중 하나와 충돌한 경우, 이동을 멈춥니다
            isMoving = false;
        }
    }
}
