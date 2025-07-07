using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// 단순화된 큐브 붕괴 컴포넌트 (개선버전)
/// 플레이어 접근, 시간 경과, 외부 트리거, 에리어 트리거에 의해 붕괴되는 큐브
/// </summary>
public class CubeCollapser : MonoBehaviour
{
    [Header("트리거 설정")]
    [Tooltip("붕괴 트리거 유형")]
    public TriggerType triggerType = TriggerType.PlayerProximity;

    // 트리거 타입 정의
    public enum TriggerType
    {
        Time,            // 시간 기반 (일정 시간 후 붕괴)
        PlayerProximity, // 플레이어 근접
        ExternalTrigger, // 외부 호출에 의한 트리거
        AreaTrigger      // 에리어 트리거 (플레이어가 지정된 영역에 들어오면 붕괴)
    }

    [Tooltip("플레이어 태그")]
    public string playerTag = "Player";

    [Tooltip("플레이어 근접 트리거 거리")]
    public float triggerDistance = 0.1f;

    [Tooltip("붕괴 전 대기 시간 (초)")]
    public float warningDelay = 1f;

    [Header("에리어 트리거 설정 (AreaTrigger 모드용)")]
    [Tooltip("트리거 영역 오브젝트 (빈 오브젝트 + 콜라이더)")]
    public GameObject triggerArea;

    [Tooltip("한 번만 트리거되는지 여부")]
    public bool oneTimeUse = true;

