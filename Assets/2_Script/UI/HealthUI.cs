using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 체력 게이지 전용 UI (Slider 버전 - Unity 2022 호환)
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
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            damageReaction = player.GetComponent<DamageReaction>();
        }

        // Slider의 Fill 이미지 자동 찾기
        if (healthSlider != null && healthFillImage == null)
        {
            healthFillImage = healthSlider.fillRect?.GetComponent<Image>();
        }

        // 초기 UI 업데이트
        UpdateHealthUI();
    }

    private void OnEnable()
    {
        // 이벤트 구독
        if (damageReaction != null)
        {
            damageReaction.whenHit.AddListener(UpdateHealthUI);
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (damageReaction != null)
        {
            damageReaction.whenHit.RemoveListener(UpdateHealthUI);
        }

        StopAllAnimations();
    }

    /// <summary>
    /// HP UI 애니메이션 업데이트
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthSlider == null || damageReaction == null) return;

        float targetRatio = (float)damageReaction.healthPoint / damageReaction.maxHealthPoint;

        // 체력 바 애니메이션
        if (healthAnimCoroutine != null)
            StopCoroutine(healthAnimCoroutine);

        healthAnimCoroutine = StartCoroutine(AnimateHealthBar(targetRatio));

        // 위험 상태일 때 펄스 효과
        HandleHealthPulseEffect(targetRatio);
    }

    /// <summary>
    /// 체력 바 애니메이션 (Slider 버전)
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

            // 색상 업데이트
            if (healthFillImage != null)
            {
                healthFillImage.color = GetHealthColor(currentHealthRatio);
            }

            yield return null;
        }

        currentHealthRatio = targetRatio;
        healthSlider.value = currentHealthRatio;

        if (healthFillImage != null)
        {
            healthFillImage.color = GetHealthColor(currentHealthRatio);
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
    /// 체력 바 펄스 효과 (Slider 버전)
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
    /// 체력 비율에 따른 색상 반환
    /// </summary>
    private Color GetHealthColor(float healthRatio)
    {
        if (healthRatio <= 0.2f)
            return healthDangerColor;
        else if (healthRatio <= 0.5f)
            return healthWarningColor;
        else
            return healthNormalColor;
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
            damageReaction.TakeDamage(10, null, 0f, 0f);
        }
    }
}