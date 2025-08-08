using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 체력 게이지 전용 UI (Slider 버전 - 색상 문제 수정)
/// </summary>
public class HealthUI : MonoBehaviour
{
    private DamageReaction damageReaction;

    [Header("HP 게이지 (Slider 사용)")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFillImage; // Slider의 Fill 이미지

    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("색상 설정")]
    [SerializeField] private Color healthNormalColor = Color.green;
    [SerializeField] private Color healthWarningColor = Color.yellow;
    [SerializeField] private Color healthDangerColor = Color.red;

    [Header("펄스 효과")]
    [SerializeField] private bool enablePulseEffect = true;
    [SerializeField] private float pulseSpeed = 2f;

    // 애니메이션 관리
    private Coroutine healthAnimCoroutine;
    private Coroutine healthPulseCoroutine;

    // 현재 값
    private float currentHealthRatio = 1f;

    private void Start()
    {
        // 플레이어 찾기 및 이벤트 구독
        SetupPlayerConnection();

        // Fill Image 제대로 찾기
        SetupFillImage();

        // 초기 UI 업데이트
        UpdateHealthUI();
    }

    /// <summary>
    /// 플레이어 연결 및 이벤트 구독
    /// </summary>
    private void SetupPlayerConnection()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            damageReaction = player.GetComponent<DamageReaction>();

