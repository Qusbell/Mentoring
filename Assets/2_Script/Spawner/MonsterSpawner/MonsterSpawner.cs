using UnityEngine;
using System.Collections;
public class MonsterSpawner : Spawner
{
    [Header("스폰 위치 설정")]
    [Tooltip("큐브 위에서 추가로 높일 거리 (기본: 0.5유닛)")]
    public float heightOffset = 0.5f;

    // 초기화
    protected void Start()
    {
        targetCollider = GetComponent<Collider>();
        if (targetCollider == null)
        {
            Debug.LogError("콜라이더 존재하지 않음 : " + gameObject.name);
            return;
        }

        // 자동 시작 제거 - MonsterCube에서 호출할 때까지 대기
        Debug.Log($"[{gameObject.name}] MonsterSpawner 초기화 완료. 외부 호출 대기 중...");
    }


    // ===== 스폰 위치 =====
    // 현재 오브젝트의 콜라이더
    protected Collider targetCollider;

    // 윗면 중앙 계산 (하위 콜라이더들 포함)
    public override void SetSpawnLocation()
    {
        // 하위 콜라이더들을 모두 포함해서 윗면 정중앙 계산
        Bounds combinedBounds = GetCombinedBoundsFromChildren();
        Vector3 topCenter = combinedBounds.center + Vector3.up * combinedBounds.extents.y;

        // 추가 높이 오프셋 적용
        spawnLocation = topCenter + Vector3.up * heightOffset;

        Debug.Log($"[{gameObject.name}] 하위 콜라이더 기반 스폰 위치 설정: {spawnLocation}");
    }

    // 하위 오브젝트들의 모든 콜라이더 범위를 합쳐서 계산
    private Bounds GetCombinedBoundsFromChildren()
    {
        // 모든 하위 콜라이더 가져오기 (자기 자신 포함)
        Collider[] allColliders = GetComponentsInChildren<Collider>();

        if (allColliders.Length == 0)
        {
            // 콜라이더가 없으면 Transform 기준으로 기본 크기 사용
            Debug.LogWarning($"[{gameObject.name}] 하위 콜라이더를 찾을 수 없습니다. Transform 크기를 사용합니다.");
            return new Bounds(transform.position, transform.lossyScale);
        }

        // 첫 번째 콜라이더로 초기 범위 설정
        Bounds combinedBounds = allColliders[0].bounds;

        // 나머지 콜라이더들 범위 모두 합치기
        for (int i = 1; i < allColliders.Length; i++)
        {
            combinedBounds.Encapsulate(allColliders[i].bounds);
        }

        Debug.Log($"[{gameObject.name}] 하위 콜라이더 {allColliders.Length}개의 범위 계산 완료. 크기: {combinedBounds.size}");

        return combinedBounds;
    }

    // ===== 트리거 / 생성 / 완료 =====
    // 생성 주기
    [SerializeField] protected float spawnRate = 2f;
    // 1. 스포너 활성화 (MonsterCube에서 호출)
    // 2. 스폰 위치 지정
    // 3. 생성 시작
    public override void SpawnTriggerOn()
    {
        Debug.Log($"[{gameObject.name}] MonsterSpawner 활성화됨! 스폰을 시작합니다.");
        base.SpawnTriggerOn();
        SetSpawnLocation(); // 스폰 위치 재설정 (하위 콜라이더 기반)
        SpawnObject();
    }


    // 끝없이 스폰시킬지 설정
    [SerializeField] bool isEndlessSpawn = false;

    // 생성
    protected override void SpawnObject()
    {
        // 스폰 트리거가 켜져있다면
        if (spawnTrigger)
        {
            // 오브젝트 생성
            base.SpawnObject();

            // 종료 체크
            CheckCompleted();

            // 종료되지 않았다면 : 다음 스폰 예약
            if (!isCompleted) { StartCoroutine(Timer.StartTimer(spawnRate, SpawnObject)); }
        }
    }

    // 종료 확인
    public override void CheckCompleted()
    {
        // 모든 프리펩을 생성했다면
        if (targetPrefabs.Count <= PrefabIndex + 1)
        {
            Debug.Log($"[{gameObject.name}] 몬스터 스폰 완료");
            base.CheckCompleted();

            // 주기적 스포너라면: 리셋 발생
            if (isEndlessSpawn) { ResetSpawner();}
        }
    }
}