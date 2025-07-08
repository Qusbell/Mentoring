using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 위에서 아래로 떨어지는 큐브가 그 아래에 있는 큐브의 윗면에 빨간색 경고 표시를 보여주는 스크립트
/// 큐브가 가까워질수록 경고 표시가 더 선명해지고, 착지 직전에 부드럽게 사라짐.
/// 오브젝트 풀링으로 성능 최적화 (프리팹용)
/// </summary>
public class WarningSystem : MonoBehaviour
{
    [Header("경고 표시 효과 설정")]
    [Tooltip("경고 색상 변화 시작 거리 비율 (0.5 = 절반 거리에서 시작)")]
    [Range(0.2f, 0.8f)]
    public float colorChangeStartRatio = 0.5f;

    [Tooltip("경고 표시 사라짐 시작 거리 (착지 전 이 거리에서 사라지기 시작)")]
    public float fadeStartDistance = 0.5f;

    [Tooltip("경고 표시 사라짐 시간 (초, 값이 클수록 천천히 사라짐)")]
    public float fadeDuration = 0.3f;

    [Header("풀링 설정")]
    [Tooltip("미리 생성할 경고 표시 개수 (권장: 5-10개, 부족하면 자동 확장)")]
    public int poolSize = 10;

    // ==================== 내부 변수들 ====================

    // 경고 표시 관련
    private GameObject targetCube;          // 아래에 있는 큐브 (발판이 생성될 큐브)
    private GameObject warningPlane;        // 현재 사용 중인 경고 표시 평면
    private Vector3 initialPosition;        // 떨어지는 큐브의 시작 위치
    private Vector3 targetPosition;         // 큐브가 착지할 목표 위치
    private float totalDistance;            // 시작 위치에서 목표까지 총 거리
    private float colorChangeStartDist;     // 색상 변화가 시작될 거리
    private bool isFading = false;          // 현재 페이드 아웃 중인지 여부
    private Material planeMaterial;         // 경고 표시의 머티리얼 (색상 조절용)

    // 오브젝트 풀링 관련
    private List<GameObject> warningPool = new List<GameObject>();  // 재사용할 발판들을 보관하는 풀
    private static WarningSystem poolOwner; // 풀을 관리할 첫 번째 인스턴스 (싱글톤 방식)

    // ==================== 고정 설정값들 ====================

    private readonly Color warningColor = Color.red;      // 경고 색상 (빨간색)
    private const float startAlpha = 0.3f;                // 초기 투명도 (30% 불투명)
    private const float maxAlpha = 0.8f;                  // 최대 투명도 (80% 불투명)
    private const float emissionIntensity = 1f;           // 발광 강도
    private const float intensityCurve = 1f;              // 색상 변화 곡선 (1 = 선형)

    // ==================== Unity 생명주기 메서드들 ====================

    void Start()
    {
        // 떨어지는 큐브의 시작 위치 저장
        initialPosition = transform.position;

        // 오브젝트 풀링: 첫 번째 WarningSystem만 풀 생성 (메모리 절약)
        if (poolOwner == null)
        {
            poolOwner = this;
            CreateWarningPool();
        }

        // 아래에 큐브가 있는지 확인하고 경고 표시 생성
        CheckForCubeBelow();
    }

    void OnEnable()
    {
        // 오브젝트가 활성화될 때도 체크 (기존 기능 유지)
        // 큐브가 비활성화→활성화되는 경우를 대비
        if (transform.position == initialPosition)
        {
            CheckForCubeBelow();
        }
    }

    void Update()
    {
        // 경고 표시가 없거나 관련 데이터가 없으면 아무것도 하지 않음
        if (warningPlane == null || targetCube == null || planeMaterial == null) return;

        // 현재 위치에서 목적지까지 남은 거리 계산
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // 큐브가 실제로 움직이고 있는지 확인 (0.05유닛 이상 이동 시)
        bool isMoving = Vector3.Distance(transform.position, initialPosition) > 0.05f;

        // 큐브가 움직이고 있고 아직 페이드 아웃 중이 아닐 때만 처리
        if (isMoving && !isFading)
        {
            // 착지 직전이면 페이드 아웃 시작
            if (distanceToTarget <= fadeStartDistance)
            {
                StartCoroutine(FadeOutWarning());
            }
            else
            {
                // 거리에 따라 경고 표시 강도 업데이트 (가까울수록 진해짐)
                UpdateWarningIntensity(distanceToTarget);
            }
        }
    }

