using System.Collections;
using UnityEngine; // Unity의 핵심 클래스들을 포함하는 네임스페이스

// 붕괴하는 큐브의 작동을 담당함
public class CubeCollapseTrigger : MonoBehaviour
{
    [Header("붕괴 트리거 조건")]
    public bool useTimeTrigger = false;         // 시간 경과로 붕괴할지 여부
    public float delayTime = 3f;                // 붕괴까지 기다릴 시간 (초)

    public bool usePlayerTrigger = false;       // 플레이어가 특정 위치에 오면 붕괴할지 여부
    public Transform player;                    // 플레이어 오브젝트
    public Vector3 triggerPosition;             // 플레이어가 도달해야 하는 위치
    public float triggerRadius = 0.5f;          // 트리거 위치 반경 (거리 허용 오차)

    [Header("붕괴 설정")]
    public float collapseSpeed = 5f;            // 붕괴 속도
    public float collapseDistance = 10f;        // 얼마나 떨어질지 (거리)

    [Header("붕괴 방향 (기본: 아래 방향)")]
    public Vector3 customCollapseDirection = Vector3.down; // 원하는 붕괴 방향 설정

    private bool isCollapsing = false;          // 현재 붕괴 중인지 여부
    private Vector3 collapseDirection;          // 실제 사용될 붕괴 방향
    private Vector3 startPosition;              // 시작 위치 저장용

    void Start()
    {
        startPosition = transform.position;

        // 사용자 설정 방향을 정규화해서 적용 (방향만 유지)
        collapseDirection = customCollapseDirection.normalized;

        // 시간 트리거가 켜졌다면 일정 시간 뒤 붕괴 시작
        if (useTimeTrigger)
        {
            StartCoroutine(CollapseAfterDelay());
        }
    }

    void Update()
    {
        // 플레이어가 트리거 위치 근처에 도달했는지 확인
        if (usePlayerTrigger && player != null)
        {
            float distanceToTrigger = Vector3.Distance(player.position, triggerPosition);
            if (distanceToTrigger < triggerRadius && !isCollapsing)
            {
                StartCoroutine(CollapseSequence());
            }
        }

        // 붕괴 중이면 설정된 방향으로 계속 이동
        if (isCollapsing)
        {
            transform.Translate(collapseDirection * collapseSpeed * Time.deltaTime);

            // 지정된 거리 이상 떨어졌다면 큐브 삭제
            if (Vector3.Distance(startPosition, transform.position) >= collapseDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    // 일정 시간 후 붕괴 시작
    IEnumerator CollapseAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        yield return StartCoroutine(CollapseSequence());
    }

    // 붕괴 연출 후 붕괴 시작
    IEnumerator CollapseSequence()
    {
        // 위아래로 살짝 흔들리는 연출
        Vector3 up = transform.position + Vector3.up * 0.3f;
        Vector3 down = transform.position - Vector3.up * 0.3f;

        for (int i = 0; i < 3; i++)
        {
            transform.position = up;
            yield return new WaitForSeconds(0.05f);
            transform.position = down;
            yield return new WaitForSeconds(0.05f);
        }

        // 본격적으로 붕괴 시작
        isCollapsing = true;
    }
}