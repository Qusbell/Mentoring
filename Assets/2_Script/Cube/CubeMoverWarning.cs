using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 기존 WarningSystem을 베이스로 한 CubeMoverWarning
/// 큐브 생성 시 경로에 워닝 표시 → 큐브 이동 시작 → 큐브가 지나간 지점의 워닝 순차 제거
/// </summary>
[RequireComponent(typeof(CubeMover))]
public class CubeMoverWarning : MonoBehaviour
{
    [Header("플레이어 대피 시간")]
    [Tooltip("큐브가 날아오기 전에 플레이어가 피할 수 있는 시간 (초)\n• 2초: 보통 난이도\n• 3초: 쉬운 난이도\n• 1초: 어려운 난이도")]
    [Range(1f, 5f)]
    public float warningToMoveDelay = 2f;

    [Header("위험 지역 표시")]
    [Tooltip("빨간 워닝이 얼마나 정확하게 표시될지 결정\n• 0.3: 매우 정밀 (성능 부하 높음)\n• 0.5: 적당함 (권장)\n• 1.0: 대략적 (성능 좋음)")]
    [Range(0.2f, 1f)]
    public float pathCheckInterval = 0.5f;

    [Tooltip("큐브가 지나간 후 워닝이 사라지는 타이밍\n• 1.0: 큐브가 완전히 지나가야 사라짐\n• 0.5: 큐브가 가까이 오면 빨리 사라짐")]
    [Range(0.5f, 2f)]
    public float passedDistance = 1f;

    [Header("충돌 감지 설정")]
    [Tooltip("어떤 바닥에 워닝을 표시할지 결정\n• Default: 기본 바닥\n• Cube: 큐브 바닥\n※ 둘 다 체크 권장")]
    public LayerMask detectLayerMask = (1 << 0) | (1 << LayerMask.NameToLayer("Cube"));

    [Tooltip("얼마나 아래까지 바닥을 찾을지 (미터)\n• 높은 맵: 100\n• 낮은 맵: 20-50")]
    [Range(10f, 100f)]
    public float raycastDistance = 50f;

    [Header("성능 최적화")]
    [Tooltip("동시에 표시할 수 있는 최대 워닝 개수\n• 많은 큐브: 15-20개\n• 적은 큐브: 5-10개")]
    [Range(5, 20)]
    public int poolSize = 15;

    [Header("개발자 전용")]
    [Tooltip("개발 중 디버그 메시지 출력 (빌드 시 끄기)")]
    public bool showDebugLog = false;

    // ==================== 고정된 시각 효과 ====================
    private static readonly Color WarningColor = Color.red;
    private const float WarningAlpha = 0.8f;
    private const float EmissionIntensity = 1f;
    private const float WarningHeightOffset = 0.01f;
    private const float WarningSizeMultiplier = 0.9f;

    // ==================== 컴포넌트 참조 ====================
    private CubeMover cubeMover;
    private Vector3 cubeSize;
    private Vector3 startPosition;
    private Vector3 endPosition;

    // ==================== 워닝 관리 ====================
    private List<WarningData> warningList = new List<WarningData>();
    private bool isWarningActive = false;
    private bool hasStartedMoving = false;

    // ==================== 오브젝트 풀링 ====================
    private List<GameObject> warningPool = new List<GameObject>();
    private static CubeMoverWarning poolOwner;

    // ==================== 워닝 데이터 클래스 ====================
    [System.Serializable]
    private class WarningData
    {
        public Vector3 worldPosition;      // 월드 좌표
        public Vector3 pathPosition;       // 경로상 위치 (큐브가 지나가는 위치)
        public GameObject targetCube;      // 아래에 있는 큐브
        public GameObject warningPlane;    // 워닝 표시 오브젝트
        public float pathProgress;         // 경로 진행률 (0~1)
        public bool hasPassed;             // 큐브가 지나갔는지 여부

        public WarningData(Vector3 worldPos, Vector3 pathPos, GameObject cube, float progress)
        {
            worldPosition = worldPos;
            pathPosition = pathPos;
            targetCube = cube;
            pathProgress = progress;
            hasPassed = false;
        }
    }

