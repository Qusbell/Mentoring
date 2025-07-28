using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 멧돼지 경고 표시 관리 (XYZ 개별 조절 버전)
/// 멧돼지 실제 이동 거리와 경고 표시 거리를 X, Y, Z 개별로 설정 가능
/// </summary>
public class BoarWarning : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("경고 시간 설정")]
    [Tooltip("경고 표시 지속 시간 (초)")]
    public float warningDuration = 1f;

    [Header("경고 거리 설정 (기획자 조절)")]
    [Tooltip("경고 표시 거리 (멧돼지 이동 거리와 독립적)")]
    public Vector3 warningDistance = new Vector3(10, 0, 0);

    [Header("경고 표시 설정")]
    [Tooltip("경고 평면을 바닥에서 얼마나 띄울지 (미터)")]
    public float warningHeightOffset = 0.1f;

    [Tooltip("지면 높이 (Y 좌표) - 레이캐스트 대신 사용")]
    public float groundLevel = 0f;

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

        // 경로 계산 (경고용 별도 계산)
        Vector3 direction = GetWarningDirection();
        float pathLength = GetWarningPathLength();
        float actualWidth = GetActualAttackWidth();

        // 경고 평면 생성
        CreateWarningPlanesAlongPath(direction, pathLength, actualWidth);

        if (main.showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] 경고 표시 생성 완료");
            Debug.Log($"[{gameObject.name}] 경고 길이: {pathLength:F2}, 폭: {actualWidth:F2}, 평면 수: {activeWarningPlanes.Count}");
            Debug.Log($"[{gameObject.name}] 멧돼지 이동거리: {main.startPositionOffset.magnitude:F2}, 경고 거리: {pathLength:F2}");
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
        // 경고 평면 간격 설정 (더 촘촘하게)
        float segmentSpacing = Mathf.Min(actualWidth * 0.5f, 2f); // 폭의 절반 또는 최대 2미터

        // 세그먼트 수 계산 (더 촘촘한 간격으로)
        int segmentCount = Mathf.CeilToInt(pathLength / segmentSpacing);
        segmentCount = Mathf.Max(1, segmentCount);

        Vector3 startPos = GetWarningStartPosition();  // 경고 시작 위치
        Vector3 endPos = GetWarningEndPosition();      // 경고 끝 위치

        // 각 세그먼트마다 경고 평면 생성
        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 segmentPos = Vector3.Lerp(startPos, endPos, t);

            CreateWarningPlaneAt(segmentPos, actualWidth);
        }
    }

    /// <summary>
    /// 지정된 위치에 경고 평면 생성 (멧돼지 밑바닥 높이 사용)
    /// </summary>
    private void CreateWarningPlaneAt(Vector3 position, float width)
    {
        GameObject warningPlane = GetWarningPlaneFromPool();

        // 멧돼지 큐브의 밑바닥 높이 계산
        float boarBottomY = transform.position.y - 1.5f; // 멧돼지 중앙에서 Y -1.5
        Vector3 warningPos = new Vector3(position.x, boarBottomY + warningHeightOffset, position.z);
        warningPlane.transform.position = warningPos;

        // 회전 설정 (지면과 평행)
        warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0);

        // 크기 설정
        warningPlane.transform.localScale = new Vector3(width, width, 1f);

        // 간단한 빨간 머티리얼 적용
        Renderer renderer = warningPlane.GetComponent<Renderer>();
        if (renderer != null)
        {
            UpdateWarningPlaneColor(warningPlane);
        }

        // 활성 리스트에 추가
        activeWarningPlanes.Add(warningPlane);

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] 경고 평면 생성 완료! 위치: {warningPos}");
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

    #region ===== 유틸리티 메서드 - 경고용 별도 계산 =====

    /// <summary>
    /// 경고 표시용 이동 방향 계산 (오프셋 적용)
    /// </summary>
    private Vector3 GetWarningDirection()
    {
        return (GetWarningEndPosition() - GetWarningStartPosition()).normalized;
    }

    /// <summary>
    /// 경고 표시용 경로 총 길이 계산 (오프셋 적용)
    /// </summary>
    private float GetWarningPathLength()
    {
        return Vector3.Distance(GetWarningStartPosition(), GetWarningEndPosition());
    }

    /// <summary>
    /// 경고 표시용 시작 위치 계산 (현재 큐브 위치)
    /// </summary>
    private Vector3 GetWarningStartPosition()
    {
        return transform.position;  // 현재 큐브 위치
    }

    /// <summary>
    /// 경고 표시용 목표 위치 계산 (독립적인 경고 거리)
    /// </summary>
    private Vector3 GetWarningEndPosition()
    {
        // 경고 거리 독립적으로 설정
        return transform.position + warningDistance;
    }

    /// <summary>
    /// 멧돼지 실제 이동 방향 계산 (기존 방식)
    /// </summary>
    private Vector3 GetMovementDirection()
    {
        return (GetEndPosition() - GetStartPosition()).normalized;
    }

    /// <summary>
    /// 멧돼지 실제 경로 총 길이 계산 (기존 방식)
    /// </summary>
    private float GetPathLength()
    {
        return Vector3.Distance(GetStartPosition(), GetEndPosition());
    }

    /// <summary>
    /// 멧돼지 실제 시작 위치 계산 (현재 큐브 위치)
    /// </summary>
    private Vector3 GetStartPosition()
    {
        return transform.position;  // 현재 큐브 위치
    }

    /// <summary>
    /// 멧돼지 실제 목표 위치 계산 (현재 위치 + 원본 오프셋)
    /// </summary>
    private Vector3 GetEndPosition()
    {
        return transform.position + main.startPositionOffset;  // 목표 지점
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
    /// 컴포넌트 비활성화 시 정리 (OnDestroy 대신 OnDisable 사용)
    /// </summary>
    void OnDisable()
    {
        ClearWarning();

        if (main != null && main.showDebugLog)
            Debug.Log($"[{gameObject.name}] BoarWarning 비활성화 시 정리 완료");
    }

    #endregion

    #region ===== 에디터 시각화 =====

#if UNITY_EDITOR
    /// <summary>
    /// 씬에서 경고 경로 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (main == null) return;

        // 멧돼지 실제 이동 경로 (청록색)
        Vector3 boarStartPos = GetStartPosition();
        Vector3 boarEndPos = GetEndPosition();
        float width = GetActualAttackWidth();

        Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // 청록색
        Gizmos.DrawLine(boarStartPos, boarEndPos);

        // 경고 표시 경로 (빨간색)
        Vector3 warningStartPos = GetWarningStartPosition();
        Vector3 warningEndPos = GetWarningEndPosition();

        Gizmos.color = new Color(1f, 0f, 0f, 0.7f); // 빨간색
        Gizmos.DrawLine(warningStartPos, warningEndPos);

        // 경고 영역 표시
        Vector3 warningDirection = GetWarningDirection();
        Vector3 perpendicular = Vector3.Cross(warningDirection, Vector3.up).normalized * (width / 2f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Vector3[] corners = {
            warningStartPos + perpendicular,
            warningStartPos - perpendicular,
            warningEndPos - perpendicular,
            warningEndPos + perpendicular
        };

        for (int i = 0; i < corners.Length; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % corners.Length]);
        }

        // 시작점들 표시
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(boarStartPos, 0.2f);
        Gizmos.DrawSphere(warningStartPos, 0.2f);

        // 끝점들 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(boarEndPos, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(warningEndPos, 0.2f);
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
            $"멧돼지 이동거리: {GetPathLength():F2}m\n" +
            $"경고 표시거리: {GetWarningPathLength():F2}m\n" +
            $"경고 오프셋: {warningDistance}\n" +
            $"경고 폭: {GetActualAttackWidth():F2}m"
        );
    }
#endif

    #endregion
}