using UnityEngine;
using System.Collections;

/// <summary>
/// 멧돼지 큐브 - 메인 컴포넌트
/// 에리어 트리거 또는 보스 호출로 활성화되는 돌진 공격 시스템
/// </summary>
[RequireComponent(typeof(BoarMovement))]
[RequireComponent(typeof(BoarWarning))]
[RequireComponent(typeof(BoarKnockback))]
public class BoarCube : MonoBehaviour
{
    #region ===== 트리거 설정 =====

    [Header("트리거 설정")]
    [Tooltip("멧돼지 트리거 유형")]
    public TriggerType triggerType = TriggerType.AreaTrigger;

    /// <summary>
    /// 멧돼지 큐브 트리거 타입
    /// </summary>
    public enum TriggerType
    {
        AreaTrigger,     // 에리어 트리거 (플레이어가 지정된 영역에 들어오면 발사)
        BossCall         // 보스 호출 (보스가 직접 호출하여 발사)
    }

    [Tooltip("발사 전 대기 시간 (초) - 워닝딜레이")]
    public float warningDelay = 1f;

    [Header("에리어 트리거 설정")]
    [Tooltip("트리거 영역 오브젝트 (빈 오브젝트 + 콜라이더) - AreaTrigger 모드용")]
    public GameObject triggerArea;

    [Tooltip("한 번만 트리거되는지 여부")]
    public bool oneTimeUse = true;

    #endregion

    #region ===== 이동 및 공격 설정 =====

    [Header("이동 설정")]
    [Tooltip("시작 위치 오프셋 (현재 위치 기준)")]
    public Vector3 startPositionOffset = new Vector3(10, 0, 0);

    [Tooltip("돌진 속도")]
    public float launchSpeed = 8f;

    [Tooltip("발사 완료 후 비활성화 대기 시간")]
    public float deactivateDelay = 0.5f;

    [Header("공격 범위")]
    [Tooltip("공격 폭 (0 = 큐브 크기와 동일)")]
    public float attackWidth = 0f;

    [Header("시각 효과")]
    [Tooltip("경고 표시 투명도")]
    public float warningAlpha = 0.7f;

    #endregion

    #region ===== 넉백 시스템 =====

    [Header("넉백 시스템")]
    [Tooltip("넉백 시스템 활성화")]
    public bool enableKnockback = true;

    [Tooltip("넉백 힘")]
    public float knockbackForce = 12f;

    [Tooltip("넉백 지속 시간")]
    public float knockbackDuration = 0.5f;

    [Tooltip("넉백 판정 반지름")]
    public float knockbackRadius = 0.3f;

    [Tooltip("넉백 대상 태그들")]
    public string[] knockbackTags = { "Player", "Monster" };

    #endregion

    #region ===== 디버그 설정 =====