    // ==================== Unity 생명주기 ====================
    void Awake()
    {
        cubeMover = GetComponent<CubeMover>();
        if (cubeMover == null)
        {
            Debug.LogError($"[{gameObject.name}] CubeMover 컴포넌트를 찾을 수 없습니다!");
            this.enabled = false;
            return;
        }

        // 풀링 시스템 초기화
        if (poolOwner == null)
        {
            poolOwner = this;
            CreateWarningPool();
        }
    }

    void Start()
    {
        CalculateCubeSize();
    }

    void OnEnable()
    {
        // 큐브가 활성화되면 워닝 시스템 시작
        if (!isWarningActive)
        {
            StartCoroutine(StartWarningSequence());
        }
    }

    void Update()
    {
        // 큐브가 움직이기 시작한 후에만 지나간 지점 체크
        if (hasStartedMoving && warningList.Count > 0)
        {
            CheckPassedWarnings();

            // 모든 워닝이 제거되면 컴포넌트 비활성화 (성능 최적화)
            bool hasActiveWarnings = false;
            foreach (WarningData warning in warningList)
            {
                if (!warning.hasPassed)
                {
                    hasActiveWarnings = true;
                    break;
                }
            }

            if (!hasActiveWarnings)
            {
                this.enabled = false;
            }
        }
    }

    // ==================== 워닝 시퀀스 메인 로직 ====================
    private IEnumerator StartWarningSequence()
    {
        if (isWarningActive) yield break;

        isWarningActive = true;
        hasStartedMoving = false;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 워닝 시퀀스 시작!");

        // 1. 큐브 경로 분석 및 워닝 생성
        GetCubeMoverSettings();
        AnalyzeMovementPath();
        CreateAllWarningPlanes();

        // 2. 모든 워닝 표시 활성화
        ActivateAllWarnings();

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 워닝 표시 완료. {warningToMoveDelay}초 후 큐브 이동 시작.");

        // 3. 대기 시간 (플레이어가 피할 수 있는 시간)
        yield return new WaitForSeconds(warningToMoveDelay);

        // 4. 큐브 이동 시작
        StartCubeMovement();
        hasStartedMoving = true;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 큐브 이동 시작! 지나간 지점의 워닝을 순차 제거합니다.");
    }

    // ==================== 경로 분석 및 워닝 생성 ====================
    private void GetCubeMoverSettings()
    {
        if (cubeMover == null) return;

        // CubeMover가 아직 움직이지 않았을 때의 원래 위치가 목표점
        endPosition = cubeMover.transform.position;
        startPosition = endPosition + cubeMover.startPositionOffset;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 경로: {startPosition} → {endPosition}");
    }

    private void CalculateCubeSize()
    {
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        if (allColliders.Length == 0)
        {
            cubeSize = transform.lossyScale;
            return;
        }

        Bounds combinedBounds = allColliders[0].bounds;
        for (int i = 1; i < allColliders.Length; i++)
            combinedBounds.Encapsulate(allColliders[i].bounds);
        cubeSize = combinedBounds.size;
    }

    private void AnalyzeMovementPath()
    {
        CleanupWarnings();

        float pathLength = Vector3.Distance(startPosition, endPosition);
        if (pathLength < 0.1f) return;

        int checkPoints = Mathf.CeilToInt(pathLength / pathCheckInterval);
        checkPoints = Mathf.Clamp(checkPoints, 2, 30);

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 경로 분석: {checkPoints}개 지점 체크");

        for (int i = 0; i <= checkPoints; i++)
        {
            float progress = (float)i / checkPoints;
            Vector3 currentPathPosition = Vector3.Lerp(startPosition, endPosition, progress);
            CheckCubeAreaForWarnings(currentPathPosition, progress);
        }

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 총 {warningList.Count}개의 워닝 지점 발견");
    }

