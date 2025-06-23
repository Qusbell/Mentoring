using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터가 나오는 큐브 컴포넌트
/// 큐브 도착 시 자동으로 몬스터 스폰
/// </summary>
public class MonsterCube : MonoBehaviour
{
    [Header("연결 대상")]
    [Tooltip("감지할 큐브 무버 (비어있으면 자동으로 찾음)")]
    public CubeMover cubeMover;

    [Tooltip("활성화할 몬스터 스포너 (비어있으면 자동으로 찾음)")]
    public MonsterSpawner monsterSpawner;

    [Header("스폰 설정")]
    [Tooltip("스폰 전 대기 시간 (초)")]
    public float delayBeforeSpawn = 0f;

    [Header("디버그")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    // 내부 상태
    private bool hasSpawnTriggered = false;

    void Start()
    {
        // 자동으로 컴포넌트 찾기
        if (cubeMover == null)
        {
            cubeMover = GetComponent<CubeMover>();
        }

        if (monsterSpawner == null)
        {
            monsterSpawner = GetComponent<MonsterSpawner>();
        }

        // 컴포넌트 확인
        if (cubeMover == null)
        {
            Debug.LogError($"[{gameObject.name}] CubeMover 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        if (monsterSpawner == null)
        {
            Debug.LogError($"[{gameObject.name}] MonsterSpawner 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] MonsterCube 초기화 완료. 큐브 도착 시 몬스터 스폰 예정.");
        }
    }

    void Update()
    {
        // 이미 스폰했으면 체크하지 않음
        if (hasSpawnTriggered)
        {
            return;
        }

        // 큐브가 도착했는지 체크
        if (cubeMover != null && cubeMover.HasArrived)
        {
            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] 큐브 도착 감지! 몬스터 스폰을 시작합니다.");
            }

            TriggerSpawn();
            hasSpawnTriggered = true;
        }
    }

    // 스폰 트리거
    private void TriggerSpawn()
    {
        if (monsterSpawner == null)
        {
            Debug.LogError($"[{gameObject.name}] MonsterSpawner가 없어서 스폰할 수 없습니다!");
            return;
        }

        if (delayBeforeSpawn > 0)
        {
            // 딜레이가 있으면 코루틴으로 처리
            StartCoroutine(DelayedSpawn());
        }
        else
        {
            // 즉시 스폰
            ActivateSpawner();
        }
    }

    // 딜레이 후 스폰
    private IEnumerator DelayedSpawn()
    {
        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] {delayBeforeSpawn}초 대기 후 스폰 시작...");
        }

        yield return new WaitForSeconds(delayBeforeSpawn);

        ActivateSpawner();
    }

    // 스포너 활성화
    private void ActivateSpawner()
    {
        if (monsterSpawner != null)
        {
            monsterSpawner.SpawnTriggerOn();

            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] 몬스터 스포너 활성화 완료!");
            }
        }
    }

    // 수동 스폰 트리거 (테스트용)
    public void ManualSpawnTrigger()
    {
        if (hasSpawnTriggered)
        {
            Debug.LogWarning($"[{gameObject.name}] 이미 스폰이 실행되었습니다.");
            return;
        }

        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] 수동으로 스폰을 트리거합니다.");
        }

        TriggerSpawn();
        hasSpawnTriggered = true;
    }

    // 상태 초기화 (재사용을 위해)
    public void ResetMonsterCube()
    {
        hasSpawnTriggered = false;

        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] MonsterCube 상태 초기화 완료.");
        }
    }

    // 상태 확인용 프로퍼티들
    public bool HasSpawnTriggered
    {
        get { return hasSpawnTriggered; }
    }

    public bool CanTriggerSpawn
    {
        get { return !hasSpawnTriggered && cubeMover != null && monsterSpawner != null; }
    }
}