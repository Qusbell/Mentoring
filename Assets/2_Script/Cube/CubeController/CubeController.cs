using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events; // 이벤트 시스템 사용을 위해 추가

/// <summary>
/// 큐브 활성화와 트리거를 관리하는 컴포넌트
/// </summary>
public class CubeController : MonoBehaviour
{
    // 공간 트리거 -> 비정기적 검사 (조건이 작동할 때마다)
    // 시간 트리거 -> 정기적 검사 (매 업데이트마다)


    // -------------------- 초기화 --------------------

    // 시작 시 모든 큐브 확인
    void Start()
    {
        // 큐브 컴포넌트 애드
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
    // MoverAdder
    private void CheckAndAddMoverComponent(GameObject cube)
    {
        CubeMover mover = cube.GetComponent<CubeMover>();
        if (mover == null)
        {
            Debug.LogWarning($"큐브 '{cube.name}'에 CubeMover 컴포넌트가 없습니다. 자동으로 추가됩니다.");
            cube.AddComponent<CubeMover>();
        }
    }



    // -------------------- 컨트롤러 트리거 --------------------

    // 이 컨트롤러가 완료된 후 활성화할 다음 큐브 컨트롤러
    [HideInInspector] public CubeController nextController;

    // 다음 컨트롤러를 활성화하는 트리거 이벤트
    [HideInInspector] public UnityEvent nextCubeControllerActivate;

    // 활성화 확인
    private bool isActivated = false;
    

    // 다음 컨트롤러 활성화 메서드
    public void ActivateNextController()
    {
        if (nextController != null)
        {
            Debug.Log($"[{gameObject.name}] 다음 컨트롤러 [{nextController.gameObject.name}]를 활성화합니다.");
            nextController.StartController();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 다음 컨트롤러가 설정되지 않았습니다.");
        }
    }

    // 컨트롤러 시작 메서드
    public void StartController()
    {
        isActivated = true;
        Debug.Log($"[{gameObject.name}] 컨트롤러 활성화됨");
    }




    // -------------------- 큐브 트리거 --------------------

    // 트리거 조건 타입 정의
    public enum TriggerType
    {
        TimeTrigger,  // 시간 트리거: 일정 시간 경과 후 오브젝트 활성화
        AreaTrigger,  // 영역 트리거: 특정 영역에 플레이어가 들어오면 활성화
        Manual        // 수동 트리거: 코드에서 직접 호출하여 활성화
    }


    // 영역 트리거 감지 시 호출됨
    public void OnAreaTrigger(GameObject triggerArea, GameObject other)
    {
        if (!isActivated) { return; }

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

    // 모든 큐브가 활성화되었는지 확인
    private void CheckAllCubesActivated()
    {
        // 모든 큐브가 활성화되었는지 확인
        foreach (var data in activationSettings)
        {
            // 하나라도 비활성화 상태라면
            // 리턴
            if (!data.hasActivated) { return; }
        }

        // <- activatedCubeCount와 activationSettings.count 의 비교로 바꾸기
        //   if (activatedCubeCount < activationSettings.Count) { return; }


        // 모든 큐브가 활성화되었으면 이벤트 발생
        if (activationSettings.Count > 0)
        {
            Debug.Log($"[{gameObject.name}] 모든 큐브가 활성화되었습니다. 이벤트를 발생시킵니다.");

            // 다음 컨트롤러 활성화 이벤트 발생
            nextCubeControllerActivate?.Invoke();
        }
    }


    // 매 프레임마다 시간 트리거 체크
    void Update()
    {
        // 활성화 체크
        if (!isActivated) { return; }

        // 활성화 상태라면, 큐브 활성화 로직 처리
        foreach (var data in activationSettings)
        {
            // 이미 활성화된 큐브는 스킵
            if (data.hasActivated) { continue; }

            // 시간 트리거 처리
            if (data.triggerType == TriggerType.TimeTrigger)
            {
                data.timer += Time.deltaTime;
                if (data.timer >= data.delayTime)
                { ActivateCube(data); }
            }
        }

        // 모든 큐브 활성화 체크
        CheckAllCubesActivated();
    }




    // -------------------- 활성화 --------------------

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

        [Tooltip("영역 트리거의 대상 태그 (기본: Player)")]
        public string targetTag = "Player";

        [Tooltip("영역 트리거일 경우, 충돌 감지할 영역 오브젝트")]
        public GameObject triggerArea;

        [Tooltip("시간 트리거일 경우, 기다릴 시간")]
        public float delayTime = 0f;

        // 경과한 시간
        [HideInInspector] public float timer = 0f;

        // 활성화 여부
        [HideInInspector] public bool hasActivated = false;
    }

    [Header("큐브 활성화 설정")]
    public List<CubeData> activationSettings = new List<CubeData>();


    // 현재 활성화된 큐브의 숫자
    private int activatedCubeCount = 0;


    // 큐브 숫자 확인
    private int CheckActivatedCubeCount()
    {
        int count = 0;

        foreach (CubeData data in activationSettings)
        {
            if (data.hasActivated)
            { count++; }
        }

        return count;
    }


    // 큐브 활성화
    private void ActivateCube(CubeData data)
    {
        // 활성화되지 않은 큐브라면
        if (data.targetCube != null && !data.hasActivated)
        {
            // 큐브 활성화
            data.targetCube.SetActive(true);
            data.hasActivated = true;
            activatedCubeCount++;

            Debug.Log($"[{gameObject.name}] 큐브 [{data.targetCube.name}]가 활성화되었습니다." +
                $" ({activatedCubeCount}/{activationSettings.Count})");
        }
    }




    // -------------------- 테스트/디버그 --------------------

    [Header("디버그 옵션")]
    [Tooltip("씬 에디터에서 영역 트리거를 시각화")]
    public bool showTriggerAreas = true;

    /*

    // 딜레이 즉시 완료 (테스트/디버그용)
    public void SkipDelay()
    {
        delayTimer = startDelay;
        delayPassed = true;
        Debug.Log($"[{gameObject.name}] 시작 딜레이를 건너뛰었습니다.");
    }


    // 모든 큐브 활성화 이벤트 즉시 트리거 (테스트/디버그용)
    public void TriggerAllCubesActivated()
    {
        if (!hasTriggeredEvent)
        {
            hasTriggeredEvent = true;
            onAllCubesActivated?.Invoke();
        }
    }

    // 모든 큐브 상태 초기화 (테스트/재시작용)
    public void ResetAll()
    {
        // 타이머 초기화
        delayTimer = 0f;
        delayPassed = startDelay <= 0f;
        hasTriggeredEvent = false;
        activatedCubeCount = 0;

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


    // 시작 딜레이 설정 (외부에서 호출 가능)
    public void SetStartDelay(float delayInSeconds)
    {
        startDelay = Mathf.Max(0f, delayInSeconds); // 음수 방지
        ResetAll(); // 설정을 변경했으니 초기화
    }


    // 특정 인덱스의 큐브를 수동으로 활성화
    public void ActivateCubeByIndex(int index)
    {
        // 딜레이가 지나지 않았으면 활성화 무시
        if (!delayPassed) return;

        // 인덱스 범위 체크
        if (index >= 0 && index < activationSettings.Count)
        { ActivateCube(activationSettings[index]); }
    }

    */

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
    }
}