    [Header("디버그")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    #endregion

    #region ===== 내부 변수 =====

    // 컴포넌트 참조
    private BoarMovement movement;
    private BoarWarning warning;
    private BoarKnockback knockback;

    // 상태 관리
    private bool hasTriggered = false;  // 트리거 발동 여부
    private bool isLaunched = false;    // 발사 진행 여부

    // 외부 참조
    private Transform playerTransform;

    #endregion

    #region ===== 초기화 =====

    void Awake()
    {
        InitializeComponents();
        SetupTriggerType();

        // 시작 시 모든 모드에서 비활성화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void InitializeComponents()
    {
        // 필수 컴포넌트 가져오기
        movement = GetComponent<BoarMovement>();
        warning = GetComponent<BoarWarning>();
        knockback = GetComponent<BoarKnockback>();

        // 각 컴포넌트 초기화
        movement.Initialize(this);
        warning.Initialize(this);
        knockback.Initialize(this);
    }

    /// <summary>
    /// 트리거 타입에 따른 설정
    /// </summary>
    private void SetupTriggerType()
    {
        switch (triggerType)
        {
            case TriggerType.AreaTrigger:
                SetupAreaTrigger();
                break;
            case TriggerType.BossCall:
                // BossCall은 별도 설정 불필요
                break;
        }
    }

    #endregion

    #region ===== 트리거 설정 메서드 =====

    /// <summary>
    /// 에리어 트리거 설정
    /// </summary>
    private void SetupAreaTrigger()
    {
        if (triggerArea == null)
        {
            Debug.LogWarning($"[{gameObject.name}] AreaTrigger 모드이지만 triggerArea가 설정되지 않았습니다!");
            return;
        }

        // 트리거 영역의 콜라이더 확인
        Collider triggerCol = triggerArea.GetComponent<Collider>();
        if (triggerCol == null)
        {
            Debug.LogError($"[{gameObject.name}] 트리거 영역에 콜라이더가 없습니다!");
            return;
        }

        // 트리거로 설정
        triggerCol.isTrigger = true;

        // BoarTrigger 컴포넌트 확인
        BoarTrigger triggerComponent = triggerArea.GetComponent<BoarTrigger>();
        if (triggerComponent == null && showDebugLog)
        {
            Debug.LogWarning($"[{gameObject.name}] 트리거 영역에 BoarTrigger 컴포넌트가 필요합니다!");
        }
    }

    #endregion

    #region ===== 공개 메서드 - 외부 호출용 =====

    /// <summary>
    /// 보스가 멧돼지 큐브를 호출할 때 사용하는 메서드
    /// 비활성화된 큐브를 활성화하고 발사
    /// </summary>
    public void BossCallLaunch()
    {
        // 모드 확인
        if (triggerType != TriggerType.BossCall)
        {
            if (showDebugLog)
                Debug.LogWarning($"[{gameObject.name}] BossCall 모드가 아닙니다. 현재 모드: {triggerType}");
            return;
        }

        // 큐브 활성화 (필요시)
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        StartLaunch();
    }

    /// <summary>
    /// 에리어 트리거에서 호출하는 메서드 (BoarTrigger에서 사용)
    /// </summary>
    public void TriggerLaunch()
    {
        // 모드 확인
        if (triggerType != TriggerType.AreaTrigger)
        {
            if (showDebugLog)
                Debug.LogWarning($"[{gameObject.name}] AreaTrigger 모드가 아닙니다. 현재 모드: {triggerType}");
            return;
        }

        // 상태 초기화 후 발사 시작
        hasTriggered = false;
        StartLaunch();
    }

    /// <summary>
    /// 즉시 발사 (경고 시간 없음) - 테스트용
    /// </summary>
    public void LaunchImmediately()
    {
        if (isLaunched)
        {
            if (showDebugLog)
                Debug.LogWarning($"[{gameObject.name}] 이미 발사된 상태입니다.");
            return;
        }

        isLaunched = true;
        StartCoroutine(ImmediateLaunchSequence());
    }

    /// <summary>
    /// 시작 위치 오프셋 설정
    /// </summary>
    public void SetStartPositionOffset(Vector3 offset)
    {
        startPositionOffset = offset;
        movement.UpdatePositions(offset);
    }

    /// <summary>
    /// 멧돼지 큐브 완전 리셋
    /// </summary>
    public void ResetBoarCube()
    {
        // 모든 코루틴 및 타이머 정지
        StopAllCoroutines();
        Timer.Instance.StopTimer(this, "Launch");

        // 각 컴포넌트 리셋
        warning.ClearWarning();
        knockback.Reset();
        movement.Reset();

        // 상태 초기화
        hasTriggered = false;
        isLaunched = false;
    }

    #endregion

    #region ===== 내부 메서드 - 발사 로직 =====

    /// <summary>
    /// 발사 시작 (워닝딜레이 적용)
    /// </summary>
    private void StartLaunch()
    {
        // 중복 실행 방지
        if (hasTriggered && oneTimeUse)
        {
            if (showDebugLog)
                Debug.LogWarning($"[{gameObject.name}] 이미 발사된 상태입니다. (oneTimeUse 설정됨)");
            return;
        }

        hasTriggered = true;

        // 워닝딜레이 후 실제 발사
        Timer.Instance.StartTimer(this, "Launch", warningDelay, LaunchBoarCube);
    }

    /// <summary>
    /// 실제 멧돼지 큐브 발사 (워닝딜레이 후 호출)
    /// </summary>
    private void LaunchBoarCube()
    {
        if (isLaunched)
        {
            if (showDebugLog)
                Debug.LogWarning($"[{gameObject.name}] 이미 발사 진행 중입니다.");
            return;
        }

        isLaunched = true;
        StartCoroutine(BoarCubeSequence());
    }

    #endregion

    #region ===== 코루틴 - 발사 시퀀스 =====

    /// <summary>
    /// 멧돼지 큐브 발사 시퀀스 (경고 → 돌진 → 비활성화)
    /// </summary>
    private System.Collections.IEnumerator BoarCubeSequence()
    {
        // 1단계: 경고 표시
        warning.ShowWarning();

        // 2단계: 경고 시간 대기
        float warningDuration = warning.GetWarningDuration();
        yield return new WaitForSeconds(warningDuration);

        // 3단계: 경고 제거
        warning.ClearWarning();

        // 4단계: 돌진 실행
        yield return StartCoroutine(movement.ExecuteLaunch());

        // 5단계: 완료 후 대기 및 비활성화
        yield return new WaitForSeconds(deactivateDelay);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 즉시 발사 시퀀스 (경고 없이 바로 돌진)
    /// </summary>
    private System.Collections.IEnumerator ImmediateLaunchSequence()
    {
        // 즉시 돌진
        yield return StartCoroutine(movement.ExecuteLaunch());

        // 완료 후 비활성화
        yield return new WaitForSeconds(deactivateDelay);
        gameObject.SetActive(false);
    }

    #endregion
}