            if (damageReaction != null)
            {
                // 체력 변경 이벤트 구독 - 모든 체력 변화를 감지 (오타 수정)
                damageReaction.whenHealthChange.AddListener(UpdateHealthUI);
                Debug.Log("HealthUI: 체력 변경 이벤트 구독 완료 - 모든 체력 변화 감지됨");
            }
            else
            {
                Debug.LogWarning("HealthUI: DamageReaction 컴포넌트를 찾을 수 없습니다");
            }
        }
        else
        {
            Debug.LogWarning("HealthUI: Player 태그 오브젝트를 찾을 수 없습니다");
        }
    }

    /// <summary>
    /// Fill Image 제대로 찾기 (기존 방식에 문제가 있었음)
    /// </summary>
    private void SetupFillImage()
    {
        if (healthSlider == null)
        {
            Debug.LogError("HealthUI: healthSlider가 할당되지 않았습니다");
            return;
        }

        // Inspector에서 직접 할당하지 않았다면 자동으로 찾기
        if (healthFillImage == null)
        {
            // 방법 1: fillRect에서 찾기
            if (healthSlider.fillRect != null)
            {
                healthFillImage = healthSlider.fillRect.GetComponent<Image>();
                Debug.Log($"HealthUI: fillRect에서 Fill Image 찾기: {healthFillImage != null}");
            }

            // 방법 2: 자식에서 "Fill" 이름으로 찾기
            if (healthFillImage == null)
            {
                Transform fillTransform = healthSlider.transform.Find("Fill Area/Fill");
                if (fillTransform != null)
                {
                    healthFillImage = fillTransform.GetComponent<Image>();
                    Debug.Log($"HealthUI: 이름으로 Fill Image 찾기: {healthFillImage != null}");
                }
            }

            // 방법 3: 모든 자식 Image 중에서 찾기 (마지막 수단)
            if (healthFillImage == null)
            {
                Image[] allImages = healthSlider.GetComponentsInChildren<Image>();
                foreach (Image img in allImages)
                {
                    if (img.name.ToLower().Contains("fill"))
                    {
                        healthFillImage = img;
                        Debug.Log($"HealthUI: 자식 검색으로 Fill Image 찾기: {img.name}");
                        break;
                    }
                }
            }
        }

        if (healthFillImage == null)
        {
            Debug.LogError("HealthUI: Fill Image를 찾을 수 없습니다. Inspector에서 직접 할당해주세요.");
        }
        else
        {
            Debug.Log($"HealthUI: Fill Image 설정 완료: {healthFillImage.name}");
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제 (오타 수정)
        if (damageReaction != null)
        {
            damageReaction.whenHealthChange.RemoveListener(UpdateHealthUI);
        }

        StopAllAnimations();
    }

    /// <summary>
    /// HP UI 애니메이션 업데이트
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthSlider == null || damageReaction == null)
        {
            Debug.LogWarning("HealthUI: healthSlider 또는 damageReaction이 null입니다");
            return;
        }

        float targetRatio = (float)damageReaction.healthPoint / damageReaction.maxHealthPoint;
        Debug.Log($"HealthUI: HP 업데이트 - {damageReaction.healthPoint}/{damageReaction.maxHealthPoint} = {targetRatio:F2}");

        // 체력 바 애니메이션
        if (healthAnimCoroutine != null)
            StopCoroutine(healthAnimCoroutine);

        healthAnimCoroutine = StartCoroutine(AnimateHealthBar(targetRatio));

        // 위험 상태일 때 펄스 효과
        HandleHealthPulseEffect(targetRatio);
    }

    /// <summary>
    /// 체력 바 애니메이션 (색상 업데이트 수정)
    /// </summary>
    private IEnumerator AnimateHealthBar(float targetRatio)
    {
        float startRatio = currentHealthRatio;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            float curveValue = animationCurve.Evaluate(progress);
            currentHealthRatio = Mathf.Lerp(startRatio, targetRatio, curveValue);

            // Slider 값 업데이트
            healthSlider.value = currentHealthRatio;

            // 색상 업데이트 (매번 확인하고 적용)
            UpdateHealthColor(currentHealthRatio);

            yield return null;
        }

        // 최종 값 설정
        currentHealthRatio = targetRatio;
        healthSlider.value = currentHealthRatio;
        UpdateHealthColor(currentHealthRatio);
    }

    /// <summary>
    /// 색상 업데이트를 별도 메서드로 분리
    /// </summary>
    private void UpdateHealthColor(float healthRatio)
    {
        if (healthFillImage != null)
        {
            Color newColor = GetHealthColor(healthRatio);
            healthFillImage.color = newColor;

            // 디버깅용 로그 (필요시 주석 해제)
            // Debug.Log($"HealthUI: 색상 변경 - HP:{healthRatio:F2} → {newColor}");
        }
        else
        {
            Debug.LogWarning("HealthUI: healthFillImage가 null이어서 색상을 변경할 수 없습니다");
        }
    }

    /// <summary>
    /// 체력 위험시 펄스 효과 관리
    /// </summary>
    private void HandleHealthPulseEffect(float healthRatio)
    {
        if (!enablePulseEffect) return;

        if (healthRatio <= 0.2f)
        {
            if (healthPulseCoroutine == null)
                healthPulseCoroutine = StartCoroutine(PulseHealthBar());
        }
        else
        {
            if (healthPulseCoroutine != null)
            {
                StopCoroutine(healthPulseCoroutine);
                healthPulseCoroutine = null;

                // 스케일 초기화
                if (healthSlider != null)
                    healthSlider.transform.localScale = Vector3.one;
            }
        }
    }

    /// <summary>
    /// 체력 바 펄스 효과
    /// </summary>
    private IEnumerator PulseHealthBar()
    {
        while (true)
        {
            float pulseValue = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float scale = Mathf.Lerp(1f, 1.1f, pulseValue);

            if (healthSlider != null)
            {
                healthSlider.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 체력 비율에 따른 색상 반환 (로그 추가)
    /// </summary>
    private Color GetHealthColor(float healthRatio)
    {
        Color resultColor;

        if (healthRatio <= 0.2f)
        {
            resultColor = healthDangerColor;
        }
        else if (healthRatio <= 0.5f)
        {
            resultColor = healthWarningColor;
        }
        else
        {
            resultColor = healthNormalColor;
        }

        return resultColor;
    }

    /// <summary>
    /// 모든 애니메이션 중지
    /// </summary>
    private void StopAllAnimations()
    {
        if (healthAnimCoroutine != null)
        {
            StopCoroutine(healthAnimCoroutine);
            healthAnimCoroutine = null;
        }

        if (healthPulseCoroutine != null)
        {
            StopCoroutine(healthPulseCoroutine);
            healthPulseCoroutine = null;
        }
    }

    /// <summary>
    /// 수동 체력 업데이트 (외부 호출용)
    /// </summary>
    public void ManualUpdateHealth()
    {
        UpdateHealthUI();
    }

    /// <summary>
    /// 테스트 메서드
    /// </summary>
    [ContextMenu("테스트 - HP 감소")]
    public void TestDecreaseHealth()
    {
        if (damageReaction != null)
        {
            damageReaction.TakeDamage(1, null, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("HealthUI: damageReaction이 null이어서 테스트할 수 없습니다");
        }
    }

    /// <summary>
    /// 힐링 테스트 메서드 (새로 추가)
    /// </summary>
    [ContextMenu("테스트 - HP 회복")]
    public void TestHealHealth()
    {
        if (damageReaction != null)
        {
            damageReaction.Heal(1);
        }
        else
        {
            Debug.LogWarning("HealthUI: damageReaction이 null이어서 테스트할 수 없습니다");
        }
    }
}