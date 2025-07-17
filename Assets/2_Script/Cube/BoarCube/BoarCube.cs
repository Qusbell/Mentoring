using UnityEngine;

/// <summary>
/// 멧돼지 큐브 - 메인 컴포넌트 (이것만 큐브에 붙이면 됨)
/// </summary>
[RequireComponent(typeof(BoarMovement))]
[RequireComponent(typeof(BoarWarning))]
[RequireComponent(typeof(BoarKnockback))]
public class BoarCube : MonoBehaviour
{
    [Header("CubeMover 스타일 이동 설정")]
    public Vector3 startPositionOffset = new Vector3(10, 0, 0);
    public float launchSpeed = 8f;

    [Header("타이밍")]
    public float warningDuration = 2f;
    public float deactivateDelay = 0.5f;

    [Header("공격 범위")]
    public float attackWidth = 0f; // 0 = 큐브 크기와 동일

    [Header("시각 효과")]
    public float warningAlpha = 0.7f;

    [Header("넉백 시스템")]
    public bool enableKnockback = true;
    public float knockbackForce = 12f;
    public float knockbackDuration = 0.5f;
    public float knockbackRadius = 0.3f;
    public string[] knockbackTags = { "Player", "Monster" };

    [Header("디버그")]
    public bool showDebugLog = true;

    // 컴포넌트 참조
    private BoarMovement movement;
    private BoarWarning warning;
    private BoarKnockback knockback;

    // 상태
    private bool isLaunched = false;

    void Awake()
    {
        // 컴포넌트 가져오기
        movement = GetComponent<BoarMovement>();
        warning = GetComponent<BoarWarning>();
        knockback = GetComponent<BoarKnockback>();

        // 초기화
        movement.Initialize(this);
        warning.Initialize(this);
        knockback.Initialize(this);
    }

    /// <summary>
    /// 멧돼지 큐브 시작 (외부에서 호출)
    /// </summary>
    public void LaunchBoarCube()
    {
        if (isLaunched)
        {
            if (showDebugLog)
                Debug.LogWarning($"[{gameObject.name}] 이미 발사된 상태입니다.");
            return;
        }

        isLaunched = true;
        StartCoroutine(BoarCubeSequence());
    }

    /// <summary>
    /// 즉시 발사 (경고 시간 없음)
    /// </summary>
    public void LaunchImmediately()
    {
        if (isLaunched) return;

        isLaunched = true;
        StartCoroutine(ImmediateLaunchSequence());
    }

    /// <summary>
    /// 멧돼지 큐브 리셋
    /// </summary>
    public void ResetBoarCube()
    {
        StopAllCoroutines();
        warning.ClearWarning();
        knockback.Reset();
        movement.Reset();

        isLaunched = false;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 멧돼지 큐브 리셋 완료");
    }

    /// <summary>
    /// 시작 위치 오프셋 설정
    /// </summary>
    public void SetStartPositionOffset(Vector3 offset)
    {
        startPositionOffset = offset;
        movement.UpdatePositions(offset);
    }

    // ==================== 시퀀스 관리 ====================

    private System.Collections.IEnumerator BoarCubeSequence()
    {
        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 멧돼지 큐브 시퀀스 시작!");

        // 1. 경고 표시
        warning.ShowWarning();

        // 2. 경고 시간 대기
        yield return new WaitForSeconds(warningDuration);

        // 3. 경고 제거
        warning.ClearWarning();

        // 4. 돌진 시작
        yield return StartCoroutine(movement.ExecuteLaunch());

        // 5. 완료 처리
        yield return new WaitForSeconds(deactivateDelay);
        gameObject.SetActive(false);
    }

    private System.Collections.IEnumerator ImmediateLaunchSequence()
    {
        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 즉시 멧돼지 돌진!");

        yield return StartCoroutine(movement.ExecuteLaunch());
        yield return new WaitForSeconds(deactivateDelay);
        gameObject.SetActive(false);
    }
}