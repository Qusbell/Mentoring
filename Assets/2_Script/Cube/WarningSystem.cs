using UnityEngine;
using System.Collections;


/// 위에서 아래로 떨어지는 큐브가 그 아래에 있는 큐브의 윗면 중앙에 빨간색 경고 표시를 보여주는 스크립트
/// 큐브가 가까워질수록 경고 표시가 더 선명해지고, 착지 직전에 부드럽게 사라짐.

public class WarningSystem : MonoBehaviour
{
    [Header("기본 설정")]
    [Tooltip("아래로 확인할 최대 거리")]
    public float rayDistance = 10f;

    [Tooltip("감지할 큐브 레이어")]
    public LayerMask detectionLayer = 1;

    [Header("경고 표시 효과 설정")]
    [Tooltip("경고 색상 변화 시작 거리 비율 (0.5 = 절반 거리에서 변화 시작)")]
    [Range(0.2f, 0.8f)]
    public float colorChangeStartRatio = 0.5f;

    [Tooltip("경고 표시 사라짐 시작 거리 (착지 전 이 거리에서 페이드 시작)")]
    public float fadeStartDistance = 0.5f;

    [Tooltip("경고 표시 사라짐 시간 (초)")]
    public float fadeDuration = 0.3f;

    // 내부 변수
    private GameObject warningPlane;      // 경고 표시 오브젝트
    private Vector3 initialPosition;      // 시작 위치
    private Vector3 targetPosition;       // 착지 위치
    private float totalDistance;          // 총 이동 거리
    private bool isFading = false;        // 페이드 중 여부
    private Material planeMaterial;       // 경고 표시 재질

    // 색상 설정
    private readonly Color warningColor = Color.red;  // 경고 색상
    private const float startAlpha = 0.3f;            // 초기 투명도
    private const float maxAlpha = 0.8f;              // 최대 투명도
    private const float emissionIntensity = 1f;       // 발광 강도


    /// 시작 시 초기화 및 첫 검사 수행
    void Start()
    {
        // 초기 위치 저장
        initialPosition = transform.position;

        // 디버그 로그 추가
        Debug.Log($"[{gameObject.name}] 시작 위치: {transform.position}, 레이어 마스크: {detectionLayer.value}");

        // 아래 큐브 확인
        CheckForCubeBelow();
    }


    /// 활성화될 때 검사 수행
    void OnEnable()
    {
        // 초기 위치일 때만 검사
        if (transform.position == initialPosition)
        {
            CheckForCubeBelow();
        }
    }