    private void CheckCubeAreaForWarnings(Vector3 centerPosition, float progress)
    {
        Vector3 halfSize = cubeSize * 0.5f;
        Vector3[] checkPoints = new Vector3[]
        {
            centerPosition + new Vector3(-halfSize.x, 0, -halfSize.z),
            centerPosition + new Vector3(halfSize.x, 0, -halfSize.z),
            centerPosition + new Vector3(-halfSize.x, 0, halfSize.z),
            centerPosition + new Vector3(halfSize.x, 0, halfSize.z),
            centerPosition
        };

        foreach (Vector3 checkPoint in checkPoints)
        {
            if (Physics.Raycast(checkPoint, Vector3.down, out RaycastHit hit, raycastDistance, detectLayerMask))
            {
                GameObject hitCube = hit.collider.gameObject;
                if (hitCube == this.gameObject) continue;

                // 이동 중인 큐브는 제외
                CubeMover hitCubeMover = hitCube.GetComponent<CubeMover>();
                if (hitCubeMover != null && hitCubeMover.IsCurrentlyMoving) continue;

                // 중복 체크
                bool alreadyExists = false;
                foreach (WarningData existing in warningList)
                {
                    if (existing.targetCube == hitCube &&
                        Vector3.Distance(existing.worldPosition, checkPoint) < pathCheckInterval * 0.8f)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    warningList.Add(new WarningData(checkPoint, centerPosition, hitCube, progress));
                }
            }
        }
    }

    // ==================== 워닝 시각화 ====================
    private void CreateAllWarningPlanes()
    {
        foreach (WarningData warning in warningList)
        {
            if (warning.targetCube != null)
            {
                GameObject warningPlane = GetWarningFromPool();
                if (warningPlane != null)
                {
                    SetupWarningPlane(warningPlane, warning.targetCube, warning.worldPosition);
                    warning.warningPlane = warningPlane;
                }
            }
        }
    }

    private void SetupWarningPlane(GameObject warningPlane, GameObject targetCube, Vector3 worldPos)
    {
        warningPlane.name = "Warning_" + targetCube.name;

        Renderer targetRenderer = targetCube.GetComponent<Renderer>();
        float topY = targetRenderer != null
            ? targetRenderer.bounds.center.y + targetRenderer.bounds.extents.y
            : targetCube.transform.position.y + 0.6f;

        warningPlane.transform.position = new Vector3(worldPos.x, topY + WarningHeightOffset, worldPos.z);
        warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0);
        warningPlane.transform.localScale = new Vector3(
            cubeSize.x * WarningSizeMultiplier,
            cubeSize.z * WarningSizeMultiplier,
            1f
        );

