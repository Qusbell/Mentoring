using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 멧돼지 경고 표시 관리 (점진적 표시)
/// 멧돼지 실제 이동 거리와 경고 표시 거리를 X, Y, Z 개별로 설정 가능
/// 경고 표시가 점진적으로 확장됨
/// </summary>
public class BoarWarning : MonoBehaviour
{
    #region ===== 설정 =====

    [Header("경고 시간 설정")]
    [Tooltip("경고 표시 지속 시간 (초)")]
    public float warningDuration = 1f;

    [Tooltip("워닝 표시 시간 비율 (0~1, 전체 시간 높을 수록 빨리 생김)")]
    [Range(0.1f, 0.8f)]
    public float expansionRatio = 0.3f;

    [Header("경고 거리 설정 (기획자 조절)")]
    [Tooltip("경고 표시 거리 (멧돼지 이동 거리와 독립적)")]
    public Vector3 warningDistance = new Vector3(10, 0, 0);

    [Header("경고 표시 설정")]
    [Tooltip("경고 평면을 바닥에서 얼마나 띄울지 (미터)")]
    public float warningHeightOffset = 0.1f;

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
    /// 경고 표시 시작 (점진적으로 확장)
    /// </summary>
    public void ShowWarning()
    {
        // 기존 경고 제거
        ClearWarning();

        // 점진적 워닝 시작
        StartCoroutine(GradualWarningCoroutine());
    }

    /// <summary>
    /// 모든 경고 표시 제거
    /// </summary>
    public void ClearWarning()
    {
        // 진행 중인 코루틴 중지
        StopAllCoroutines();

        foreach (GameObject plane in activeWarningPlanes)
        {
            ReturnWarningPlaneToPool(plane);
        }

        activeWarningPlanes.Clear();
    }

    /// <summary>
    /// 경고 지속 시간 반환
    /// </summary>
    public float GetWarningDuration()
    {
        return warningDuration;
    }

    #endregion

    #region ===== 내부 메서드 - 점진적 워닝 =====

    /// <summary>
    /// 점진적으로 워닝을 표시하는 코루틴
    /// </summary>
    private IEnumerator GradualWarningCoroutine()
    {
        // 경로 계산
        Vector3 direction = GetWarningDirection();
        float pathLength = GetWarningPathLength();
        float actualWidth = GetActualAttackWidth();

        // 전체 세그먼트 수 계산
        float segmentSpacing = Mathf.Min(actualWidth * 0.5f, 2f);
        int totalSegments = Mathf.CeilToInt(pathLength / segmentSpacing);
        totalSegments = Mathf.Max(1, totalSegments);

        Vector3 startPos = GetWarningStartPosition();
        Vector3 endPos = GetWarningEndPosition();

        // 워닝 확장 시간 설정
        float expansionTime = warningDuration * expansionRatio;
        float segmentDelay = expansionTime / totalSegments;

        // 점진적으로 세그먼트 추가
        for (int i = 0; i < totalSegments; i++)
        {
            float t = (float)i / totalSegments;
            Vector3 segmentPos = Vector3.Lerp(startPos, endPos, t);

            // 그라데이션 없이 균일한 알파값 사용
            CreateWarningPlaneAt(segmentPos, actualWidth, main.warningAlpha);

            // 다음 세그먼트까지 대기
            yield return new WaitForSeconds(segmentDelay);
        }

        // 워닝 완료 후 남은 시간 대기
        float remainingTime = warningDuration - expansionTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
    }

    /// <summary>
    /// 지정된 위치에 경고 평면 생성 (균일한 알파 적용)
    /// </summary>
    private void CreateWarningPlaneAt(Vector3 position, float width, float alpha)
    {
        GameObject warningPlane = GetWarningPlaneFromPool();

        // 멧돼지 큐브의 밑바닥 높이 계산
        float boarBottomY = transform.position.y - 1.5f;
        Vector3 warningPos = new Vector3(position.x, boarBottomY + warningHeightOffset, position.z);
        warningPlane.transform.position = warningPos;

        // 회전 설정 (지면과 평행)
        warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0);

        // 크기 설정
        warningPlane.transform.localScale = new Vector3(width, width, 1f);

        // 균일한 색상 적용
        Renderer renderer = warningPlane.GetComponent<Renderer>();
        if (renderer != null)
        {
            UpdateWarningPlaneColor(warningPlane, alpha);
        }

        // 활성 리스트에 추가
        activeWarningPlanes.Add(warningPlane);
    }

    /// <summary>
    /// 경고 평면의 색상 업데이트 (균일한 알파 적용)
    /// </summary>
    private void UpdateWarningPlaneColor(GameObject plane, float alpha)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer == null) return;

        // 인스턴스 머티리얼 생성 (개별 색상 적용을 위해)
        Material instanceMaterial = new Material(sharedWarningMaterial);

        // 균일한 색상 설정
        Color finalColor = warningColor;
        finalColor.a = alpha; // 모든 평면이 같은 알파값
        instanceMaterial.color = finalColor;

        // 발광 색상 설정
        instanceMaterial.SetColor("_EmissionColor", warningColor * (alpha * 0.5f));

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

        return CreateWarningPlane();
    }

    /// <summary>
    /// 경고 평면을 풀로 반환
    /// </summary>
    private static void ReturnWarningPlaneToPool(GameObject plane)
    {
        if (plane == null) return;

        plane.SetActive(false);

        if (warningPlanePool.Count < POOL_SIZE * 2)
        {
            warningPlanePool.Enqueue(plane);
        }
        else
        {
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
        return transform.position;
    }

    /// <summary>
    /// 경고 표시용 목표 위치 계산 (독립적인 경고 거리)
    /// </summary>
    private Vector3 GetWarningEndPosition()
    {
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
        return transform.position;
    }

    /// <summary>
    /// 멧돼지 실제 목표 위치 계산 (현재 위치 + 원본 오프셋)
    /// </summary>
    private Vector3 GetEndPosition()
    {
        return transform.position + main.startPositionOffset;
    }

    /// <summary>
    /// 실제 공격 폭 계산
    /// </summary>
    private float GetActualAttackWidth()
    {
        if (main.attackWidth > 0f)
            return main.attackWidth;

        Renderer cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            return Mathf.Max(cubeRenderer.bounds.size.x, cubeRenderer.bounds.size.z);
        }

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

        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawLine(boarStartPos, boarEndPos);

        // 경고 표시 경로 (균일한 빨간색)
        Vector3 warningStartPos = GetWarningStartPosition();
        Vector3 warningEndPos = GetWarningEndPosition();

        Gizmos.color = new Color(1f, 0f, 0f, 0.7f); // 균일한 빨간색
        Gizmos.DrawLine(warningStartPos, warningEndPos);

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
#endif

    #endregion
}