using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // 이벤트 시스템 사용을 위해 추가

/// <summary>
/// 큐브 활성화와 트리거를 관리하는 컴포넌트
/// </summary>
public class CubeController : MonoBehaviour
{
    // 트리거 조건 타입 정의
    public enum TriggerType
    {
        TimeTrigger,  // 시간 트리거: 일정 시간 경과 후 오브젝트 활성화
        AreaTrigger,  // 영역 트리거: 특정 영역에 플레이어가 들어오면 활성화
        Manual        // 수동 트리거: 코드에서 직접 호출하여 활성화
    }

    // 큐브 활성화 설정을 저장하는 클래스
    [System.Serializable]
    public class CubeData
    {
        [Header("오브젝트 설정")]
        [Tooltip("활성화할 큐브")]
        public GameObject targetCube;

        [Header("트리거 설정")]
        [Tooltip("활성화 트리거 종류")]
        public TriggerType triggerType = TriggerType.TimeTrigger;

        [Tooltip("시간 트리거일 경우, 기다릴 시간")]
        public float delayTime = 0f;

        [Tooltip("영역 트리거일 경우, 충돌 감지할 영역 오브젝트")]
        public GameObject triggerArea;

        [Tooltip("영역 트리거의 대상 태그 (기본: Player)")]
        public string targetTag = "Player";

        [HideInInspector] public float timer = 0f;
        [HideInInspector] public bool hasActivated = false;
    }

    [Header("큐브 활성화 설정")]
    public CubeData[] activationSettings;

    [Header("전체 시작 설정")]
    [Tooltip("전체 시작 전 추가 대기 시간 (초)")]
    public float globalStartDelay = 0f;

    [Tooltip("전체 시작 딜레이 사용 여부")]
    public bool useGlobalStartDelay = true;

    [Tooltip("외부 트리거 대기 여부 (체크하면 StartExecution 호출 전까지 시작하지 않음)")]
    public bool waitForExternalTrigger = false;

    [Header("연쇄 실행 설정")]
    [Tooltip("모든 큐브가 활성화된 후 트리거할 다음 컨트롤러")]
    public CubeController nextController;

    [Tooltip("다음 컨트롤러로 넘어가기 전 대기 시간 (초)")]
    public float nextControllerDelay = 0f;

    [Tooltip("모든 큐브가 활성화되면 발생하는 이벤트")]
    public UnityEvent onAllCubesActivated;

    [Header("디버그 옵션")]
    [Tooltip("씬 에디터에서 영역 트리거를 시각화")]
    public bool showTriggerAreas = true;

    // 시작 딜레이 타이머
    private float globalStartTimer = 0f;
    private bool globalDelayPassed = false;
    private bool hasStartedExecution = false;
    private bool hasTriggeredNextController = false;
    private int activatedCubeCount = 0;

    // 시작 시 모든 큐브 확인
    void Start()
    {
        // 외부 트리거 대기 설정이 아니면 자동으로 시작 설정
        if (!waitForExternalTrigger)
        {
            hasStartedExecution = true;

            // 글로벌 딜레이가 0이거나 사용하지 않는 경우 바로 시작
            if (globalStartDelay <= 0f || !useGlobalStartDelay)
            {
                globalDelayPassed = true;
            }
        }

        foreach (var data in activationSettings)
        {
            if (data.targetCube != null)
            {
                // CubeMover 컴포넌트가 없으면 추가
                CheckAndAddMoverComponent(data.targetCube);
            }
        }
    }

    // 큐브에 CubeMover 컴포넌트가 있는지 확인하고 없으면 추가
    private void CheckAndAddMoverComponent(GameObject cube)
    {
        CubeMover mover = cube.GetComponent<CubeMover>();
        if (mover == null)
        {
            Debug.LogWarning($"큐브 '{cube.name}'에 CubeMover 컴포넌트가 없습니다. 자동으로 추가됩니다.");
            cube.AddComponent<CubeMover>();
        }
    }

    // 매 프레임마다 조건 확인 (시간 트리거 처리용)
    void Update()
    {
        // 실행이 시작되지 않았으면 처리하지 않음
        if (!hasStartedExecution) return;

        // 글로벌 딜레이 체크
        if (!globalDelayPassed && useGlobalStartDelay)
        {
            globalStartTimer += Time.deltaTime;
            if (globalStartTimer >= globalStartDelay)
            {
                globalDelayPassed = true;
                Debug.Log($"[{gameObject.name}] 전체 시작 딜레이 {globalStartDelay}초가 지났습니다. 큐브 활성화를 시작합니다.");
            }
            else
            {
                return; // 딜레이가 지나지 않았으면 큐브 활성화 로직 실행 안 함
            }
        }

        foreach (var data in activationSettings)
        {
            // 이미 활성화된 큐브는 스킵
            if (data.hasActivated) continue;

            // 시간 트리거 처리
            if (data.triggerType == TriggerType.TimeTrigger)
            {
                data.timer += Time.deltaTime;
                if (data.timer >= data.delayTime)
                {
                    ActivateCube(data);
                }
            }
        }

        // 모든 큐브 활성화 체크
        CheckAllCubesActivated();
    }

    // 모든 큐브가 활성화되었는지 확인
    private void CheckAllCubesActivated()
    {
        // 이미 다음 컨트롤러를 트리거했으면 스킵
        if (hasTriggeredNextController) return;

        // 모든 큐브가 활성화되었는지 확인
        bool allActivated = true;
        foreach (var data in activationSettings)
        {
            if (!data.hasActivated)
            {
                allActivated = false;
                break;
            }
        }

        // 모든 큐브가 활성화되었으면 이벤트 발생
        if (allActivated && activationSettings.Length > 0)
        {
            hasTriggeredNextController = true;
            Debug.Log($"[{gameObject.name}] 모든 큐브가 활성화되었습니다. 이벤트를 발생시킵니다.");

            // 이벤트 발생
            onAllCubesActivated?.Invoke();

            // 다음 컨트롤러 트리거
            StartCoroutine(TriggerNextController());
        }
    }