        SetupWarningMaterial(warningPlane);
    }

    private void SetupWarningMaterial(GameObject warningPlane)
    {
        Renderer renderer = warningPlane.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material warningMat = new Material(Shader.Find("Standard"));

            // 색상 설정
            Color finalColor = WarningColor;
            finalColor.a = WarningAlpha;
            warningMat.color = finalColor;

            // 반투명 설정
            warningMat.SetFloat("_Mode", 3);
            warningMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            warningMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            warningMat.SetInt("_ZWrite", 0);
            warningMat.DisableKeyword("_ALPHATEST_ON");
            warningMat.EnableKeyword("_ALPHABLEND_ON");
            warningMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            warningMat.renderQueue = 3000;

            // 발광 효과
            warningMat.EnableKeyword("_EMISSION");
            warningMat.SetColor("_EmissionColor", WarningColor * EmissionIntensity);

            renderer.material = warningMat;
        }
    }

    private void ActivateAllWarnings()
    {
        int activatedCount = 0;
        foreach (WarningData warning in warningList)
        {
            if (warning.warningPlane != null)
            {
                warning.warningPlane.SetActive(true);
                activatedCount++;
            }
        }

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] {activatedCount}개 워닝 표시 활성화");
    }

    // ==================== 큐브 이동 관리 ====================
    private void StartCubeMovement()
    {
        if (cubeMover == null)
        {
            Debug.LogError($"[{gameObject.name}] CubeMover가 없어서 큐브를 이동시킬 수 없습니다!");
            return;
        }

        // CubeMover는 OnEnable에서 자동으로 이동을 시작하므로
        // 여기서는 특별한 처리가 필요 없음
        // 이미 gameObject.SetActive(true) 상태이기 때문
    }

    // ==================== 지나간 워닝 체크 및 제거 ====================
    private void CheckPassedWarnings()
    {
        Vector3 cubeCurrentPos = transform.position;
        int passedCount = 0;

        foreach (WarningData warning in warningList)
        {
            if (warning.hasPassed)
            {
                passedCount++;
                continue;
            }

            if (warning.warningPlane != null && warning.warningPlane.activeInHierarchy)
            {
                // 큐브가 해당 워닝 지점을 지나갔는지 체크
                float distanceToWarning = Vector3.Distance(cubeCurrentPos, warning.pathPosition);

                if (distanceToWarning <= passedDistance)
                {
                    // 워닝 제거
                    warning.hasPassed = true;
                    passedCount++;
                    StartCoroutine(FadeOutWarning(warning));

                    if (showDebugLog)
                        Debug.Log($"[{gameObject.name}] 워닝 지점 통과! 워닝 제거: {warning.warningPlane.name}");
                }
            }
            else
            {
                // 비활성화된 워닝은 이미 지나간 것으로 처리
                warning.hasPassed = true;
                passedCount++;
            }
        }

        // 성능 로그 (디버그용)
        if (showDebugLog && passedCount == warningList.Count)
        {
            Debug.Log($"[{gameObject.name}] 모든 워닝 지점 통과 완료!");
        }
    }

    private IEnumerator FadeOutWarning(WarningData warningData)
    {
        if (warningData.warningPlane == null) yield break;

        GameObject warningPlane = warningData.warningPlane;
        Renderer renderer = warningPlane.GetComponent<Renderer>();

        if (renderer != null && renderer.material != null)
        {
            Material mat = renderer.material;
            Color startColor = mat.color;
            Color emissionColor = mat.GetColor("_EmissionColor");

            float fadeDuration = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                // 중간에 오브젝트가 파괴되었는지 체크
                if (warningPlane == null || mat == null) yield break;

                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;

                // 색상 페이드
                Color newColor = startColor;
                newColor.a = Mathf.Lerp(startColor.a, 0f, t);
                mat.color = newColor;

                // 발광 페이드
                Color newEmission = Color.Lerp(emissionColor, Color.black, t);
                mat.SetColor("_EmissionColor", newEmission);

                yield return null;
            }
        }

        // 워닝을 풀로 반환
        ReturnWarningToPool(warningPlane);
        warningData.warningPlane = null;
    }

    // ==================== 오브젝트 풀링 시스템 ====================
    private void CreateWarningPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Quad);
            warning.name = "PooledWarning_" + i;
            Destroy(warning.GetComponent<Collider>());
            warning.layer = LayerMask.NameToLayer("Ignore Raycast");
            warning.SetActive(false);
            warningPool.Add(warning);
        }
    }

    private GameObject GetWarningFromPool()
    {
        if (poolOwner != this && poolOwner != null)
            return poolOwner.GetWarningFromPool();

        foreach (GameObject warning in warningPool)
            if (!warning.activeInHierarchy)
                return warning;

        // 풀 확장
        GameObject newWarning = GameObject.CreatePrimitive(PrimitiveType.Quad);
        newWarning.name = "ExtraWarning_" + Random.Range(1000, 9999);
        Destroy(newWarning.GetComponent<Collider>());
        newWarning.layer = LayerMask.NameToLayer("Ignore Raycast");
        warningPool.Add(newWarning);
        return newWarning;
    }

    private void ReturnWarningToPool(GameObject warning)
    {
        if (warning != null)
            warning.SetActive(false);
    }

    // ==================== 정리 ====================
    private void CleanupWarnings()
    {
        foreach (WarningData warning in warningList)
        {
            if (warning.warningPlane != null)
                ReturnWarningToPool(warning.warningPlane);
        }
        warningList.Clear();
    }

    public void ResetWarningSystem()
    {
        CleanupWarnings();
        isWarningActive = false;
        hasStartedMoving = false;
        this.enabled = true;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 워닝 시스템 리셋 완료");
    }

    void OnDisable()
    {
        CleanupWarnings();
        isWarningActive = false;
        hasStartedMoving = false;
    }

    void OnDestroy()
    {
        CleanupWarnings();
    }

    // ==================== 디버그 및 테스트 ====================
    void OnDrawGizmos()
    {
        if (!showDebugLog || !Application.isPlaying) return;

        // 경로 표시
        if (startPosition != Vector3.zero && endPosition != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPosition, endPosition);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPosition, 0.2f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPosition, 0.2f);
        }

        // 워닝 지점들 표시
        Gizmos.color = Color.red;
        foreach (WarningData warning in warningList)
        {
            if (!warning.hasPassed)
                Gizmos.DrawSphere(warning.pathPosition, 0.1f);
        }
    }
}