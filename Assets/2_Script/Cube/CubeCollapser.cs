using System;
using System.Collections;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;


// Update로 상태를 변화하지 말고
// 코루틴으로 상태 변화
// 예시:
// Idle 상태  ->  WaitForSeconds  ->  Shaking  ->  WaitForSeconds  ->  Falling


/// <summary>
/// 성능 최적화된 큐브 붕괴 컴포넌트
/// 기존 기능은 100% 유지하면서 불필요한 Update만 제거
/// 자식 오브젝트 (0,0,0) 위치 문제 해결
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
    private float sqrTriggerDistance;

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

    // 성능 최적화 상수
    private const int PROXIMITY_CHECK_INTERVAL = 1;  // 1프레임마다 체크

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
    private bool hasTriggered = false; // 에리어 트리거용

    // 성능 최적화 변수
    private int frameCounter = 0;

    // 시작 시 초기화
    void Awake()
    {
        // 원래 위치 저장
        originalPosition = transform.position;

        // 거리 계산 최적화를 위한 제곱값 미리 계산
        sqrTriggerDistance = triggerDistance * triggerDistance;


        // 에리어 트리거 모드에서 트리거 영역 설정
        switch (triggerType)
        {
            case TriggerType.AreaTrigger:
                SetupAreaTrigger();
                break;

            case TriggerType.ExternalTrigger:
                SetupSelfTrigger();
                break;
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
            StartCollapse();
        }
    }



    void ChangeToShaking()
    {
        // Debug.Log($"{this.gameObject.name} : 흔들림으로 전환");
        currentState = CubeState.Shaking;
        shakeTimer = 0f;
        currentShakeIntensity = INITIAL_SHAKE_INTENSITY;

        Timer.Instance.StartTimer(this, "_Falling", SHAKE_DURATION + DEACTIVATE_TIME, ChangeToFalling);
    }

    void ChangeToFalling()
    {
        if (currentState != CubeState.Collapsed)
        {
            // Debug.Log($"{this.gameObject.name} : 붕괴로 전환");
            currentState = CubeState.Collapsed;
        }
    }

    private void StartCollapse()
    {
        Timer.Instance.StartTimer(this, "_Shaking", warningDelay, ChangeToShaking);
        // Timer.Instance.StartTimer(this, "_Falling", warningDelay + SHAKE_DURATION + DEACTIVATE_TIME, ChangeToFalling);
    }



    // 매 프레임 실행 
    void Update()
    {
        // 현재 상태에 따른 처리
        switch (currentState)
        {
            case CubeState.Idle:
                // 성능 최적화: PlayerProximity만 Update에서 거리 체크
                if (triggerType == TriggerType.PlayerProximity)
                { CheckPlayerProximity(); }
                // 다른 트리거 타입들은 Idle 상태에서만 Update 불필요
                // Shaking이나 Falling 상태가 되면 Update 필요함
                break;

            case CubeState.Shaking:
                UpdateShaking();
                break;

            case CubeState.Falling:
                UpdateFalling();
                break;
        }
    }



    // 성능 최적화된 플레이어 근접 확인
    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;

        // 3프레임마다만 거리 체크
        frameCounter++;
        if (frameCounter >= PROXIMITY_CHECK_INTERVAL)
        {
            frameCounter = 0;

            float sqrDistance = (transform.position - playerTransform.position).sqrMagnitude;

            if (sqrDistance <= sqrTriggerDistance)
            {
                Debug.Log($"{this.gameObject.name} : StartCollapseProcedure");
                StartCollapse();
            }
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
                // triggerCol = triggerArea.AddComponent<BoxCollider>();
                if (showDebugLog)
                    Debug.LogError($"{gameObject.name} : 트리거 영역에 콜라이더 부재");
            }

            // 트리거로 설정
            triggerCol.isTrigger = true;

            // CollapseTrigger 컴포넌트 확인 및 추가
            CollapseTrigger triggerComponent = triggerArea.GetComponent<CollapseTrigger>();
            if (triggerComponent == null)
            {
                // triggerComponent = triggerArea.AddComponent<CollapseTrigger>();
                if (showDebugLog)
                    Debug.Log($"{gameObject.name} : CollapseTrigger 추가해라");
            }

            // 새로운 CollapseTrigger는 triggerArea 참조로 자동 연결되므로 추가 설정 불필요
            // if (showDebugLog)
            //     Debug.Log($"[{gameObject.name}] AreaTrigger 설정 완료. 트리거 영역: {triggerArea.name}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} : AreaTrigger 모드이지만 triggerArea가 설정되지 않았습니다!");
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
        col.isTrigger = true; // <- 콜라이더가 존재하면 무조건 그 콜라이더를 trigger로 설정해버림
    }

    // 재귀적으로 레이어 변경
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        // 자기 자신 레이어 변경
        obj.layer = layer;

        // 모든 자식들의 레이어도 변경
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    // 흔들림 상태 업데이트 (기존 로직 100% 유지)
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

            // 자신 레이어 디폴트화
            SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("Default"));

            // NavMesh 리빌드 - 발판 사라짐
            if (NavMeshManager.instance != null)
            { NavMeshManager.instance.Rebuild(); }
        }
        else
        {
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
    }

    // 떨어지는 상태 업데이트
    // <- 호출 자체가 안 되고 있는 경우가 있음
    private void UpdateFalling()
    {
        // 이전 위치 저장
        float prevY = transform.position.y;

        // 아래 방향으로 이동 (고정 속도)
        transform.Translate(Vector3.down * COLLAPSE_SPEED * Time.deltaTime);

        // 떨어진 거리 누적 계산
        fallenDistance += (prevY - transform.position.y);

        // Debug.Log($"{this.gameObject.name} : 붕괴 중");

        // 거리 기반 비활성화 체크
        if (fallenDistance >= DEACTIVATE_DISTANCE)
        {
            Debug.Log($"{this.gameObject.name} : 붕괴 후 비활성화");
            DeactivateCube();
        }
    }



    // 붕괴 절차 시작
    //  private IEnumerator StartCollapseProcedure()
    //  {
    //      if (currentState != CubeState.Idle)
    //      {
    //          // 중복 붕괴 방지
    //          yield break;
    //      }
    //  
    //      if (showDebugLog)
    //          Debug.Log($"[{gameObject.name}] 붕괴 시작! 트리거 타입: {triggerType}");
    //  
    //      // 경고 대기 시간
    //      yield return new WaitForSeconds(warningDelay);
    //  
    //      // 흔들림 단계 시작
    //      currentState = CubeState.Shaking;
    //      shakeTimer = 0f;
    //      currentShakeIntensity = INITIAL_SHAKE_INTENSITY;
    //  
    //      // 시간 기반 비활성화 설정
    //      yield return new WaitForSeconds(SHAKE_DURATION + DEACTIVATE_TIME);
    //  
    //      // 아직 비활성화되지 않았다면
    //      if (currentState != CubeState.Collapsed)
    //      {
    //          DeactivateCube();
    //      }
    //  }


    // 큐브 비활성화 
    private void DeactivateCube()
    {
        currentState = CubeState.Collapsed;

        //if (showDebugLog)
        // Debug.Log($"[{gameObject.name}] 붕괴 완료!");

        gameObject.SetActive(false);

        // 비활성화 후 컴포넌트도 비활성화
        this.enabled = false;
    }


    // 직접 붕괴 트리거 
    public void TriggerCollapse()
    {
        // AreaTrigger 모드는 상태 체크 없이 강제 진행
        //  if (triggerType == TriggerType.AreaTrigger)
        //  {
            // if (showDebugLog)
            //     Debug.Log($"[{gameObject.name}] 에리어 트리거에 의한 강제 붕괴!");

            // Debug.Log($"{this.gameObject.name} : 붕괴 시작");

            // 상태 초기화 후 붕괴 시작
            currentState = CubeState.Idle;
            StartCollapse();
            //  return;
        //  }

        // 다른 트리거 모드는 기존 로직 유지
        // if (currentState != CubeState.Idle)
        // {
        //     if (showDebugLog)
        //         Debug.Log($"[{gameObject.name}] 이미 붕괴 진행 중이므로 무시됨. 현재 상태: {currentState}");
        //     return;
        // }
        // 
        // if (showDebugLog)
        //     Debug.Log($"[{gameObject.name}] 외부에서 붕괴 트리거됨!");
        // 
        // StartCoroutine(StartCollapseProcedure());
    }


    // OnTriggerEnter 이벤트 처리 (기존 로직 100% 유지)
    //    private void OnTriggerEnter(Collider other)
    //    {
    //        if (other.CompareTag(playerTag))
    //        {
    //            // 한 번만 트리거 확인
    //            if (oneTimeUse && hasTriggered) return;
    //    
    //            hasTriggered = true;
    //    
    //            if (showDebugLog)
    //                Debug.Log($"[{gameObject.name}] 플레이어 감지! 연결된 CubeCollapser들 찾는 중...");
    //    
    //            // 이 에리어를 triggerArea로 설정한 모든 CubeCollapser 찾기 (비활성화된 것도 포함)
    //            CubeCollapser[] allCollapsers = FindObjectsOfType<CubeCollapser>(true);
    //    
    //            int foundCount = 0;
    //    
    //            foreach (CubeCollapser collapser in allCollapsers)
    //            {
    //                // 이 에리어를 참조하고 AND AreaTrigger 모드인 CubeCollapser만 처리
    //                if (collapser != null &&
    //                    collapser.triggerArea == this.gameObject &&
    //                    collapser.triggerType == CubeCollapser.TriggerType.AreaTrigger)  // 이 조건 추가!
    //                {
    //                    foundCount++;
    //    
    //                    if (showDebugLog)
    //                        Debug.Log($"[{gameObject.name}] 연결된 AreaTrigger 큐브 발견: [{collapser.gameObject.name}] - 워닝딜레이 {collapser.warningDelay}초 후 붕괴 시작");
    //    
    //                    // 각 큐브의 워닝딜레이만큼 기다린 후 붕괴 시작
    //                    Timer.Instance.StartTimer(this, collapser.warningDelay, () => {
    //                        if (collapser == null) return;
    //    
    //                        // 큐브가 비활성화되어 있다면 활성화
    //                        if (!collapser.gameObject.activeInHierarchy)
    //                        {
    //                            collapser.gameObject.SetActive(true);
    //                            if (showDebugLog)
    //                                Debug.Log($"[{gameObject.name}] 워닝딜레이 완료 - 큐브 [{collapser.gameObject.name}] 활성화 후 붕괴");
    //                        }
    //                        else
    //                        {
    //                            if (showDebugLog)
    //                                Debug.Log($"[{gameObject.name}] 워닝딜레이 완료 - 큐브 [{collapser.gameObject.name}] 붕괴 시작");
    //                        }
    //    
    //                        // 붕괴 트리거 (AreaTrigger 모드에서 상태 무관하게 작동)
    //                        collapser.TriggerCollapse();
    //                    });
    //                }
    //                // Time 모드나 다른 모드는 무시
    //                else if (collapser != null &&
    //                         collapser.triggerArea == this.gameObject &&
    //                         collapser.triggerType != CubeCollapser.TriggerType.AreaTrigger)
    //                {
    //                    if (showDebugLog)
    //                        Debug.Log($"[{gameObject.name}] 큐브 [{collapser.gameObject.name}]는 {collapser.triggerType} 모드이므로 CollapseTrigger에서 처리하지 않음");
    //                }
    //            }
    //    
    //            if (showDebugLog)
    //            {
    //                if (foundCount > 0)
    //                    Debug.Log($"[{gameObject.name}] 총 {foundCount}개의 AreaTrigger 큐브 발견!");
    //                else
    //                    Debug.LogWarning($"[{gameObject.name}] 이 에리어를 참조하는 AreaTrigger 모드 CubeCollapser를 찾을 수 없습니다.");
    //            }
    //        }
    //    }

    // 붕괴 큐브 초기화 (자식 오브젝트 (0,0,0) 문제 해결)
    public void Reset()
    {
        StopAllCoroutines();
        currentState = CubeState.Idle;
        fallenDistance = 0f;
        shakeTimer = 0f;
        hasTriggered = false;
        frameCounter = 0;  // 최적화 변수 리셋

        // originalPosition이 (0,0,0) 근처면 위치 변경하지 않음 (자식 오브젝트 문제 해결)
        if (originalPosition.magnitude > 0.1f)
        {
            transform.position = originalPosition;
        }

        gameObject.SetActive(true);

        // 리셋 시 적절한 Update 상태로 복원
        if (triggerType == TriggerType.PlayerProximity)
        {
            this.enabled = true;
        }
        else
        {
            this.enabled = false;
        }

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 큐브 리셋 완료");
    }
}