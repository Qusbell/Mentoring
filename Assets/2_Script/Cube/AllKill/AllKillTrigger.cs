using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 올킬 조건을 체크하는 트리거 컴포넌트
/// CubeController의 KillTrigger와 연동하여 영역 내 모든 몬스터 제거 시 완료 상태 제공
/// </summary>

public class AllKillTrigger : MonoBehaviour
{
    [Header("몬스터 스포너 설정")]
    [Tooltip("감시할 몬스터 스포너들")]
    public List<MonsterSpawner> targetSpawners = new List<MonsterSpawner>();

    [Header("감지 설정")]
    [Tooltip("몬스터 태그")]
    public string monsterTag = "Monster";

    [Tooltip("플레이어 태그")]
    public string playerTag = "Player";

    [Header("디버그")]
    [Tooltip("디버그 로그 출력")]
    public bool showDebugLog = false;

    // 내부 상태 변수들
    private bool isPlayerInArea = false;        // 플레이어 진입 여부
    private bool canCheckKillAll = false;       // 올킬 체크 가능 여부
    private bool isCompleted = false;           // 올킬 달성 여부
    private Collider triggerCollider;          // 트리거 콜라이더

    // 외부에서 접근 가능한 프로퍼티 (CubeController에서 사용)
    public bool IsCompleted => isCompleted;
    public bool IsPlayerInArea => isPlayerInArea;
    public bool CanCheckKillAll => canCheckKillAll;
    public int TargetSpawnerCount => targetSpawners.Count;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError($"[{gameObject.name}] AllKillTrigger에 Collider가 필요");
            this.enabled = false;
            return;
        }
        // 트리거 설정 확인
        if (!triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] Collider가 Trigger로 설정 안했다. Inspector에서 Is Trigger를 체크해라.");
        }
    }

    void Start()
    {
        // 스포너 설정 확인
        if (targetSpawners.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] 감시할 MonsterSpawner가 설정되지 않았다 Inspector에서 Target Spawners를 설정해라.");
        }

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] AllKillTrigger 초기화 완료. 스포너 {targetSpawners.Count}개 감시.");
    }

    void Update()
    {
        // 이미 완료되었으면 중단
        if (isCompleted)
        {
            this.enabled = false;
            return;
        }

        // 플레이어가 진입하지 않았으면 대기
        if (!isPlayerInArea)
            return;

        // 모든 스포너 완료 체크
        if (!canCheckKillAll && AllSpawnersCompleted())
        {
            canCheckKillAll = true;

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 모든 스포너 완료 올킬 체크 시작.");
        }

        // 올킬 체크: 영역 안에 몬스터가 없으면 완료
        if (canCheckKillAll && NoMonstersInArea())
        {
            CompleteKillAll();
        }
    }

    // 모든 스포너가 완료되었는지 확인
    private bool AllSpawnersCompleted()
    {
        if (targetSpawners.Count == 0)
            return false;

        foreach (var spawner in targetSpawners)
        {
            if (spawner == null) continue;
            // MonsterSpawner의 isCompleted 체크
            if (!spawner.IsSpawnerCompleted)
                return false;
        }
        return true;
    }

    // 영역 안에 몬스터가 없는지 확인
    private bool NoMonstersInArea()
    {
        if (triggerCollider == null)
            return false;

        // Physics.OverlapBox를 사용해 영역 내 몬스터 검색
        Vector3 center = triggerCollider.bounds.center;
        Vector3 halfExtents = triggerCollider.bounds.size / 2;

        Collider[] monsters = Physics.OverlapBox(
            center,
            halfExtents,
            transform.rotation
        );

        // 몬스터 태그를 가진 오브젝트가 있는지 확인
        foreach (var col in monsters)
        {
            if (col.CompareTag(monsterTag))
            {
                return false; // 몬스터가 있음
            }
        }

        return true; // 몬스터가 없음
    }

    // 올킬 달성 처리
    private void CompleteKillAll()
    {
        if (isCompleted)
            return; // 중복 실행 방지

        isCompleted = true;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 올킬 달성 ");

        // CubeController가 IsCompleted 프로퍼티를 통해 확인하므로
        // 별도의 신호 전달은 불필요

        // 컴포넌트 비활성화 (성능 최적화)
        this.enabled = false;
    }

    // 트리거 이벤트 처리
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInArea = true;

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 플레이어 진입 올킬 체크 준비.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInArea = false;

            if (showDebugLog)
                Debug.Log($"[{gameObject.name}] 플레이어 퇴장 올킬 중지.");
        }
    }

    // 상태 리셋 (재사용을 위해)
    public void ResetAllKillTrigger()
    {
        isPlayerInArea = false;
        canCheckKillAll = false;
        isCompleted = false;
        this.enabled = true;

        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] AllKillTrigger 상태 리셋 완료.");
    }

    // 수동 올킬 체크 (테스트용)
    [ContextMenu("수동 올킬 체크")]
    public void ManualAllKillCheck()
    {
        if (showDebugLog)
            Debug.Log($"[{gameObject.name}] 수동 올킬 체크 실행");

        if (NoMonstersInArea())
        {
            CompleteKillAll();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 아직 영역 내에 몬스터가 있다.");
        }
    }

    // 디버그용: 씬에서 트리거 영역 시각화
    void OnDrawGizmos()
    {
        if (triggerCollider == null)
            return;

        // 트리거 영역 표시
        Color gizmoColor = Color.red;
        if (isPlayerInArea) gizmoColor = Color.green;
        if (isCompleted) gizmoColor = Color.blue;

        gizmoColor.a = 0.3f;
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(triggerCollider.bounds.center, triggerCollider.bounds.size);

        // 테두리
        gizmoColor.a = 1f;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
    }
}