    [Header("디버그 설정")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    // 내부 고정 설정 (Inspector에서 수정 불가)
    private const float COLLAPSE_SPEED = 15f;         // 붕괴 속도
    private const float DEACTIVATE_DISTANCE = 10f;   // 비활성화 거리
    private const float DEACTIVATE_TIME = 2f;        // 비활성화 시간
    private const float SHAKE_DURATION = 2.0f;       // 흔들림 지속 시간
    private const float INITIAL_SHAKE_INTENSITY = 0.05f; // 초기 흔들림 강도
    private const float MAX_SHAKE_INTENSITY = 0.2f;  // 최대 흔들림 강도
    private const float SHAKE_SPEED = 5f;           // 흔들림 속도
    private const float SHAKE_ACCELERATION = 5.0f;   // 흔들림 가속화 비율

    // 큐브 상태 정의
    private enum CubeState
    {
        Idle,       // 대기 상태
        Shaking,    // 흔들림 상태
        Falling,    // 떨어지는 상태
        Collapsed   // 붕괴 완료
    }

    // 내부 변수
    private CubeState currentState = CubeState.Idle;
    private Transform playerTransform;
    private Vector3 originalPosition;
    private float currentShakeIntensity;
    private float fallenDistance = 0f;
    private float shakeTimer = 0f;
    private float sqrTriggerDistance;
    private bool hasTriggered = false; // 에리어 트리거용

    // 시작 시 초기화
    void Awake()
    {
        // 원래 위치 저장
        originalPosition = transform.position;

        // 거리 계산 최적화를 위한 제곱값 미리 계산
        sqrTriggerDistance = triggerDistance * triggerDistance;

        // 에리어 트리거 모드에서 트리거 영역 설정
        if (triggerType == TriggerType.AreaTrigger)
        {
            SetupAreaTrigger();
        }
        else if (triggerType == TriggerType.ExternalTrigger)
        {
            // ExternalTrigger 모드에서 자기 자신을 트리거로 설정
            SetupSelfTrigger();
        }
    }

    void Start()
    {
        // 플레이어 찾기 (한 번만 실행)
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // 시간 트리거인 경우 자동으로 붕괴 시작
        if (triggerType == TriggerType.Time)
        {
            StartCoroutine(StartCollapseProcedure());
        }
    }

    // 에리어 트리거용 설정
    private void SetupAreaTrigger()
    {
        if (triggerArea != null)
        {
            Collider triggerCol = triggerArea.GetComponent<Collider>();
            if (triggerCol == null)
            {
                triggerCol = triggerArea.AddComponent<BoxCollider>();
                if (showDebugLog)
                    Debug.Log($"[{gameObject.name}] 트리거 영역에 BoxCollider가 자동 추가됨: {triggerArea.name}");
            }

            // 트리거로 설정
            triggerCol.isTrigger = true;

            // CollapseTrigger 컴포넌트 확인 및 추가
            CollapseTrigger triggerComponent = triggerArea.GetComponent<CollapseTrigger>();
            if (triggerComponent == null)
            {
                triggerComponent = triggerArea.AddComponent<CollapseTrigger>();
                if (showDebugLog)
                    Debug.Log($"[{gameObject.name}] 트리거 영역에 CollapseTrigger가 자동 추가됨: {triggerArea.name}");
            }

            // 자기 자신을 타겟으로 등록
            if (!triggerComponent.targetCollapsers.Contains(this))
            {
                triggerComponent.targetCollapsers.Add(this);
                if (showDebugLog)
                    Debug.Log($"[{gameObject.name}] 자기 자신이 트리거 타겟으로 등록됨");
            }

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] AreaTrigger 설정 완료. 트리거 영역: {triggerArea.name}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] AreaTrigger 모드이지만 triggerArea가 설정되지 않았습니다!");
        }
    }

    // ExternalTrigger용 자기 자신 콜라이더 설정
    private void SetupSelfTrigger()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] ExternalTrigger용 BoxCollider가 자동 추가됨");
        }

        // 트리거로 설정
        col.isTrigger = true;
    }

    // 매 프레임 실행 
    void Update()
    {
        // 현재 상태에 따른 처리
        switch (currentState)
        {
            case CubeState.Idle:
                CheckPlayerProximity();
                break;

            case CubeState.Shaking:
                UpdateShaking();
                break;

            case CubeState.Falling:
                UpdateFalling();
                break;

            case CubeState.Collapsed:
                // 이미 붕괴됨 - 아무것도 하지 않음
                break;
        }
    }

    // 플레이어 근접 확인
    private void CheckPlayerProximity()
    {
        if (triggerType != TriggerType.PlayerProximity || playerTransform == null) return;

        float sqrDistance = (transform.position - playerTransform.position).sqrMagnitude;
        if (sqrDistance <= sqrTriggerDistance)
        {
            StartCoroutine(StartCollapseProcedure());
        }
    }

    // 흔들림 상태 업데이트
    private void UpdateShaking()
    {
        // 흔들림 타이머 증가
        shakeTimer += Time.deltaTime;

        // 특정 시간이 지나면 흔들림 단계 완료
        if (shakeTimer >= SHAKE_DURATION)
        {
            // 흔들림 종료, 떨어지기 시작
            currentState = CubeState.Falling;

            // 오브젝트 위치를 원래 위치로 정확히 재조정 (흔들림 중지)
            transform.position = new Vector3(
                originalPosition.x,
                transform.position.y,  // Y 유지
                originalPosition.z
            );

            // NavMesh에서 즉시 제거하기 위해 NavMeshModifier 추가하여 제외시키기
            NavMeshModifier modifier = GetComponent<NavMeshModifier>();
            if (modifier == null)
            {
                modifier = gameObject.AddComponent<NavMeshModifier>();
            }
            modifier.overrideArea = true;
            modifier.area = 1; // Not Walkable 영역으로 설정

            // 몬스터가 스폰된 후에만 베이크 (수정된 부분)
            if (NavMeshManager.hasAnyMonsterSpawned)
            {
                NavMeshManager.instance.Rebuild();
                Debug.Log($"[{gameObject.name}] 큐브 붕괴 - NavMesh 베이크");
            }

            return;
        }

        // 진행률에 따라 흔들림 강도 계산 (지수적으로 증가)
        float progress = shakeTimer / SHAKE_DURATION; // 0 ~ 1 범위

        // 비선형 흔들림 강도 (시간이 지날수록 더 빨리 증가)
        float intensityFactor = Mathf.Pow(progress, SHAKE_ACCELERATION);

        // 초기 강도에서 최대 강도로 증가
        currentShakeIntensity = Mathf.Lerp(INITIAL_SHAKE_INTENSITY, MAX_SHAKE_INTENSITY, intensityFactor);

        // 시간 경과에 따라 더 빠르게 흔들림 (진행률에 따라 속도 증가)
        float currentShakeSpeed = SHAKE_SPEED * (1f + progress);

        // 시간에 따른 흔들림 위치 계산
        float time = Time.time * currentShakeSpeed;
        float xOffset = Mathf.Sin(time * 0.9f) * currentShakeIntensity;
        float zOffset = Mathf.Sin(time * 1.1f) * currentShakeIntensity;

        // 진행률이 높아질수록 더 무작위적인 움직임 추가
        if (progress > 0.7f)
        {
            xOffset += Mathf.Sin(time * 2.7f) * currentShakeIntensity * 0.3f;
            zOffset += Mathf.Sin(time * 3.1f) * currentShakeIntensity * 0.3f;
        }

        // 위치 적용 (Y축은 유지, X와 Z만 변경)
        transform.position = new Vector3(
            originalPosition.x + xOffset,
            transform.position.y,  // Y축은 현재 높이 유지
            originalPosition.z + zOffset
        );
    }

    // 떨어지는 상태 업데이트
    private void UpdateFalling()
    {
        // 이전 위치 저장
        float prevY = transform.position.y;

        // 아래 방향으로 이동 (고정 속도)
        transform.Translate(Vector3.down * COLLAPSE_SPEED * Time.deltaTime);

        // 떨어진 거리 누적 계산
        fallenDistance += (prevY - transform.position.y);

        // 거리 기반 비활성화 체크
        if (fallenDistance >= DEACTIVATE_DISTANCE)
        {
            DeactivateCube();
        }
    }

    // 붕괴 절차 시작
    private IEnumerator StartCollapseProcedure()
    {
        // 이미 진행 중이면 취소
        if (currentState != CubeState.Idle) yield break;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 붕괴 시작! 트리거 타입: {triggerType}");

        // 경고 대기 시간
        yield return new WaitForSeconds(warningDelay);

        // 흔들림 단계 시작
        currentState = CubeState.Shaking;
        shakeTimer = 0f;
        currentShakeIntensity = INITIAL_SHAKE_INTENSITY;

        // 시간 기반 비활성화 설정
        yield return new WaitForSeconds(SHAKE_DURATION + DEACTIVATE_TIME);

        // 아직 비활성화되지 않았다면
        if (currentState != CubeState.Collapsed)
        {
            DeactivateCube();
        }
    }

    // 큐브 비활성화
    private void DeactivateCube()
    {
        currentState = CubeState.Collapsed;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 붕괴 완료!");

        gameObject.SetActive(false);
    }

    // 직접 붕괴 트리거 (에디터나 다른 스크립트에서 호출 가능)
    public void TriggerCollapse()
    {
        if (currentState == CubeState.Idle)
        {
            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 외부에서 붕괴 트리거됨!");

            StartCoroutine(StartCollapseProcedure());
        }
    }

    // OnTriggerEnter 이벤트 처리 (트리거 콜라이더와 충돌 시)
    private void OnTriggerEnter(Collider other)
    {
        // ExternalTrigger 모드 또는 AreaTrigger 모드에서 처리
        if ((triggerType == TriggerType.ExternalTrigger || triggerType == TriggerType.AreaTrigger) &&
            currentState == CubeState.Idle)
        {
            // AreaTrigger 모드에서 한 번만 트리거 확인
            if (triggerType == TriggerType.AreaTrigger && oneTimeUse && hasTriggered)
            {
                return;
            }

            // 플레이어 태그 확인
            if (other.CompareTag(playerTag))
            {
                if (triggerType == TriggerType.AreaTrigger)
                {
                    hasTriggered = true;
                    if (showDebugLog)
                        Debug.Log($"[{gameObject.name}] 에리어 트리거 발동! 플레이어가 영역 [{triggerArea?.name}]에 진입");
                }
                else if (showDebugLog)
                {
                    Debug.Log($"[{gameObject.name}] 외부 트리거 발동! 플레이어 접촉");
                }

                StartCoroutine(StartCollapseProcedure());
            }
        }
    }

    // 붕괴 큐브 초기화 (재사용 시)
    public void Reset()
    {
        StopAllCoroutines();
        currentState = CubeState.Idle;
        fallenDistance = 0f;
        shakeTimer = 0f;
        hasTriggered = false;
        transform.position = originalPosition;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 큐브 리셋 완료");
    }
}