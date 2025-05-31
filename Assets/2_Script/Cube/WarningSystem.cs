using UnityEngine;
using System.Collections;

/// <summary>
/// 위에서 아래로 떨어지는 큐브가 그 아래에 있는 큐브의 윗면에 빨간색 경고 표시를 보여주는 스크립트
/// 큐브가 가까워질수록 경고 표시가 더 선명해지고, 착지 직전에 부드럽게 사라짐.
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

    // 내부 변수 (수정할 필요 없음)
    private GameObject targetCube;          // 아래에 있는 큐브
    private GameObject warningPlane;        // 경고 표시 평면
    private Vector3 initialPosition;        // 시작 위치
    private Vector3 targetPosition;         // 목적지 위치
    private float totalDistance;            // 총 이동 거리
    private float colorChangeStartDist;     // 색상 변화 시작 거리
    private bool isFading = false;          // 사라지는 중인지 여부
    private Material planeMaterial;         // 경고 표시 재질

    // 고정 설정값 (수정하려면 코드 편집 필요)
    private readonly Color warningColor = Color.red;      // 경고 색상 (빨간색)
    private const float startAlpha = 0.3f;                // 초기 투명도 (0.3 = 70% 투명)
    private const float maxAlpha = 0.8f;                  // 최대 투명도 (0.8 = 20% 투명)
    private const float emissionIntensity = 1f;           // 발광 강도
    private const float intensityCurve = 1f;              // 색상 변화 곡선 (1 = 일정한 속도)

    
    //   void Awake()
    //   {
    //       // 시작 시 중요 정보 출력
    //       Debug.Log("경고 시스템: 큐브 감지를 위해 Default 레이어를 사용합니다.");
    //   }

    void Start()
    {
        // 초기 위치 저장
        initialPosition = transform.position;

        // 아래에 있는 큐브 체크 및 경고 표시
        CheckForCubeBelow();
    }

    void OnEnable()
    {
        // 활성화될 때도 체크
        if (transform.position == initialPosition) // 초기 위치일 때만 체크
        {
            CheckForCubeBelow();
        }
    }


    private void CheckForCubeBelow()
    {
        // 자기 자신을 레이캐스트에서 제외하기 위해 임시로 레이어 변경
        int originalLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 모든 충돌 오브젝트 감지 (무한 거리로 설정하여 거리 제한 없이 감지)
        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, Mathf.Infinity, 1);

        // 디버그: 감지된 오브젝트 수 확인
        Debug.Log("레이캐스트로 감지된 오브젝트 수: " + hits.Length);

        // 충돌 거리에 따라 정렬 (가까운 것부터)
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        bool foundValidCube = false;

        foreach (RaycastHit hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;

            // 디버그: 감지된 오브젝트 정보
            Debug.Log("- 감지된 오브젝트: " + hitObject.name + " (거리: " + hit.distance + " 유닛)");

            // 자기 자신 제외
            if (hitObject == gameObject) continue;

            // 플레이어 제외 (태그로 확인)
            if (hitObject.CompareTag("Player"))
            {
                Debug.Log("플레이어 오브젝트 제외: " + hitObject.name);
                continue;
            }

            // 현재 이동 중인 큐브 제외 (CubeMover가 있고 현재 움직이고 있는 큐브)
            CubeMover cubeMover = hitObject.GetComponent<CubeMover>();
            if (cubeMover != null && cubeMover.IsCurrentlyMoving)
            {
                Debug.Log("현재 이동 중인 큐브 제외: " + hitObject.name);
                continue;
            }

            // 적합한 큐브 찾음
            targetCube = hitObject;
            foundValidCube = true;

            // 디버그: 선택된 큐브 확인
            Debug.Log("선택된 큐브: " + targetCube.name + " (거리: " + hit.distance + " 유닛)");

            // 큐브 처리 로직
            Renderer targetRenderer = targetCube.GetComponent<Renderer>();
            Renderer thisRenderer = GetComponent<Renderer>();

            if (targetRenderer != null && thisRenderer != null)
            {
                float targetTopY = targetRenderer.bounds.center.y + targetRenderer.bounds.extents.y;
                float thisHalfHeight = thisRenderer.bounds.extents.y;

                // 큐브가 착지할 위치 계산
                targetPosition = new Vector3(
                    transform.position.x,
                    targetTopY + thisHalfHeight,
                    transform.position.z
                );

                // 시작 위치에서 목적지까지 총 거리 계산
                totalDistance = Vector3.Distance(initialPosition, targetPosition);

                // 색상 변화 시작 거리 계산
                colorChangeStartDist = totalDistance * colorChangeStartRatio;

                // 빨간색 경고 표시 생성
                CreateWarningPlane(hit, targetRenderer);
            }

            break; // 첫 번째 적합한 큐브 찾았으므로 중단
        }

        // 원래 레이어로 복원
        gameObject.layer = originalLayer;

        if (!foundValidCube)
        {
            Debug.LogWarning("아래에 적합한 큐브를 찾을 수 없습니다.");
        }
    }

    // 경고 표시 생성 
    private void CreateWarningPlane(RaycastHit hit, Renderer targetRenderer)
    {
        if (targetCube == null) return;

        // 이전에 생성된 경고 표시가 있으면 제거
        if (warningPlane != null)
        {
            Destroy(warningPlane);
        }

        // 큐브 윗면 위치 계산
        float targetTopY = targetRenderer.bounds.center.y + targetRenderer.bounds.extents.y;
        Vector3 planePosition = new Vector3(
            hit.point.x,
            targetTopY + 0.005f, // 높이를 아주 살짝 올림 (5mm)
            hit.point.z
        );
        // 디버그: 경고 표시 위치 확인
        Debug.Log("경고 표시 생성 위치: " + planePosition + ", 감지된 큐브: " + targetCube.name);

        // 평면 생성
        warningPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        warningPlane.name = "Warning_" + targetCube.name;

        // 위치 및 회전 설정
        warningPlane.transform.position = planePosition;
        warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0); // 바닥에 평행하게 설정

        // 크기 설정 (큐브 크기에 맞게)
        float planeSize = targetRenderer.bounds.extents.x * 2;
        warningPlane.transform.localScale = new Vector3(planeSize, planeSize, 1f);

        // 충돌체 제거 (필요 없음)
        Collider planeCollider = warningPlane.GetComponent<Collider>();
        if (planeCollider != null)
        {
            Destroy(planeCollider);
        }

        // 경고 표시를 Ignore Raycast 레이어로 설정하여 다른 레이캐스트에 감지되지 않도록 함
        warningPlane.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 머티리얼 설정
        Renderer planeRenderer = warningPlane.GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            planeMaterial = new Material(Shader.Find("Standard"));

            // 반투명 설정
            planeMaterial.SetFloat("_Mode", 3); // 투명 모드
            planeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            planeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            planeMaterial.SetInt("_ZWrite", 0);
            planeMaterial.DisableKeyword("_ALPHATEST_ON");
            planeMaterial.EnableKeyword("_ALPHABLEND_ON");
            planeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            planeMaterial.renderQueue = 3000;

            // 초기 색상 설정 
            Color color = warningColor;
            color.a = startAlpha; // 시작 투명도 설정
            planeMaterial.color = color;

            // 발광 효과 (처음에는 약하게)
            planeMaterial.EnableKeyword("_EMISSION");
            planeMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);

            // 머티리얼 적용
            planeRenderer.material = planeMaterial;
        }
    }

    void Update()
    {
        // 경고 표시가 없거나 큐브가 움직이지 않았으면 아무것도 하지 않음
        if (warningPlane == null || targetCube == null || planeMaterial == null) return;

        // 현재 목적지까지 거리 계산
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // 움직임 감지 (초기 위치에서 0.05 이상 움직였는지)
        bool isMoving = Vector3.Distance(transform.position, initialPosition) > 0.05f;

        if (isMoving && !isFading)
        {
            // 목적지까지 남은 거리가 페이드 시작 거리보다 작으면 사라지기 시작
            if (distanceToTarget <= fadeStartDistance)
            {
                StartCoroutine(FadeOutWarning());
            }
            else
            {
                // 색상 선명도 계산 (거리가 줄어들수록 더 선명해짐)
                UpdateWarningIntensity(distanceToTarget);
            }
        }
    }

    // 거리에 따라 경고 표시 선명도 업데이트
    private void UpdateWarningIntensity(float currentDistance)
    {
        // 거리 비율 계산 (1 = 먼 거리, 0 = 가까운 거리)
        float distanceRatio = Mathf.Clamp01(currentDistance / totalDistance);

        // 색상 변화 시작점 이후의 비율만 사용
        if (distanceRatio > colorChangeStartRatio)
        {
            // 아직 색상 변화 구간에 도달하지 않음 - 초기 색상 유지
            Color color = warningColor;
            color.a = startAlpha;  // 초기 투명도 유지
            planeMaterial.color = color;
            planeMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);
            return;
        }

        // 변화 진행 비율 계산 (0 = 변화 시작, 1 = 최대 강도)
        float changeProgress = 1f - (distanceRatio / colorChangeStartRatio);

        // 비선형 곡선 적용 (더 자연스러운 변화를 위해)
        float curvedProgress = Mathf.Pow(changeProgress, intensityCurve);

        // 투명도 계산 (startAlpha에서 maxAlpha로 변화)
        float alpha = Mathf.Lerp(startAlpha, maxAlpha, curvedProgress);

        // 색상 및 발광 효과 업데이트
        Color newColor = warningColor;
        newColor.a = alpha;  // 새 투명도 적용
        planeMaterial.color = newColor;

        // 발광 강도도 함께 증가
        float emissionStrength = alpha * emissionIntensity;
        planeMaterial.SetColor("_EmissionColor", warningColor * emissionStrength);
    }

    // 경고 표시 서서히 사라지게 하기
    private IEnumerator FadeOutWarning()
    {
        if (warningPlane == null || planeMaterial == null) yield break;

        isFading = true;

        // 현재 색상 가져오기
        Color startColor = planeMaterial.color;
        Color emissionColor = planeMaterial.GetColor("_EmissionColor");

        // 서서히 사라지게 처리
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            // 투명도 감소 (점점 투명해짐)
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(startColor.a, 0f, t);
            planeMaterial.color = newColor;

            // 발광 강도도 함께 감소
            Color newEmission = Color.Lerp(emissionColor, Color.black, t);
            planeMaterial.SetColor("_EmissionColor", newEmission);

            yield return null;
        }

        // 완전히 투명해지면 제거
        RemoveWarning();
    }

    // 경고 표시 제거
    private void RemoveWarning()
    {
        if (warningPlane != null)
        {
            Destroy(warningPlane);
            warningPlane = null;
        }

        isFading = false;
    }

    void OnDisable()
    {
        // 비활성화될 때 경고 제거
        RemoveWarning();
    }

    void OnDestroy()
    {
        // 스크립트가 파괴될 때 경고 제거 (씬 전환이나 게임 종료 시)
        // 지금 파괴되는건 없지만 예상치 못한 상황(씬 전환, 게임 종료, 디버그 중 오브젝트 삭제 등) 메모리 누수 방지용
        RemoveWarning();
    }
}