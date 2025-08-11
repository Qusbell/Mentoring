using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 체력 게이지 전용 UI (싱글톤 방식)
/// </summary>
public class HealthUI : SingletonT<HealthUI>
{
    private DamageReaction damageReaction;

    [Header("HP 게이지")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("색상 설정")]
    [SerializeField] private Color healthNormalColor = Color.green;
    [SerializeField] private Color healthWarningColor = Color.yellow;
    [SerializeField] private Color healthDangerColor = Color.red;

    private Coroutine healthAnimCoroutine;
    private float currentHealthRatio = 1f;

    private void Start()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        //DontDestroyOnLoad(gameObject);
        ConnectToPlayer();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        DisconnectFromPlayer();
        StopAnimations();
    }

    protected override void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        DisconnectFromPlayer();
        StopAnimations();
        base.OnDestroy();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ConnectToPlayer();
    }

    /// <summary>
    /// 플레이어 연결
    /// </summary>
    private void ConnectToPlayer()
    {
        DisconnectFromPlayer(); // 기존 연결 해제

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            damageReaction = player.GetComponent<DamageReaction>();
            if (damageReaction != null)
            {
                damageReaction.whenHealthChange.AddListener(UpdateHealthUI);
                SetupFillImage();
                UpdateHealthUI();
                Debug.Log("HealthUI: 플레이어 연결 완료");
            }
        }
    }

    /// <summary>
    /// 플레이어 연결 해제
    /// </summary>
    private void DisconnectFromPlayer()
    {
        if (damageReaction != null)
        {
            damageReaction.whenHealthChange.RemoveListener(UpdateHealthUI);
            damageReaction = null;
        }
    }


    /// <summary> /// 
    /// 체력 텍스트 업데이트 /// 
    /// </summary>
    private void UpdateHealthText()
    {
        if (healthText != null && damageReaction != null)
        {
            healthText.text = $"{damageReaction.healthPoint}/{damageReaction.maxHealthPoint}";
        }
    }

    /// <summary>
    /// Fill Image 설정
    /// </summary>
    private void SetupFillImage()
    {
        if (healthSlider == null) return;

        if (healthFillImage == null)
        {
            healthFillImage = healthSlider.fillRect?.GetComponent<Image>();
        }
    }

    /// <summary>
    /// 체력 UI 업데이트
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthSlider == null || damageReaction == null) return;

        float targetRatio = (float)damageReaction.healthPoint / damageReaction.maxHealthPoint;

        if (healthAnimCoroutine != null)
            StopCoroutine(healthAnimCoroutine);

        healthAnimCoroutine = StartCoroutine(AnimateHealthBar(targetRatio));
    }

    /// <summary>
    /// 체력바 애니메이션
    /// </summary>
    private IEnumerator AnimateHealthBar(float targetRatio)
    {
        float startRatio = currentHealthRatio;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = animationCurve.Evaluate(elapsedTime / animationDuration);
            currentHealthRatio = Mathf.Lerp(startRatio, targetRatio, progress);

            healthSlider.value = currentHealthRatio;
            UpdateHealthColor(currentHealthRatio);
            UpdateHealthText();

            yield return null;
        }

        currentHealthRatio = targetRatio;
        healthSlider.value = currentHealthRatio;
        UpdateHealthColor(currentHealthRatio);
        UpdateHealthText();
    }

    /// <summary>
    /// 색상 업데이트
    /// </summary>
    private void UpdateHealthColor(float healthRatio)
    {
        if (healthFillImage == null) return;

        Color newColor;
        if (healthRatio <= 0.2f)
            newColor = healthDangerColor;
        else if (healthRatio <= 0.5f)
            newColor = healthWarningColor;
        else
            newColor = healthNormalColor;

        healthFillImage.color = newColor;
    }

    /// <summary>
    /// 애니메이션 중지
    /// </summary>
    private void StopAnimations()
    {
        if (healthAnimCoroutine != null)
        {
            StopCoroutine(healthAnimCoroutine);
            healthAnimCoroutine = null;
        }
    }

    /// <summary>
    /// 외부 호출용 메서드들
    /// </summary>
    public void ConnectToNewPlayer() => ConnectToPlayer();
    public void ManualUpdateHealth() => UpdateHealthUI();

    [ContextMenu("테스트 - HP 감소")]
    public void TestDecreaseHealth()
    {
        damageReaction?.TakeDamage(1, null, 0f, 0f);
    }

    [ContextMenu("테스트 - HP 회복")]
    public void TestHealHealth()
    {
        damageReaction?.Heal(1);
    }
}