    // 다음 컨트롤러 트리거
    private IEnumerator TriggerNextController()
    {
        if (nextController != null)
        {
            // 대기 시간 적용
            if (nextControllerDelay > 0)
            {
                Debug.Log($"[{gameObject.name}] {nextControllerDelay}초 후 다음 컨트롤러를 시작합니다.");
                yield return new WaitForSeconds(nextControllerDelay);
            }

            Debug.Log($"[{gameObject.name}] 다음 컨트롤러 [{nextController.gameObject.name}]를 시작합니다.");
            nextController.StartExecution();
        }
    }

    // 영역 트리거 감지 시 호출됨
    public void OnAreaTrigger(GameObject triggerArea, GameObject other)
    {
        // 실행이 시작되지 않았거나 글로벌 딜레이가 지나지 않았으면 트리거 무시
        if (!hasStartedExecution || (!globalDelayPassed && useGlobalStartDelay)) return;

        // 각 활성화 데이터를 확인
        foreach (var data in activationSettings)
        {
            // 이미 활성화된 큐브는 스킵
            if (data.hasActivated) continue;

            // 영역 트리거 조건이고 영역과 태그가 일치하는지 확인
            if (data.triggerType == TriggerType.AreaTrigger &&
                data.triggerArea == triggerArea &&
                other.CompareTag(data.targetTag))
            {
                ActivateCube(data);
            }
        }
    }

    // 큐브 활성화
    void ActivateCube(CubeData data)
    {
        if (data.targetCube != null && !data.hasActivated)
        {
            // 큐브 활성화
            data.targetCube.SetActive(true);
            data.hasActivated = true;
            activatedCubeCount++;

            Debug.Log($"[{gameObject.name}] 큐브 [{data.targetCube.name}]가 활성화되었습니다. ({activatedCubeCount}/{activationSettings.Length})");
        }
    }

    // 특정 인덱스의 큐브를 수동으로 활성화
    public void ActivateCubeByIndex(int index)
    {
        // 실행이 시작되지 않았거나 글로벌 딜레이가 지나지 않았으면 활성화 무시
        if (!hasStartedExecution || (!globalDelayPassed && useGlobalStartDelay)) return;

        if (index >= 0 && index < activationSettings.Length)
        {
            ActivateCube(activationSettings[index]);

            // 모든 큐브 활성화 체크
            CheckAllCubesActivated();
        }
    }

    // 모든 큐브 상태 초기화 (테스트/재시작용)
    public void ResetAll()
    {
        // 글로벌 타이머 초기화
        globalStartTimer = 0f;
        globalDelayPassed = globalStartDelay <= 0f || !useGlobalStartDelay;
        hasTriggeredNextController = false;
        activatedCubeCount = 0;

        // waitForExternalTrigger 설정에 따라 시작 상태 설정
        hasStartedExecution = !waitForExternalTrigger;

        foreach (var data in activationSettings)
        {
            data.hasActivated = false;
            data.timer = 0f;

            if (data.targetCube != null && data.targetCube.activeSelf)
            {
                data.targetCube.SetActive(false);
            }
        }
    }

    // 전체 시작 딜레이 설정 (외부에서 호출 가능)
    public void SetGlobalStartDelay(float delayInSeconds)
    {
        globalStartDelay = Mathf.Max(0f, delayInSeconds); // 음수 방지
        useGlobalStartDelay = true;
        ResetAll(); // 설정을 변경했으니 초기화
    }

    // 실행 시작 (외부에서 호출 가능)
    public void StartExecution()
    {
        if (!hasStartedExecution)
        {
            hasStartedExecution = true;
            globalStartTimer = 0f;
            Debug.Log($"[{gameObject.name}] 실행을 시작합니다.");
        }
    }

    // 딜레이 즉시 완료 (테스트/디버그용)
    public void SkipGlobalDelay()
    {
        globalDelayPassed = true;
    }

    // 연쇄적 실행 즉시 트리거 (테스트/디버그용)
    public void TriggerAllCubesActivated()
    {
        if (!hasTriggeredNextController)
        {
            hasTriggeredNextController = true;
            onAllCubesActivated?.Invoke();
            StartCoroutine(TriggerNextController());
        }
    }

    // 디버그용: 씬에서 영역 트리거와 큐브를 보여줌
    void OnDrawGizmos()
    {
        if (!showTriggerAreas || activationSettings == null) return;

        foreach (var data in activationSettings)
        {
            // 트리거 영역 표시
            if (data.triggerType == TriggerType.AreaTrigger && data.triggerArea != null)
            {
                Collider triggerCollider = data.triggerArea.GetComponent<Collider>();
                if (triggerCollider != null)
                {
                    // 영역 트리거는 반투명 박스로 표시
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                    Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
                }
            }
        }

        // 다음 컨트롤러 연결 표시
        if (nextController != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nextController.transform.position);

            // 화살표 표시
            Vector3 direction = (nextController.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, nextController.transform.position);
            Vector3 arrowPos = transform.position + direction * (distance * 0.8f);

            // 화살표 헤드 그리기
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.5f;
            Gizmos.DrawLine(arrowPos, arrowPos - direction * 0.5f + right);
            Gizmos.DrawLine(arrowPos, arrowPos - direction * 0.5f - right);
        }
    }
}