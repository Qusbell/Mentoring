using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터가 나오는 큐브 컴포넌트 (이동 시작 위치 스폰 지원)
/// 큐브 활성화 시 또는 도착 시 자동으로 몬스터 스폰
/// </summary>
public class MonsterCube : MonoBehaviour
{
    [Header("연결 대상")]
    [Tooltip("감지할 큐브 무버 (비어있으면 자동으로 찾음)")]
    public CubeMover cubeMover;

    [Tooltip("활성화할 몬스터 스포너 (비어있으면 자동으로 찾음)")]
    public MonsterSpawner monsterSpawner;

    [Header("스폰 타이밍 설정")]
    [Tooltip("체크하면 큐브 활성화와 동시에 이동 시작 위치에서 스폰 (기본: 이동 완료 후 스폰)")]
    public bool spawnOnActivation = false;

    [Header("스폰 설정")]
    [Tooltip("스폰 전 대기 시간 (초)")]
    public float delayBeforeSpawn = 0f;

    [Header("디버그")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = true;

    // 내부 상태
    private bool hasSpawnTriggered = false;
    private bool hasActivated = false;

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
        if (monsterSpawner == null)
        {
            Debug.LogError($"[{gameObject.name}] MonsterSpawner 컴포넌트를 찾을 수 없습니다");
            return;
        }

        if (showDebugLog)
        {
            string timing = spawnOnActivation ? "이동 시작 위치에서 스폰" : "이동 완료 후 스폰";
            Debug.Log($"[{gameObject.name}] MonsterCube 초기화 완료. 스폰 타이밍: {timing}");
        }

        // 즉시 스폰이 체크되어 있으면 바로 실행
        if (spawnOnActivation)
        {
            CheckActivationSpawn();
        }
    }

    void Update()
    {
        // 이미 스폰했으면 체크하지 않음
        if (hasSpawnTriggered)
        {
            return;
        }

        // 즉시 스폰이 아닌 경우에만 이동 완료 체크
        if (!spawnOnActivation)
        {
            CheckArrivalSpawn();
        }
    }

    // 활성화 시 스폰 체크 (이동 시작 위치에서)
    private void CheckActivationSpawn()
    {
        if (hasSpawnTriggered) return;

        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] 큐브 활성화 감지 - 이동 시작 위치에서 몬스터 스폰을 시작합니다.");
        }

        TriggerSpawnAtMoveStartPosition();
        hasSpawnTriggered = true;
        hasActivated = true;
    }

    // 도착 시 스폰 체크 (기본 동작)
    private void CheckArrivalSpawn()
    {
        if (hasSpawnTriggered) return;

        // CubeMover가 없으면 즉시 스폰 (정적 큐브)
        if (cubeMover == null)
        {
            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] CubeMover가 없는 정적 큐브. 즉시 몬스터 스폰.");
            }

            TriggerSpawn();
            hasSpawnTriggered = true;
            return;
        }

        // 큐브가 도착했는지 체크
        if (cubeMover.HasArrived)
        {
            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] 큐브 도착 감지 몬스터 스폰을 시작합니다.");
            }

            TriggerSpawn();
            hasSpawnTriggered = true;
        }
    }

    // 이동 시작 위치에서 스폰 트리거
    private void TriggerSpawnAtMoveStartPosition()
    {
        if (monsterSpawner == null)
        {
            Debug.LogError($"[{gameObject.name}] MonsterSpawner가 없어서 스폰할 수 없습니다");
            return;
        }

        // CubeMover가 있으면 이동 시작 위치 계산
        Vector3 spawnPosition;
        if (cubeMover != null)
        {
            // 이동 시작 위치 = 현재 위치 + CubeMover의 startPositionOffset
            spawnPosition = transform.position + cubeMover.startPositionOffset;

            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] 이동 시작 위치에서 스폰: {spawnPosition} (오프셋: {cubeMover.startPositionOffset})");
            }
        }
        else
        {
            // CubeMover가 없으면 현재 위치 사용
            spawnPosition = transform.position;

            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] CubeMover가 없어 현재 위치에서 스폰: {spawnPosition}");
            }
        }

        // MonsterSpawner를 이동 시작 위치로 임시 이동
        Vector3 originalPosition = monsterSpawner.transform.position;
        monsterSpawner.transform.position = spawnPosition;

        if (delayBeforeSpawn > 0)
        {
            StartCoroutine(DelayedSpawnAndRestore(originalPosition));
        }
        else
        {
            ActivateSpawner();
            // 다음 프레임에 원래 위치로 복원
            StartCoroutine(RestoreSpawnerPosition(originalPosition));
        }
    }

    // 기본 스폰 트리거 (도착 시 - 기본 위치에서)
    private void TriggerSpawn()
    {
        if (monsterSpawner == null)
        {
            Debug.LogError($"[{gameObject.name}] MonsterSpawner가 없어서 스폰할 수 없습니다");
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

    // 딜레이 후 스폰하고 위치 복원
    private IEnumerator DelayedSpawnAndRestore(Vector3 originalPosition)
    {
        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] {delayBeforeSpawn}초 대기 후 이동 시작 위치에서 스폰 시작");
        }

        yield return new WaitForSeconds(delayBeforeSpawn);
        ActivateSpawner();

        yield return RestoreSpawnerPosition(originalPosition);
    }

    // 딜레이 후 스폰 (기본 위치)
    private IEnumerator DelayedSpawn()
    {
        if (showDebugLog)
        {
            Debug.Log($"[{gameObject.name}] {delayBeforeSpawn}초 대기 후 스폰 시작");
        }

        yield return new WaitForSeconds(delayBeforeSpawn);
        ActivateSpawner();
    }

    // MonsterSpawner 위치 복원
    private IEnumerator RestoreSpawnerPosition(Vector3 originalPosition)
    {
        yield return new WaitForEndOfFrame();

        if (monsterSpawner != null)
        {
            monsterSpawner.transform.position = originalPosition;

            if (showDebugLog)
            {
                Debug.Log($"[{gameObject.name}] MonsterSpawner 위치 복원 완료: {originalPosition}");
            }
        }
    }

    // 스포너 활성화
    private void ActivateSpawner()
    {
        if (monsterSpawner != null)
        {
            monsterSpawner.SpawnTriggerOn();

            if (showDebugLog)
            {
                string timing = spawnOnActivation ? "이동 시작 위치에서 스폰" : "이동 완료 후 스폰";
                Debug.Log($"[{gameObject.name}] 몬스터 스포너 활성화 완료 (타이밍: {timing})");
            }
        }
    }

    // 수동 스폰 트리거 (테스트용)
    [ContextMenu("수동 스폰 트리거")]
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

        if (spawnOnActivation)
        {
            CheckActivationSpawn();
        }
        else
        {
            TriggerSpawn();
            hasSpawnTriggered = true;
        }
    }

    // 상태 초기화 (재사용을 위해)
    public void ResetMonsterCube()
    {
        hasSpawnTriggered = false;
        hasActivated = false;

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

    public bool HasActivated
    {
        get { return hasActivated; }
    }

    public bool CanTriggerSpawn
    {
        get { return !hasSpawnTriggered && monsterSpawner != null; }
    }

    public bool IsActivationMode
    {
        get { return spawnOnActivation; }
    }
}