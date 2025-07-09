using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class MonsterSpawner : Spawner
{
    // 생성 주기
    [SerializeField] protected float spawnRate = 2f;
    // 시작과 함께 트리거 작동시킬지 설정
    [SerializeField] bool startTrigger = false;
    // 끝없이 스폰시킬지 설정
    [SerializeField] bool isEndlessSpawn = false;
    private enum SpawnType
    {
        OnTop,  // 오브젝트의 위에서 생성될지 결정
        Around  // 오브젝트 주변에서 생성될지 결정
    }
    // 오브젝트를 어떤 방식으로 생성할지 결정
    [SerializeField] SpawnType spawnType = SpawnType.OnTop;
    // 초기화
    protected void Start()
    {
        myCollider = GetComponent<Collider>();
        if (myCollider == null)
        {
            Debug.Log("콜라이더 존재하지 않음 : " + gameObject.name);
            return;
        }
        if (startTrigger)
        { SpawnTriggerOn(); }
    }
    // ===== 스폰 위치 =====
    // 현재 오브젝트의 콜라이더
    protected Collider myCollider;
    // 윗면 중앙 계산 (하위 콜라이더들 포함)
    protected override void SetSpawnLocation()
    {
        // SpawnType에 따라서 다르게 설정
        switch (spawnType)
        {
            case SpawnType.OnTop:
                // 하위 콜라이더들을 모두 포함해서 윗면 정중앙 계산
                Bounds combinedBounds = GetCombinedBoundsFromChildren();
                Vector3 topCenter = combinedBounds.center + Vector3.up * combinedBounds.extents.y;
                // 추가 높이 오프셋 적용
                spawnLocation = topCenter;
                break;
            case SpawnType.Around:
                // <- 4가지 방향 결정 (일단은)
                break;
        }
    }
    // 스폰 가능한 장소를 모두 List로 생성
    // <- 나중에 반응 보고 결정
    private List<Vector3> GetAroundSpawnLocations()
    {
        List<Vector3> aroundSpawnLocations = new List<Vector3>();
        return aroundSpawnLocations;
    }
    // 하위 오브젝트들의 모든 콜라이더 범위를 합치기
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
        { combinedBounds.Encapsulate(allColliders[i].bounds); }
        return combinedBounds;
    }
    // ===== 트리거 / 생성 / 완료 =====
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
    // 생성
    protected override void SpawnObject()
    {
        // 스폰 트리거가 켜져있다면
        if (spawnTrigger)
        {
            // 오브젝트 생성
            Debug.Log(PrefabIndex + "번째 몬스터 생성");
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
            base.CheckCompleted();
            // 주기적 스포너라면: 리셋 발생
            if (isEndlessSpawn) { ResetSpawner(); }
        }
        else
        {
            // 다음 프리펩 인덱스 지정
            PrefabIndex += 1;
        }
    }
}