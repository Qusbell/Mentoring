using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스태미나 게이지 전용 UI (싱글톤 방식)
/// </summary>
public class StaminaUI : SingletonT<StaminaUI>
{
    private StaminaAction staminaAction;

    [Header("스태미나 게이지")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image staminaFillImage;

    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("색상 설정")]
    [SerializeField] private Color staminaNormalColor = Color.blue;
    [SerializeField] private Color staminaWarningColor = Color.yellow;
    [SerializeField] private Color staminaEmptyColor = Color.red;

    private Coroutine staminaAnimCoroutine;
    private float currentStaminaRatio = 1f;

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
            staminaAction = player.GetComponent<StaminaAction>();
            if (staminaAction != null)
            {
                staminaAction.whenStaminaChanged.AddListener(UpdateStaminaUI);
                SetupFillImage();
                UpdateStaminaUI();
                Debug.Log("StaminaUI: 플레이어 연결 완료");
            }
        }
    }

    /// <summary>
    /// 플레이어 연결 해제
    /// </summary>
    private void DisconnectFromPlayer()
    {
        if (staminaAction != null)
        {
            staminaAction.whenStaminaChanged.RemoveListener(UpdateStaminaUI);
            staminaAction = null;
        }
    }

    /// <summary>
    /// Fill Image 설정
    /// </summary>
    private void SetupFillImage()
    {
        if (staminaSlider == null) return;

        if (staminaFillImage == null)
        {
            staminaFillImage = staminaSlider.fillRect?.GetComponent<Image>();
        }
    }

    /// <summary>
    /// 스태미나 UI 업데이트
    /// </summary>
    private void UpdateStaminaUI()
    {
        if (staminaSlider == null || staminaAction == null) return;

        float targetRatio = (float)staminaAction.stamina / staminaAction.maxStaminaValue;

        if (staminaAnimCoroutine != null)
            StopCoroutine(staminaAnimCoroutine);

        staminaAnimCoroutine = StartCoroutine(AnimateStaminaBar(targetRatio));
    }

    /// <summary>
    /// 스태미나바 애니메이션
    /// </summary>
    private IEnumerator AnimateStaminaBar(float targetRatio)
    {
        float startRatio = currentStaminaRatio;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = animationCurve.Evaluate(elapsedTime / animationDuration);
            currentStaminaRatio = Mathf.Lerp(startRatio, targetRatio, progress);

            staminaSlider.value = currentStaminaRatio;
            UpdateStaminaColor(currentStaminaRatio);

            yield return null;
        }

        currentStaminaRatio = targetRatio;
        staminaSlider.value = currentStaminaRatio;
        UpdateStaminaColor(currentStaminaRatio);
    }

    /// <summary>
    /// 색상 업데이트
    /// </summary>
    private void UpdateStaminaColor(float staminaRatio)
    {
        if (staminaFillImage == null) return;

        Color newColor;
        if (staminaRatio <= 0.1f)
            newColor = staminaEmptyColor;
        else if (staminaRatio <= 0.3f)
            newColor = staminaWarningColor;
        else
            newColor = staminaNormalColor;

        staminaFillImage.color = newColor;
    }

    /// <summary>
    /// 애니메이션 중지
    /// </summary>
    private void StopAnimations()
    {
        if (staminaAnimCoroutine != null)
        {
            StopCoroutine(staminaAnimCoroutine);
            staminaAnimCoroutine = null;
        }
    }

    /// <summary>
    /// 외부 호출용 메서드들
    /// </summary>
    public void ConnectToNewPlayer() => ConnectToPlayer();
    public void ManualUpdateStamina() => UpdateStaminaUI();

    [ContextMenu("테스트 - 스태미나 사용")]
    public void TestUseStamina()
    {
        staminaAction?.UseStamina(1);
    }

    [ContextMenu("테스트 - 스태미나 회복")]
    public void TestRecoverStamina()
    {
        staminaAction?.RecoverStamina(1);
    }
}