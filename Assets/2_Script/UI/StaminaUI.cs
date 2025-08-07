using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스태미나 칸 전용 UI (Unity 2022 호환 - Scale 방식)
/// </summary>
public class StaminaUI : MonoBehaviour
{
    private StaminaAction staminaAction;

    [Header("스태미나 칸 UI")]
    [SerializeField] private Transform staminaContainer;     // 스태미나 칸들의 부모
    [SerializeField] private GameObject staminaSlotPrefab;   // 스태미나 칸 프리팹

    [Header("애니메이션 설정")]
    [SerializeField] private float staminaAnimationDuration = 0.2f;
    [SerializeField] private float staminaScaleEffect = 1.3f; // 칸 변화시 스케일 효과
    [SerializeField] private AnimationCurve scaleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("색상 효과")]
    [SerializeField] private bool enableColorFlash = true;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("색상 설정")]
    [SerializeField] private Color staminaBackgroundColor = Color.gray;
    [SerializeField] private Color staminaFillColor = Color.blue;

    // 스태미나 칸 이미지들
    private List<StaminaSlot> staminaSlots = new List<StaminaSlot>();

    // 현재 값
    private int lastStaminaCount = 0;

    // 스태미나 칸 정보를 담는 클래스
    private class StaminaSlot
    {
        public GameObject slotObject;
        public Image backgroundImage;
        public Image fillImage;
        public RectTransform slotRect;

        public StaminaSlot(GameObject obj, Image bg, Image fill)
        {
            slotObject = obj;
            backgroundImage = bg;
            fillImage = fill;
            slotRect = obj.GetComponent<RectTransform>();
        }
    }

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
            CreateStaminaSlot(slot, i);
        }

        lastStaminaCount = staminaAction.maxStaminaValue;
    }

    /// <summary>
    /// 개별 스태미나 칸 생성 (Background + Fill 구조)
    /// </summary>
    private void CreateStaminaSlot(GameObject slot, int index)
    {
        // 외부 배경 이미지 설정
        Image backgroundImage = slot.GetComponent<Image>();
        if (backgroundImage == null)
        {
            backgroundImage = slot.AddComponent<Image>();
        }

        backgroundImage.color = staminaBackgroundColor;
        backgroundImage.sprite = null; // 기본 흰색 사각형 사용

        // 내부 Fill 오브젝트 생성
        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(slot.transform);

        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = staminaFillColor;
        fillImage.sprite = null; // 기본 흰색 사각형 사용

        // Fill RectTransform 설정 (배경보다 약간 작게)
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.offsetMin = Vector2.one * 3; // 3픽셀 여백
        fillRect.offsetMax = Vector2.one * -3; // 3픽셀 여백

        // 스태미나 슬롯 정보 저장
        StaminaSlot staminaSlot = new StaminaSlot(slot, backgroundImage, fillImage);
        staminaSlots.Add(staminaSlot);

        slot.name = $"StaminaSlot_{index}";
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

        StaminaSlot slot = staminaSlots[slotIndex];
        if (slot.backgroundImage == null) yield break;

        Vector3 originalScale = Vector3.one;
        Color originalColor = slot.backgroundImage.color;

        // 1단계: 컬러 플래시 (선택사항)
        if (enableColorFlash)
        {
            slot.backgroundImage.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            slot.backgroundImage.color = originalColor;
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
            slot.slotRect.localScale = originalScale * scale;

            yield return null;
        }

        // 3단계: Fill 상태 변경 (Scale 방식)
        if (slot.fillImage != null)
        {
            slot.fillImage.transform.localScale = fillSlot ? Vector3.one : Vector3.zero;
        }

        // 4단계: 스케일 다운 애니메이션
        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / halfDuration;

            float curveValue = scaleAnimationCurve.Evaluate(progress);
            float scale = Mathf.Lerp(staminaScaleEffect, 1f, curveValue);
            slot.slotRect.localScale = originalScale * scale;

            yield return null;
        }

        // 최종 상태
        slot.slotRect.localScale = originalScale;
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