using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스태미나 게이지 전용 UI (Slider 버전 - 펄스 효과만 제거)
/// </summary>
public class StaminaUI : MonoBehaviour
{
    private StaminaAction staminaAction;

    [Header("스태미나 게이지 (Slider 사용)")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image staminaFillImage; // Slider의 Fill 이미지

    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("색상 설정")]
    [SerializeField] private Color staminaNormalColor = Color.blue;
    [SerializeField] private Color staminaWarningColor = Color.yellow;
    [SerializeField] private Color staminaEmptyColor = Color.red;

    // 애니메이션 관리
    private Coroutine staminaAnimCoroutine;

    // 현재 값
    private float currentStaminaRatio = 1f;

    private void Start()
    {
        // 플레이어 찾기 및 이벤트 구독
        SetupPlayerConnection();

        // Fill Image 제대로 찾기
        SetupFillImage();

        // 초기 UI 업데이트
        UpdateStaminaUI();
    }

    /// <summary>
    /// 플레이어 연결 및 이벤트 구독
    /// </summary>
    private void SetupPlayerConnection()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            staminaAction = player.GetComponent<StaminaAction>();

            if (staminaAction != null)
            {
                // 스태미나 변경 이벤트 구독
                staminaAction.whenStaminaChanged.AddListener(UpdateStaminaUI);
                Debug.Log("StaminaUI: 스태미나 변경 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("StaminaUI: StaminaAction 컴포넌트를 찾을 수 없습니다");
            }
        }
        else
        {
            Debug.LogWarning("StaminaUI: Player 태그 오브젝트를 찾을 수 없습니다");
        }
    }

    /// <summary>
    /// Fill Image 제대로 찾기
    /// </summary>
    private void SetupFillImage()
    {
        if (staminaSlider == null)
        {
            Debug.LogError("StaminaUI: staminaSlider가 할당되지 않았습니다");
            return;
        }

        // Inspector에서 직접 할당하지 않았다면 자동으로 찾기
        if (staminaFillImage == null)
        {
            // 방법 1: fillRect에서 찾기
            if (staminaSlider.fillRect != null)
            {
                staminaFillImage = staminaSlider.fillRect.GetComponent<Image>();
                Debug.Log($"StaminaUI: fillRect에서 Fill Image 찾기: {staminaFillImage != null}");
            }

            // 방법 2: 자식에서 "Fill" 이름으로 찾기
            if (staminaFillImage == null)
            {
                Transform fillTransform = staminaSlider.transform.Find("Fill Area/Fill");
                if (fillTransform != null)
                {
                    staminaFillImage = fillTransform.GetComponent<Image>();
                    Debug.Log($"StaminaUI: 이름으로 Fill Image 찾기: {staminaFillImage != null}");
                }
            }

            // 방법 3: 모든 자식 Image 중에서 찾기 (마지막 수단)
            if (staminaFillImage == null)
            {
                Image[] allImages = staminaSlider.GetComponentsInChildren<Image>();
                foreach (Image img in allImages)
                {
                    if (img.name.ToLower().Contains("fill"))
                    {
                        staminaFillImage = img;
                        Debug.Log($"StaminaUI: 자식 검색으로 Fill Image 찾기: {img.name}");
                        break;
                    }
                }
            }
        }

        if (staminaFillImage == null)
        {
            Debug.LogError("StaminaUI: Fill Image를 찾을 수 없습니다. Inspector에서 직접 할당해주세요.");
        }
        else
        {
            Debug.Log($"StaminaUI: Fill Image 설정 완료: {staminaFillImage.name}");
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (staminaAction != null)
        {
            staminaAction.whenStaminaChanged.RemoveListener(UpdateStaminaUI);
        }

        StopAllAnimations();
    }

    /// <summary>
    /// 스태미나 UI 애니메이션 업데이트
    /// </summary>
    private void UpdateStaminaUI()
    {
        if (staminaSlider == null || staminaAction == null)
        {
            Debug.LogWarning("StaminaUI: staminaSlider 또는 staminaAction이 null입니다");
            return;
        }

        float targetRatio = (float)staminaAction.stamina / staminaAction.maxStaminaValue;
        Debug.Log($"StaminaUI: 스태미나 업데이트 - {staminaAction.stamina}/{staminaAction.maxStaminaValue} = {targetRatio:F2}");

        // 스태미나 바 애니메이션
        if (staminaAnimCoroutine != null)
            StopCoroutine(staminaAnimCoroutine);

        staminaAnimCoroutine = StartCoroutine(AnimateStaminaBar(targetRatio));
    }

    /// <summary>
    /// 스태미나 바 애니메이션
    /// </summary>
    private IEnumerator AnimateStaminaBar(float targetRatio)
    {
        float startRatio = currentStaminaRatio;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;

            float curveValue = animationCurve.Evaluate(progress);
            currentStaminaRatio = Mathf.Lerp(startRatio, targetRatio, curveValue);

            // Slider 값 업데이트
            staminaSlider.value = currentStaminaRatio;

            // 색상 업데이트
            UpdateStaminaColor(currentStaminaRatio);

            yield return null;
        }

        // 최종 값 설정
        currentStaminaRatio = targetRatio;
        staminaSlider.value = currentStaminaRatio;
        UpdateStaminaColor(currentStaminaRatio);
    }

    /// <summary>
    /// 색상 업데이트를 별도 메서드로 분리
    /// </summary>
    private void UpdateStaminaColor(float staminaRatio)
    {
        if (staminaFillImage != null)
        {
            Color newColor = GetStaminaColor(staminaRatio);
            staminaFillImage.color = newColor;
        }
        else
        {
            Debug.LogWarning("StaminaUI: staminaFillImage가 null이어서 색상을 변경할 수 없습니다");
        }
    }

    /// <summary>
    /// 스태미나 비율에 따른 색상 반환
    /// </summary>
    private Color GetStaminaColor(float staminaRatio)
    {
        Color resultColor;

        if (staminaRatio <= 0.1f) // 스태미나가 거의 없을 때
        {
            resultColor = staminaEmptyColor;
        }
        else if (staminaRatio <= 0.3f) // 스태미나가 적을 때
        {
            resultColor = staminaWarningColor;
        }
        else // 스태미나가 충분할 때
        {
            resultColor = staminaNormalColor;
        }

        return resultColor;
    }

    /// <summary>
    /// 모든 애니메이션 중지
    /// </summary>
    private void StopAllAnimations()
    {
        if (staminaAnimCoroutine != null)
        {
            StopCoroutine(staminaAnimCoroutine);
            staminaAnimCoroutine = null;
        }
    }

    /// <summary>
    /// 수동 스태미나 업데이트 (외부 호출용)
    /// </summary>
    public void ManualUpdateStamina()
    {
        UpdateStaminaUI();
    }

    /// <summary>
    /// 테스트 메서드들
    /// </summary>
    [ContextMenu("테스트 - 스태미나 사용")]
    public void TestUseStamina()
    {
        if (staminaAction != null)
        {
            staminaAction.UseStamina(1);
        }
        else
        {
            Debug.LogWarning("StaminaUI: staminaAction이 null이어서 테스트할 수 없습니다");
        }
    }

    [ContextMenu("테스트 - 스태미나 회복")]
    public void TestRecoverStamina()
    {
        if (staminaAction != null)
        {
            staminaAction.RecoverStamina(1);
        }
        else
        {
            Debug.LogWarning("StaminaUI: staminaAction이 null이어서 테스트할 수 없습니다");
        }
    }
}