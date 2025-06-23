using UnityEngine;

/// <summary>
/// 큐브 붕괴를 위한 에리어 트리거 컴포넌트
/// 플레이어나 지정된 오브젝트가 트리거 영역에 들어오면 연결된 큐브를 붕괴시킴
/// </summary>
[RequireComponent(typeof(Collider))]
public class CubeCollapseTrigger : MonoBehaviour
{
    [Header("붕괴 설정")]
    [Tooltip("붕괴시킬 큐브 (CubeCollapser 컴포넌트가 있어야 함)")]
    public CubeCollapser targetCube;

    [Tooltip("붕괴를 트리거할 오브젝트의 태그")]
    public string triggerTag = "Player";

    [Tooltip("한 번만 작동하는지 여부 (true: 한 번 작동 후 비활성화)")]
    public bool oneTimeUse = true;

    [Header("디버그 설정")]
    [Tooltip("트리거 영역을 씬에서 시각화")]
    public bool showTriggerArea = true;

    // 내부 변수
    private bool hasTriggered = false;
    private Collider triggerCollider;

    void Awake()
    {
        // 콜라이더 설정 확인
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            // 트리거로 설정
            triggerCollider.isTrigger = true;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] CubeCollapseTrigger에 Collider가 없습니다!");
        }

        // 타겟 큐브 검증
        if (targetCube == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 타겟 큐브가 설정되지 않았습니다.");
        }
    }

    void Start()
    {
        // 시작 시 상태 로그
        Debug.Log($"[{gameObject.name}] 큐브 붕괴 트리거 준비 완료. " +
                 $"타겟: {(targetCube != null ? targetCube.name : "없음")}, " +
                 $"트리거 태그: {triggerTag}");
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 트리거되었고 일회용이면 무시
        if (hasTriggered && oneTimeUse)
        {
            return;
        }

        // 태그 확인
        if (!other.CompareTag(triggerTag))
        {
            return;
        }

        // 타겟 큐브 확인
        if (targetCube == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 타겟 큐브가 설정되지 않아 붕괴를 시작할 수 없습니다.");
            return;
        }

        // 붕괴 트리거
        Debug.Log($"[{gameObject.name}] {other.name}이(가) 트리거 영역에 진입했습니다. " +
                 $"큐브 '{targetCube.name}' 붕괴를 시작합니다.");

        targetCube.TriggerCollapse();
        hasTriggered = true;

        // 일회용인 경우 비활성화
        if (oneTimeUse)
        {
            Debug.Log($"[{gameObject.name}] 일회용 트리거가 작동했습니다. 트리거를 비활성화합니다.");
            triggerCollider.enabled = false;
        }
    }

    // 트리거 초기화 (재사용을 위해)
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
        Debug.Log($"[{gameObject.name}] 트리거가 초기화되었습니다.");
    }

    // 타겟 큐브 변경
    public void SetTargetCube(CubeCollapser newTarget)
    {
        targetCube = newTarget;
        Debug.Log($"[{gameObject.name}] 타겟 큐브가 '{(newTarget != null ? newTarget.name : "없음")}'로 변경되었습니다.");
    }

    // 수동으로 붕괴 트리거 (다른 스크립트에서 호출 가능)
    public void ManualTrigger()
    {
        if (targetCube != null && (!hasTriggered || !oneTimeUse))
        {
            Debug.Log($"[{gameObject.name}] 수동 트리거로 큐브 '{targetCube.name}' 붕괴를 시작합니다.");
            targetCube.TriggerCollapse();
            hasTriggered = true;

            if (oneTimeUse && triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
    }

    // 디버그용: 씬에서 트리거 영역 시각화
    void OnDrawGizmos()
    {
        if (!showTriggerArea) return;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // 트리거 상태에 따라 색상 변경
        if (hasTriggered && oneTimeUse)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // 회색 (비활성화됨)
        }
        else
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f); // 빨간색 (활성화됨)
        }

        // 콜라이더 타입에 따라 다르게 그리기
        if (col is BoxCollider)
        {
            BoxCollider boxCol = col as BoxCollider;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(boxCol.center, boxCol.size);
            Gizmos.matrix = oldMatrix;
        }
        else if (col is SphereCollider)
        {
            SphereCollider sphereCol = col as SphereCollider;
            Gizmos.DrawSphere(transform.position + sphereCol.center,
                             sphereCol.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
        }
        else
        {
            // 기타 콜라이더는 단순 와이어프레임으로
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }

        // 타겟 큐브가 있으면 연결선 그리기
        if (targetCube != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // 노란색 연결선
            Gizmos.DrawLine(transform.position, targetCube.transform.position);

            // 타겟 큐브 위치에 작은 표시
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f); // 빨간색
            Gizmos.DrawWireCube(targetCube.transform.position, Vector3.one * 0.2f);
        }
    }
}