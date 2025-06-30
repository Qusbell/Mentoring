using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 단순화된 큐브 붕괴 컴포넌트
/// 플레이어 접근, 시간 경과, 외부 트리거, 에리어 트리거에 의해 붕괴되는 큐브
/// 에리어 트리거 모드에서는 다른 큐브들도 함께 붕괴 가능
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
        AreaTrigger      // 에리어 트리거 (다른 큐브들도 함께 붕괴)
    }

    [Tooltip("플레이어 태그")]
    public string playerTag = "Player";

    [Tooltip("플레이어 근접 트리거 거리")]
    public float triggerDistance = 0.1f;

    [Tooltip("붕괴 전 대기 시간 (초)")]
    public float warningDelay = 1f;

    [Header("에리어 트리거 설정 (AreaTrigger 모드용)")]
    [Tooltip("한 번만 트리거되는지 여부")]
    public bool oneTimeUse = true;

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

        // 에리어 트리거 모드에서 콜라이더 설정
        if (triggerType == TriggerType.AreaTrigger)
        {
            SetupAreaTrigger();
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

    // 에리어 트리거용 콜라이더 설정
    private void SetupAreaTrigger()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // 콜라이더가 없으면 박스 콜라이더 추가
            col = gameObject.AddComponent<BoxCollider>();
            Debug.Log($"[{gameObject.name}] 에리어 트리거용 BoxCollider가 자동 추가되었습니다.");
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


    /* <- 컴파일 오류 회피용 통째로 주석처리

    // 추가 큐브들 순차적으로 붕괴 트리거
    private IEnumerator TriggerAdditionalCubes()
    {
        foreach (var cube in additionalCubes)
        {
            if (cube != null && cube != this && cube.currentState == CubeState.Idle)
            {
                cube.TriggerCollapse();

                // 간격이 설정되어 있으면 대기
                if (collapseInterval > 0)
                {
                    yield return new WaitForSeconds(collapseInterval);
                }
            }
        }
    }
    */


    // 큐브 비활성화
    private void DeactivateCube()
    {
        currentState = CubeState.Collapsed;
        gameObject.SetActive(false);
    }

    // 직접 붕괴 트리거 (에디터나 다른 스크립트에서 호출 가능)
    public void TriggerCollapse()
    {
        if (currentState == CubeState.Idle)
        {
            StartCoroutine(StartCollapseProcedure());
        }
    }

    // OnTriggerEnter 이벤트 처리 (트리거 콜라이더와 충돌 시)
    private void OnTriggerEnter(Collider other)
    {
        // 외부 트리거 모드 또는 에리어 트리거 모드에서 처리
        if ((triggerType == TriggerType.ExternalTrigger || triggerType == TriggerType.AreaTrigger) &&
            currentState == CubeState.Idle)
        {
            // 에리어 트리거 모드에서 한 번만 트리거 확인
            if (triggerType == TriggerType.AreaTrigger && oneTimeUse && hasTriggered)
            {
                return;
            }

            // 플레이어 태그가 설정된 경우 태그 확인
            if (!string.IsNullOrEmpty(playerTag))
            {
                if (other.CompareTag(playerTag))
                {
                    if (triggerType == TriggerType.AreaTrigger)
                    {
                        hasTriggered = true;
                        Debug.Log($"[{gameObject.name}] 에리어 트리거 발동! 붕괴 시작");
                    }
                    StartCoroutine(StartCollapseProcedure());
                }
            }
            else // 태그 설정이 안 된 경우 모든 충돌 처리
            {
                if (triggerType == TriggerType.AreaTrigger)
                {
                    hasTriggered = true;
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
    }

    // 디버그용: 씬에서 트리거 영역 시각화
    void OnDrawGizmos()
    {
        if (triggerType == TriggerType.PlayerProximity)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // 주황색, 반투명
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
        else if (triggerType == TriggerType.AreaTrigger)
        {
            // 에리어 트리거 영역 표시
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f); // 빨간색, 반투명
                if (col is BoxCollider)
                {
                    BoxCollider box = col as BoxCollider;
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.matrix = oldMatrix;
                }
                else
                {
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                }
            }

            // 연결된 큐브들과 연결선 표시 (제거됨 - 단순화)
        }
    }
}