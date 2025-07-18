using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 멧돼지 경고 표시 관리
/// 돌진 경로에 시각적 경고 효과를 표시하는 시스템
/// </summary>
public class BoarWarning : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("경고 시간 설정")]
    [Tooltip("경고 표시 지속 시간 (초)")]
    public float warningDuration = 1f;

    #endregion

    #region ===== 내부 변수 =====

    // 메인 컴포넌트 참조
    private BoarCube main;

    // 경고 평면 관리
    private List<GameObject> activeWarningPlanes = new List<GameObject>();

    // 공유 리소스 (메모리 효율성을 위한 정적 변수)
    private static Material sharedWarningMaterial;
    private static Queue<GameObject> warningPlanePool = new Queue<GameObject>();
    private static readonly Color warningColor = Color.red;
    private static readonly int POOL_SIZE = 20;

    #endregion

    #region ===== 초기화 =====

    /// <summary>
    /// BoarCube에서 호출하는 초기화 메서드
    /// </summary>
    public void Initialize(BoarCube mainComponent)
    {
        main = mainComponent;

        InitializeSharedResources();
        EnsureWarningPlanePool();

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] BoarWarning 초기화 완료");
    }

    /// <summary>
    /// 공유 머티리얼 초기화 (모든 경고 평면이 공유)
    /// </summary>
    private static void InitializeSharedResources()
    {
        if (sharedWarningMaterial != null) return;

        // 반투명 머티리얼 생성
        sharedWarningMaterial = new Material(Shader.Find("Standard"));

        // 투명도 설정
        sharedWarningMaterial.SetFloat("_Mode", 3); // Transparent 모드
        sharedWarningMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        sharedWarningMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        sharedWarningMaterial.SetInt("_ZWrite", 0);
        sharedWarningMaterial.EnableKeyword("_ALPHABLEND_ON");
        sharedWarningMaterial.renderQueue = 3000;

        // 발광 효과 추가
        sharedWarningMaterial.EnableKeyword("_EMISSION");

        Debug.Log("[BoarWarning] 공유 머티리얼 초기화 완료");
    }

    /// <summary>
    /// 경고 평면 오브젝트 풀 준비
    /// </summary>
    private static void EnsureWarningPlanePool()
    {
        while (warningPlanePool.Count < POOL_SIZE)
        {
            GameObject plane = CreateWarningPlane();
            plane.SetActive(false);
            warningPlanePool.Enqueue(plane);
        }
    }

    /// <summary>
    /// 새로운 경고 평면 생성
    /// </summary>
    private static GameObject CreateWarningPlane()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.name = "PooledWarningPlane";

        // 콜라이더 제거 (시각적 효과만 필요)
        Object.Destroy(plane.GetComponent<Collider>());

        // 레이어 설정 (레이캐스트 무시)
        plane.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 머티리얼 적용
        plane.GetComponent<Renderer>().material = sharedWarningMaterial;

        return plane;
    }

    #endregion

    #region ===== 공개 메서드 =====

    /// <summary>
    /// 경고 표시 시작
    /// </summary>
    public void ShowWarning()
    {
        // 기존 경고 제거
        ClearWarning();

        // 경로 계산
        Vector3 direction = GetMovementDirection();
        float pathLength = GetPathLength();
        float actualWidth = GetActualAttackWidth();

        // 경고 평면 생성
        CreateWarningPlanesAlongPath(direction, pathLength, actualWidth);

        if (main.showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] 경고 표시 생성 완료");
            Debug.Log($"[{gameObject.name}] 경로 길이: {pathLength:F2}, 폭: {actualWidth:F2}, 평면 수: {activeWarningPlanes.Count}");
        }
    }

    /// <summary>
    /// 모든 경고 표시 제거
    /// </summary>
    public void ClearWarning()
    {
        foreach (GameObject plane in activeWarningPlanes)
        {
            ReturnWarningPlaneToPool(plane);
        }

        activeWarningPlanes.Clear();

        if (main.showDebugLog && activeWarningPlanes.Count > 0)
            Debug.Log($"[{gameObject.name}] 경고 표시 제거 완료");
    }

    /// <summary>
    /// 경고 지속 시간 반환
    /// </summary>
    public float GetWarningDuration()
    {
        return warningDuration;
    }

    #endregion

    #region ===== 내부 메서드 - 경고 생성 =====

    /// <summary>
    /// 경로를 따라 경고 평면들 생성
    /// </summary>
    private void CreateWarningPlanesAlongPath(Vector3 direction, float pathLength, float actualWidth)
    {
        // 세그먼트 수 계산 (폭을 기준으로)
        int segmentCount = Mathf.CeilToInt(pathLength / actualWidth);
        segmentCount = Mathf.Max(1, segmentCount);

        Vector3 startPos = GetStartPosition();
        Vector3 endPos = GetEndPosition();

        // 각 세그먼트마다 경고 평면 생성
        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 segmentPos = Vector3.Lerp(startPos, endPos, t);

            CreateWarningPlaneAt(segmentPos, actualWidth);
        }
    }

    /// <summary>
    /// 지정된 위치에 경고 평면 생성
    /// </summary>
    private void CreateWarningPlaneAt(Vector3 position, float width)
    {
        // 지면 탐지를 위한 레이어마스크
        LayerMask groundLayerMask = (1 << 0) | (1 << 8); // Default + Cube 레이어

        // 위에서 아래로 레이캐스트하여 지면 찾기
        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayerMask))
        {
            GameObject warningPlane = GetWarningPlaneFromPool();

            // 위치 설정 (지면 바로 위)
            warningPlane.transform.position = hit.point + Vector3.up * 0.01f;

            // 회전 설정 (지면과 평행)
            warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0);

            // 크기 설정
            warningPlane.transform.localScale = new Vector3(width, width, 1f);

            // 색상 적용
            UpdateWarningPlaneColor(warningPlane);

            // 활성 리스트에 추가
            activeWarningPlanes.Add(warningPlane);
        }
        else if (main.showDebugLog)
        {
            Debug.LogWarning($"[{gameObject.name}] 위치 {position}에서 지면을 찾을 수 없습니다");
        }
    }

    /// <summary>
    /// 경고 평면의 색상 업데이트
    /// </summary>
    private void UpdateWarningPlaneColor(GameObject plane)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer == null) return;

        // 인스턴스 머티리얼 생성 (개별 색상 적용을 위해)
        Material instanceMaterial = new Material(sharedWarningMaterial);

        // 색상 설정
        Color finalColor = warningColor;
        finalColor.a = main.warningAlpha;
        instanceMaterial.color = finalColor;

        // 발광 색상 설정
        instanceMaterial.SetColor("_EmissionColor", warningColor * 0.5f);

        renderer.material = instanceMaterial;
    }

    #endregion

    #region ===== 내부 메서드 - 풀링 시스템 =====

    /// <summary>
    /// 풀에서 경고 평면 가져오기
    /// </summary>
    private static GameObject GetWarningPlaneFromPool()
    {
        EnsureWarningPlanePool();

        if (warningPlanePool.Count > 0)
        {
            GameObject plane = warningPlanePool.Dequeue();
            plane.SetActive(true);
            return plane;
        }

        // 풀이 비어있으면 새로 생성
        Debug.LogWarning("[BoarWarning] 경고 평면 풀이 부족합니다. 새로 생성합니다.");
        return CreateWarningPlane();
    }

    /// <summary>
    /// 경고 평면을 풀로 반환
    /// </summary>
    private static void ReturnWarningPlaneToPool(GameObject plane)
    {
        if (plane == null) return;

        plane.SetActive(false);

        // 풀 크기 제한
        if (warningPlanePool.Count < POOL_SIZE * 2)
        {
            warningPlanePool.Enqueue(plane);
        }
        else
        {
            // 풀이 너무 크면 파괴
            Object.Destroy(plane);
        }
    }

    #endregion

    #region ===== 유틸리티 메서드 =====

    /// <summary>
    /// 이동 방향 계산
    /// </summary>
    private Vector3 GetMovementDirection()
    {
        return (GetEndPosition() - GetStartPosition()).normalized;
    }

    /// <summary>
    /// 경로 총 길이 계산
    /// </summary>
    private float GetPathLength()
    {
        return Vector3.Distance(GetStartPosition(), GetEndPosition());
    }

    /// <summary>
    /// 시작 위치 계산
    /// </summary>
    private Vector3 GetStartPosition()
    {
        return transform.position + main.startPositionOffset;
    }

    /// <summary>
    /// 목표 위치 계산
    /// </summary>
    private Vector3 GetEndPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// 실제 공격 폭 계산
    /// </summary>
    private float GetActualAttackWidth()
    {
        // 설정된 값이 있으면 사용
        if (main.attackWidth > 0f)
            return main.attackWidth;

        // 렌더러 크기 기반 계산
        Renderer cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            return Mathf.Max(cubeRenderer.bounds.size.x, cubeRenderer.bounds.size.z);
        }

        // 기본값: Transform 스케일 기반
        return Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
    }

    #endregion

    #region ===== 정보 조회 =====

    /// <summary>
    /// 현재 활성화된 경고 평면 수 반환
    /// </summary>
    public int GetActiveWarningCount()
    {
        return activeWarningPlanes.Count;
    }

    /// <summary>
    /// 경고가 현재 표시 중인지 여부
    /// </summary>
    public bool IsWarningActive
    {
        get { return activeWarningPlanes.Count > 0; }
    }

    #endregion

    #region ===== 생명주기 관리 =====

    /// <summary>
    /// 오브젝트 파괴 시 정리
    /// </summary>
    void OnDestroy()
    {
        ClearWarning();

        if (main != null && main.showDebugLog)
            Debug.Log($"[{gameObject.name}] BoarWarning 파괴 시 정리 완료");
    }

    #endregion

    #region ===== 디버그 시각화 =====

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 경고 경로 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (main == null) return;

        Vector3 startPos = GetStartPosition();
        Vector3 endPos = GetEndPosition();
        float width = GetActualAttackWidth();

        // 경로 선 그리기
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawLine(startPos, endPos);

        // 경고 영역 표시
        Vector3 direction = GetMovementDirection();
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized * (width / 2f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Vector3[] corners = {
            startPos + perpendicular,
            startPos - perpendicular,
            endPos - perpendicular,
            endPos + perpendicular
        };

        for (int i = 0; i < corners.Length; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % corners.Length]);
        }
    }

    /// <summary>
    /// 선택 시 상세 정보 표시
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (main == null || !main.showDebugLog) return;

        // 활성 경고 평면들 강조 표시
        Gizmos.color = Color.yellow;
        foreach (GameObject plane in activeWarningPlanes)
        {
            if (plane != null)
            {
                Gizmos.DrawWireCube(plane.transform.position, plane.transform.localScale);
            }
        }

        // 상태 정보 텍스트
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 4f,
            $"경고 평면 수: {activeWarningPlanes.Count}\n" +
            $"경고 지속시간: {warningDuration}초\n" +
            $"경로 길이: {GetPathLength():F2}m\n" +
            $"경고 폭: {GetActualAttackWidth():F2}m"
        );
    }
#endif

    #endregion
}