    // ==================== 오브젝트 풀링 메서드들 ====================

    /// <summary>
    /// 게임 시작 시 경고 표시들을 미리 생성해서 풀에 보관
    /// 성능 최적화: 게임 중 생성/파괴 반복을 줄임
    /// </summary>
    private void CreateWarningPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            // Unity 기본 평면(Quad) 생성
            GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Quad);
            warning.name = "PooledWarning_" + i;

            // 불필요한 충돌체 제거 (레이캐스트 방해 방지)
            Collider col = warning.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // 레이캐스트에 감지되지 않도록 레이어 설정
            warning.layer = LayerMask.NameToLayer("Ignore Raycast");

            // 비활성화해서 풀에 보관 (화면에 보이지 않음)
            warning.SetActive(false);
            warningPool.Add(warning);
        }
    }

    /// <summary>
    /// 풀에서 사용 가능한 경고 표시를 가져옴
    /// 재사용 가능한 게 있으면 그걸 쓰고, 없으면 새로 생성
    /// </summary>
    private GameObject GetWarningFromPool()
    {
        // 다른 WarningSystem이 풀 오너라면 그쪽에 요청
        if (poolOwner != this && poolOwner != null)
        {
            return poolOwner.GetWarningFromPool();
        }

        // 풀에서 비활성화된(사용 안 중인) 경고 표시 찾기
        foreach (GameObject warning in warningPool)
        {
            if (!warning.activeInHierarchy)  // 비활성화 상태 = 사용 가능
            {
                return warning;  // 재사용!
            }
        }

        // 풀이 모자라면 새로 생성해서 풀에 추가 (동적 확장)
        GameObject newWarning = GameObject.CreatePrimitive(PrimitiveType.Quad);
        newWarning.name = "ExtraWarning";

        // 기본 설정 적용
        Collider col = newWarning.GetComponent<Collider>();
        if (col != null) Destroy(col);
        newWarning.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 풀에 추가
        warningPool.Add(newWarning);

        return newWarning;
    }

    /// <summary>
    /// 사용 완료된 경고 표시를 풀로 반환
    /// Destroy 대신 SetActive(false)로 숨겨서 나중에 재사용
    /// </summary>
    private void ReturnWarningToPool(GameObject warning)
    {
        if (warning != null)
        {
            warning.SetActive(false);  // 파괴 대신 비활성화 (재사용을 위해)
        }
    }

    // ==================== 경고 표시 생성 및 관리 ====================

    /// <summary>
    /// 떨어지는 큐브 아래에 다른 큐브가 있는지 레이캐스트로 확인
    /// 있으면 그 큐브 위에 경고 표시 생성
    /// </summary>
    private void CheckForCubeBelow()
    {
        // 레이캐스트 시 자기 자신이 감지되지 않도록 임시로 레이어 변경
        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 아래 방향으로 레이캐스트 발사 (무한 거리, Default 레이어만)
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, Mathf.Infinity, 1);

        // 가까운 순서대로 정렬 (첫 번째로 만나는 적합한 큐브 선택)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool foundValidCube = false;

        // 감지된 모든 오브젝트 검사
        foreach (RaycastHit hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;

            // 제외할 오브젝트들 필터링
            if (hitObject == gameObject) continue;        // 자기 자신 제외
            if (hitObject.CompareTag("Player")) continue;  // 플레이어 제외
            if (hitObject.CompareTag("Monster")) continue; // 몬스터 제외

            // 현재 이동 중인 큐브는 제외 (불안정한 착지점)
            CubeMover cubeMover = hitObject.GetComponent<CubeMover>();
            if (cubeMover != null && cubeMover.IsCurrentlyMoving) continue;

            // 적합한 큐브 발견!
            targetCube = hitObject;
            foundValidCube = true;

            // 착지 위치 및 경고 표시 생성을 위한 계산
            Renderer targetRenderer = targetCube.GetComponent<Renderer>();
            Renderer thisRenderer = GetComponent<Renderer>();

            if (targetRenderer != null && thisRenderer != null)
            {
                // 아래 큐브의 윗면 높이 계산
                float targetTopY = targetRenderer.bounds.center.y + targetRenderer.bounds.extents.y;
                // 떨어지는 큐브의 절반 높이
                float thisHalfHeight = thisRenderer.bounds.extents.y;

                // 떨어지는 큐브가 착지할 정확한 위치 계산
                targetPosition = new Vector3(
                    transform.position.x,
                    targetTopY + thisHalfHeight,  // 아래 큐브 위 + 자신의 절반 높이
                    transform.position.z
                );

                // 총 이동 거리 계산 (색상 변화 기준점 계산용)
                totalDistance = Vector3.Distance(initialPosition, targetPosition);

                // 색상 변화가 시작될 거리 계산
                colorChangeStartDist = totalDistance * colorChangeStartRatio;

                // 경고 표시 생성
                CreateWarningPlane(hit, targetRenderer);
            }

            break; // 첫 번째 적합한 큐브만 사용
        }

        // 레이어 원상복구
        gameObject.layer = originalLayer;

        // 적합한 큐브를 찾지 못한 경우 디버그 로그 출력
        if (!foundValidCube)
        {
            Debug.LogWarning($"[{gameObject.name}] 아래에 적합한 큐브를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 실제 경고 표시(빨간 평면)를 생성하고 설정하는 메서드
    /// 오브젝트 풀링을 사용해서 기존 발판 재사용
    /// </summary>
    private void CreateWarningPlane(RaycastHit hit, Renderer targetRenderer)
    {
        if (targetCube == null) return;

        // 이전에 사용 중인 경고 표시가 있으면 풀로 반환
        if (warningPlane != null)
        {
            ReturnWarningToPool(warningPlane);
        }

        // 풀에서 경고 표시 가져오기 (재사용 또는 새로 생성)
        warningPlane = GetWarningFromPool();
        if (warningPlane == null) return;

        // 경고 표시 활성화 및 이름 설정
        warningPlane.SetActive(true);
        warningPlane.name = "Warning_" + targetCube.name;

        // 경고 표시가 생성될 위치 계산 (아래 큐브 윗면에 살짝 위)
        float targetTopY = targetRenderer.bounds.center.y + targetRenderer.bounds.extents.y;
        Vector3 planePosition = new Vector3(
            hit.point.x,                // 레이캐스트 충돌 지점 X
            targetTopY + 0.005f,        // 아래 큐브 윗면 + 5mm (겹침 방지)
            hit.point.z                 // 레이캐스트 충돌 지점 Z
        );

        // 경고 표시 위치 및 회전 설정
        warningPlane.transform.position = planePosition;
        warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0); // 바닥에 평행

        // 경고 표시 크기를 아래 큐브 크기에 맞게 조정
        float planeSize = targetRenderer.bounds.extents.x * 2;  // 큐브 너비
        warningPlane.transform.localScale = new Vector3(planeSize, planeSize, 1f);

        // 경고 표시의 색상 및 투명도 설정
        SetupWarningMaterial();
    }

    /// <summary>
    /// 경고 표시의 머티리얼(색상, 투명도, 발광 등) 설정
    /// 빨간색 반투명 발광 효과 적용
    /// </summary>
    private void SetupWarningMaterial()
    {
        if (warningPlane == null) return;

        Renderer planeRenderer = warningPlane.GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            // 새 머티리얼 생성 (Unity 표준 셰이더 사용)
            planeMaterial = new Material(Shader.Find("Standard"));

            // 반투명 설정 (알파 블렌딩)
            planeMaterial.SetFloat("_Mode", 3); // Transparent 모드
            planeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            planeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            planeMaterial.SetInt("_ZWrite", 0);
            planeMaterial.DisableKeyword("_ALPHATEST_ON");
            planeMaterial.EnableKeyword("_ALPHABLEND_ON");
            planeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            planeMaterial.renderQueue = 3000;

            // 초기 색상 설정 (빨간색, 30% 불투명)
            Color color = warningColor;
            color.a = startAlpha;
            planeMaterial.color = color;

            // 발광 효과 설정 (빨간 빛이 나도록)
            planeMaterial.EnableKeyword("_EMISSION");
            planeMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);

            // 머티리얼을 경고 표시에 적용
            planeRenderer.material = planeMaterial;
        }
    }

    // ==================== 경고 표시 효과 업데이트 ====================

    /// <summary>
    /// 떨어지는 큐브와 목표 지점 사이의 거리에 따라 경고 표시의 강도 조절
    /// 가까워질수록 더 선명하고 밝게, 멀수록 흐리게
    /// </summary>
    private void UpdateWarningIntensity(float currentDistance)
    {
        // 전체 거리 대비 현재 거리 비율 계산 (1.0 = 멀음, 0.0 = 가까움)
        float distanceRatio = Mathf.Clamp01(currentDistance / totalDistance);

        // 색상 변화 구간에 아직 도달하지 않았으면 초기 색상 유지
        if (distanceRatio > colorChangeStartRatio)
        {
            Color color = warningColor;
            color.a = startAlpha;  // 초기 투명도 유지
            planeMaterial.color = color;
            planeMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);
            return;
        }

        // 색상 변화 진행률 계산 (0.0 = 변화 시작, 1.0 = 최대 강도)
        float changeProgress = 1f - (distanceRatio / colorChangeStartRatio);

        // 부드러운 변화를 위한 곡선 적용
        float curvedProgress = Mathf.Pow(changeProgress, intensityCurve);

        // 진행률에 따라 투명도 계산 (startAlpha → maxAlpha)
        float alpha = Mathf.Lerp(startAlpha, maxAlpha, curvedProgress);

        // 새로운 색상 적용
        Color newColor = warningColor;
        newColor.a = alpha;
        planeMaterial.color = newColor;

        // 발광 강도도 함께 증가 (더 밝게 빛남)
        float emissionStrength = alpha * emissionIntensity;
        planeMaterial.SetColor("_EmissionColor", warningColor * emissionStrength);
    }

    /// <summary>
    /// 착지 직전에 경고 표시를 서서히 사라지게 하는 효과
    /// fadeDuration 시간에 걸쳐 완전 투명해질 때까지 점진적으로 페이드
    /// </summary>
    private IEnumerator FadeOutWarning()
    {
        // 유효성 검사
        if (warningPlane == null || planeMaterial == null) yield break;

        isFading = true;  // 페이드 중 플래그 설정

        // 현재 색상과 발광 색상 저장 (페이드 시작점)
        Color startColor = planeMaterial.color;
        Color emissionColor = planeMaterial.GetColor("_EmissionColor");

        // 페이드 아웃 루프
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // 페이드 진행률 계산 (0.0 → 1.0)
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            // 투명도 점진적 감소 (현재 투명도 → 0)
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(startColor.a, 0f, t);
            planeMaterial.color = newColor;

            // 발광도 점진적 감소 (현재 발광 → 검은색)
            Color newEmission = Color.Lerp(emissionColor, Color.black, t);
            planeMaterial.SetColor("_EmissionColor", newEmission);

            yield return null;  // 다음 프레임까지 대기
        }

        // 페이드 완료 후 경고 표시 완전 제거
        RemoveWarning();
    }

    // ==================== 정리 및 안전장치 메서드들 ====================

    /// <summary>
    /// 경고 표시 제거 및 풀로 반환
    /// 메모리 누수 방지를 위한 핵심 정리 메서드
    /// </summary>
    private void RemoveWarning()
    {
        if (warningPlane != null)
        {
            ReturnWarningToPool(warningPlane);  // 파괴 대신 풀로 반환 (재사용)
            warningPlane = null;                // 참조 해제
        }
        isFading = false;  // 페이드 상태 초기화
    }

    /// <summary>
    /// Unity 이벤트: 오브젝트가 비활성화될 때 자동 호출
    /// 큐브가 SetActive(false)되거나 씬 전환 시 안전장치 역할
    /// </summary>
    void OnDisable()
    {
        RemoveWarning();  // 비활성화 시 경고 표시도 정리
    }

    /// <summary>
    /// Unity 이벤트: 오브젝트가 파괴될 때 자동 호출
    /// 게임 종료, 씬 전환, Destroy() 호출 시 메모리 누수 방지
    /// </summary>
    void OnDestroy()
    {
        RemoveWarning();  // 파괴 시 경고 표시도 정리 (메모리 누수 방지)
    }
}