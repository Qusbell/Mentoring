using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("디버그 옵션")]
    [Tooltip("씬 에디터에서 영역 트리거를 시각화")]
    public bool showTriggerAreas = true;

    // 시작 시 모든 큐브 확인
    void Start()
    {
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
    }

    // 영역 트리거 감지 시 호출됨
    public void OnAreaTrigger(GameObject triggerArea, GameObject other)
    {
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
        if (data.targetCube != null)
        {
            // 큐브 활성화
            data.targetCube.SetActive(true);
            data.hasActivated = true;
        }
    }

    // 특정 인덱스의 큐브를 수동으로 활성화
    public void ActivateCubeByIndex(int index)
    {
        if (index >= 0 && index < activationSettings.Length)
        {
            ActivateCube(activationSettings[index]);
        }
    }

    // 모든 큐브 상태 초기화 (테스트/재시작용)
    public void ResetAll()
    {
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