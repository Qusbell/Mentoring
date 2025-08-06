using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스태미나 칸 전용 UI (애니메이션 포함)
/// </summary>
public class StaminaUI : MonoBehaviour
{
    private StaminaAction staminaAction;

    [Header("스태미나 칸 UI")]
    [SerializeField] private Transform staminaContainer;     // 스태미나 칸들의 부모
    [SerializeField] private GameObject staminaSlotPrefab;   // 스태미나 칸 프리팹
    [SerializeField] private Sprite staminaFullSprite;       // 가득찬 스태미나 칸
    [SerializeField] private Sprite staminaEmptySprite;      // 빈 스태미나 칸

    [Header("애니메이션 설정")]
    [SerializeField] private float staminaAnimationDuration = 0.2f;
    [SerializeField] private float staminaScaleEffect = 1.3f; // 칸 변화시 스케일 효과
    [SerializeField] private AnimationCurve scaleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("색상 효과")]
    [SerializeField] private bool enableColorFlash = true;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    // 스태미나 칸 이미지들
    private List<Image> staminaSlots = new List<Image>();

    // 현재 값
    private int lastStaminaCount = 0;

    private void Start()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            staminaAction = player.GetComponent<StaminaAction>();
        }

        // 스태미나 칸 생성
        CreateStaminaSlots();

        // 초기 UI 업데이트
        UpdateStaminaUI();
    }

    private void OnEnable()
    {
        // 이벤트 구독
        if (staminaAction != null)
        {
            staminaAction.whenStaminaChanged.AddListener(UpdateStaminaUI);
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (staminaAction != null)
        {
            staminaAction.whenStaminaChanged.RemoveListener(UpdateStaminaUI);
        }

        StopAllCoroutines();
    }

    /// <summary>
    /// 스태미나 칸들 생성
    /// </summary>
    private void CreateStaminaSlots()
    {
        if (staminaAction == null || staminaContainer == null || staminaSlotPrefab == null) return;

        // 기존 칸들 제거
        foreach (Transform child in staminaContainer)
        {
            Destroy(child.gameObject);
        }
        staminaSlots.Clear();

        // 최대 스태미나 수만큼 칸 생성
        for (int i = 0; i < staminaAction.maxStaminaValue; i++)
        {
            GameObject slot = Instantiate(staminaSlotPrefab, staminaContainer);
            Image slotImage = slot.GetComponent<Image>();

            if (slotImage != null)
            {
                staminaSlots.Add(slotImage);
                slotImage.sprite = staminaFullSprite; // 초기에는 모두 채움

                // 슬롯 이름 설정 (디버그용)
                slot.name = $"StaminaSlot_{i}";
            }
        }

        lastStaminaCount = staminaAction.maxStaminaValue;
    }

    /// <summary>
    /// 스태미나 칸 애니메이션 업데이트
    /// </summary>
    private void UpdateStaminaUI()
    {
        if (staminaAction == null) return;

        int currentStamina = staminaAction.stamina;

        // 변화된 칸만 애니메이션
        for (int i = 0; i < staminaSlots.Count; i++)
        {
            bool shouldBeFilled = i < currentStamina;
            bool wasFilled = i < lastStaminaCount;

            // 상태가 변한 칸만 애니메이션
            if (shouldBeFilled != wasFilled)
            {
                StartCoroutine(AnimateStaminaSlot(i, shouldBeFilled));
            }
        }

        lastStaminaCount = currentStamina;
    }

    /// <summary>
    /// 개별 스태미나 칸 애니메이션
    /// </summary>
    private IEnumerator AnimateStaminaSlot(int slotIndex, bool fillSlot)
    {
        if (slotIndex >= staminaSlots.Count) yield break;

        Image slotImage = staminaSlots[slotIndex];
        if (slotImage == null) yield break;

        Vector3 originalScale = Vector3.one;
        Color originalColor = slotImage.color;

        // 1단계: 컬러 플래시 (선택사항)
        if (enableColorFlash)
        {
            slotImage.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            slotImage.color = originalColor;
        }

        // 2단계: 스케일 업 애니메이션
        float elapsedTime = 0f;
        float halfDuration = staminaAnimationDuration * 0.5f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / halfDuration;

            float curveValue = scaleAnimationCurve.Evaluate(progress);
            float scale = Mathf.Lerp(1f, staminaScaleEffect, curveValue);
            slotImage.transform.localScale = originalScale * scale;

            yield return null;
        }

        // 3단계: 스프라이트 변경
        slotImage.sprite = fillSlot ? staminaFullSprite : staminaEmptySprite;

        // 4단계: 스케일 다운 애니메이션
        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / halfDuration;

            float curveValue = scaleAnimationCurve.Evaluate(progress);
            float scale = Mathf.Lerp(staminaScaleEffect, 1f, curveValue);
            slotImage.transform.localScale = originalScale * scale;

            yield return null;
        }

        // 최종 상태
        slotImage.transform.localScale = originalScale;
    }

    /// <summary>
    /// 스태미나 최대값 변경시 칸 재생성
    /// </summary>
    public void RefreshStaminaSlots()
    {
        CreateStaminaSlots();
        UpdateStaminaUI();
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
    }

    [ContextMenu("테스트 - 스태미나 회복")]
    public void TestRecoverStamina()
    {
        if (staminaAction != null)
        {
            staminaAction.RecoverStamina(1);
        }
    }

    [ContextMenu("테스트 - 칸 재생성")]
    public void TestRefreshSlots()
    {
        RefreshStaminaSlots();
    }
}