    /// 아래에 있는 큐브 확인 및 경고 표시 생성
    private void CheckForCubeBelow()
    {
        RaycastHit hit;

        // 레이캐스트로 아래 큐브 감지 - 디버그 레이 추가
        Debug.DrawRay(transform.position, Vector3.down * rayDistance, Color.yellow, 5f);

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, detectionLayer))
        {
            // 디버그 로그 추가
            Debug.Log($"레이캐스트 감지: {hit.collider.gameObject.name}, 거리: {hit.distance}");

            // 감지된 오브젝트에서 렌더러 컴포넌트 확인
            Renderer targetRenderer = hit.collider.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                Debug.Log($"대상 렌더러 크기: {targetRenderer.bounds.size}, 중앙: {targetRenderer.bounds.center}");

                // 중요: 히트 포인트를 바로 사용 - 이것이 충돌 지점의 정확한 위치
                Vector3 warningPosition = hit.point;

                // 큐브 윗면 중앙에 경고 표시 생성 - 히트 포인트 직접 사용
                CreateWarningPlane(warningPosition, targetRenderer);

                // 이 큐브의 착지 위치 계산
                float landingY = warningPosition.y + GetComponent<Renderer>().bounds.extents.y;
                targetPosition = new Vector3(
                    transform.position.x,
                    landingY,
                    transform.position.z
                );

                // 총 이동 거리 계산
                totalDistance = Vector3.Distance(initialPosition, targetPosition);
                Debug.Log($"착지 위치: {targetPosition}, 총 이동 거리: {totalDistance}");
            }
            else
            {
                Debug.LogWarning($"감지된 오브젝트 {hit.collider.gameObject.name}에 Renderer 컴포넌트가 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("아래쪽으로 큐브를 감지하지 못했습니다. 레이어 설정을 확인하세요.");
        }
    }


    /// 큐브 윗면 중앙에 경고 표시 생성
    /// 
    /// <param name="position">경고 표시 생성 위치 (히트 포인트)</param>
    /// <param name="targetRenderer">아래 큐브의 렌더러 컴포넌트</param>
    private void CreateWarningPlane(Vector3 position, Renderer targetRenderer)
    {
        // 이전 경고 표시 제거
        if (warningPlane != null)
        {
            Destroy(warningPlane);
        }

        // 디버그 로그 추가
        Debug.Log($"경고 표시 생성 위치: {position}");

        // 평면 생성
        warningPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        warningPlane.name = "Warning_Plane";

        // 위치 및 회전 설정 (히트 포인트 위치 그대로 사용)
        warningPlane.transform.position = position;
        warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0); // X축 기준 90도 회전 (바닥과 평행)

        // 아래 큐브 크기에 맞게 크기 설정
        float planeWidth = targetRenderer.bounds.size.x * 0.9f; // 약간 작게 만들어 경계를 벗어나지 않도록
        float planeLength = targetRenderer.bounds.size.z * 0.9f;
        warningPlane.transform.localScale = new Vector3(planeWidth, planeLength, 1f);

        Debug.Log($"경고 표시 크기: {planeWidth} x {planeLength}");

        // 충돌체 비활성화 (충돌 처리 방지)
        warningPlane.GetComponent<Collider>().enabled = false;

        // Ignore Raycast 레이어로 설정
        warningPlane.layer = LayerMask.NameToLayer("Ignore Raycast");

        // 머티리얼 생성 및 설정
        Renderer planeRenderer = warningPlane.GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            planeMaterial = new Material(Shader.Find("Standard"));

            // 반투명 설정
            planeMaterial.SetFloat("_Mode", 3);
            planeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            planeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            planeMaterial.SetInt("_ZWrite", 0);
            planeMaterial.DisableKeyword("_ALPHATEST_ON");
            planeMaterial.EnableKeyword("_ALPHABLEND_ON");
            planeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            planeMaterial.renderQueue = 3000;

            // 초기 색상 및 투명도 설정
            Color color = warningColor;
            color.a = startAlpha;
            planeMaterial.color = color;

            // 발광 효과 추가
            planeMaterial.EnableKeyword("_EMISSION");
            planeMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);

            planeRenderer.material = planeMaterial;
        }
    }


    /// 매 프레임 업데이트 - 거리에 따른 경고 표시 효과 조절
    void Update()
    {
        // 경고 표시가 없으면 아무것도 하지 않음
        if (warningPlane == null || planeMaterial == null) return;

        // 현재 착지 위치까지 거리 계산
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // 움직임 감지 (초기 위치에서 약간이라도 움직였는지)
        bool isMoving = Vector3.Distance(transform.position, initialPosition) > 0.05f;

        if (isMoving && !isFading)
        {
            // 착지 직전이면 사라지기 시작
            if (distanceToTarget <= fadeStartDistance)
            {
                StartCoroutine(FadeOutWarning());
            }
            else
            {
                // 거리에 따라 경고 표시 색상 업데이트
                UpdateWarningIntensity(distanceToTarget);
            }
        }
    }


    /// 거리에 따라 경고 표시 선명도 업데이트
    /// <param name="currentDistance">현재 착지 위치까지 거리</param>
    private void UpdateWarningIntensity(float currentDistance)
    {
        // 거리 비율 계산 (1 = 먼 거리, 0 = 가까운 거리)
        float distanceRatio = Mathf.Clamp01(currentDistance / totalDistance);

        // 색상 변화 시작점 이전이면 초기 색상 유지
        if (distanceRatio > colorChangeStartRatio)
        {
            Color color = warningColor;
            color.a = startAlpha;
            planeMaterial.color = color;
            planeMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);
            return;
        }

        // 변화 진행 비율 계산 (거리가 가까워질수록 값이 커짐)
        float changeProgress = 1f - (distanceRatio / colorChangeStartRatio);

        // 투명도 계산 (점점 더 불투명해짐)
        float alpha = Mathf.Lerp(startAlpha, maxAlpha, changeProgress);

        // 색상 및 발광 효과
        Color newColor = warningColor;
        newColor.a = alpha;
        planeMaterial.color = newColor;
        planeMaterial.SetColor("_EmissionColor", warningColor * alpha * emissionIntensity);
    }

    /// 경고 표시를 서서히 사라지게 하는 코루틴
    private IEnumerator FadeOutWarning()
    {
        if (warningPlane == null || planeMaterial == null) yield break;

        isFading = true;

        // 현재 색상 저장
        Color startColor = planeMaterial.color;
        Color emissionColor = planeMaterial.GetColor("_EmissionColor");

        // 페이드 아웃 효과
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            // 점점 투명해지는 효과
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(startColor.a, 0f, t);
            planeMaterial.color = newColor;

            // 발광 효과도 함께 감소
            Color newEmission = Color.Lerp(emissionColor, Color.black, t);
            planeMaterial.SetColor("_EmissionColor", newEmission);

            yield return null;
        }

        // 완전히 투명해지면 제거
        RemoveWarning();
    }

    /// 경고 표시 제거
    private void RemoveWarning()
    {
        if (warningPlane != null)
        {
            Destroy(warningPlane);
            warningPlane = null;
        }

        isFading = false;
    }

    /// 비활성화될 때 경고 표시 제거
    void OnDisable()
    {
        RemoveWarning();
    }

    /// 파괴될 때 경고 표시 제거
    void OnDestroy()
    {
        RemoveWarning();
    }

    /// 씬 뷰에서 레이캐스트 경로 시각화 (디버깅용) 게임에서는 안보여
    void OnDrawGizmos()
    {
        // 레이캐스트 경로 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * rayDistance);

        // 감지된 큐브가 있으면 위치 표시
        if (warningPlane != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(warningPlane.transform.position, 0.1f);
        }
    }
}