using UnityEngine;
using System.Collections;
public class MonsterSpawner : Spawner
{
    // 초기화
    protected void Start()
    {
        targetCollider = GetComponent<Collider>();
        if (targetCollider == null)
        {
            Debug.LogError("콜라이더 존재하지 않음 : " + gameObject.name);
            return;
        }

        // 스폰 위치 미리 설정
        SetSpawnLocation();

        // 자동 시작 제거 - MonsterCube에서 호출할 때까지 대기
        Debug.Log($"[{gameObject.name}] MonsterSpawner 초기화 완료. 외부 호출 대기 중...");
    }
    // ===== 스폰 위치 =====
    // 현재 오브젝트의 콜라이더
    protected Collider targetCollider;
    // 윗면 중앙 계산
    public override void SetSpawnLocation()
    {
        if (targetCollider == null) return;

        Bounds bounds = targetCollider.bounds;
        Vector3 topCenter = bounds.center + Vector3.up * bounds.extents.y;  // 윗면 중앙 위치 계산
        spawnLocation = topCenter;
        // 이후 파묻힘 현상 등 발생 시, 이하 부분 적용
        //    // 필요하다면 추가 오프셋 적용 (예: 살짝 띄우기)
        //    float heightOffset = 0.5f; // 필요에 따라 조정
        //    spawnLocation = topCenter + Vector3.up * heightOffset;
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
        SetSpawnLocation(); // 스폰 위치 재설정
        SpawnObject();
    }
    // 생성
    protected override void SpawnObject()
    {
        base.SpawnObject();
        CheckCompleted();
        // ----- 조건 체크 -----
        // 미완료 && 스폰 트리거 On
        if (!isCompleted && spawnTrigger)
        {
            StartCoroutine(Timer.StartTimer(spawnRate, SpawnObject));
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
            // <- 주기적 스포너라면: 리셋 발생
        }
    }
}