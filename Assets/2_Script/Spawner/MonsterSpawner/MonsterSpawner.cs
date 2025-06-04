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
            Debug.LogError("Collider component missing!");
            return;
        }
    }




    // ===== 스폰 위치 =====

    // 현재 오브젝트의 콜라이더
    protected Collider targetCollider;

    // 윗면 중앙 계산
    public override void SetSpawnLocation()
    {
        Bounds bounds = targetCollider.bounds;
        Vector3 topCenter = bounds.center + Vector3.up * bounds.extents.y;  // 윗면 중앙 위치 계산
        spawnLocation = topCenter;

        // 이후 파묻힘 현상 등 발생 시, 이하 부분 적용
        //    // 필요하다면 추가 오프셋 적용 (예: 살짝 띄우기)
        //    float heightOffset = 0.5f; // 필요에 따라 조정
        //    spawnLocation = topCenter + Vector3.up * heightOffset;
    }




    // 1. 스포너 활성화
    // 2. 스폰 위치 지정
    public override void SpawnTriggerOn()
    {
        base.SpawnTriggerOn();
        SetSpawnLocation();
    }

    // 종료 확인
    public override void CheckCompleted()
    {
        // 모든 프리펩을 생성했다면
        if (targetPrefabs.Count <= PrefabIndex++)
        {
            base.CheckCompleted();
            // <- 주기적 스포너라면: 리셋 발생
        }
    }




}