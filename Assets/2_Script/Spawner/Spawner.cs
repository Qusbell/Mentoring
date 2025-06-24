using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//====================
// 다양한 스폰 조건과 방식을 구현할 수 있는 추상 클래스
// 자식 클래스에서 SpawnTrigger()를 오버라이드하여 스폰 조건 구현
//====================
abstract public class Spawner : MonoBehaviour
{
    // 지정한 오브젝트 (생성할 프리팹들과 위치 지정)
    [SerializeField] protected List<GameObject> targetPrefabs = new List<GameObject>(); // 순차적으로 생성할 프리팹 목록

    // 생성할 프리팹의 인덱스 (몇 번째 프리팹인지)
    private int _prefabIndex = 0;
    public int PrefabIndex
    {
        get { return _prefabIndex; }
        protected set
        {
            // null 검사 && 인덱스 검사
            if (targetPrefabs != null && 0 <= value && value < targetPrefabs.Count)
            { _prefabIndex = value; }
        }
    }


    // ===== 생성 조건 충족 여부 =====

    // 스폰 조건 만족 여부 (true로 만들면 == 생성)
    protected bool spawnTrigger = false;

    // 생성 조건 만족 / 트리거 켜기
    public virtual void SpawnTriggerOn()
    { spawnTrigger = true; }

    // 생성 트리거 끄기
    public virtual void SpawnTriggerOFF()
    { spawnTrigger = false; }



    // ===== 완료 / 재활성 =====

    // 스포너 완료 상태 확인
    protected bool isCompleted = false;

    // 스포너 완료 조건
    public virtual void CheckCompleted()
    { isCompleted = true; }

    // 스포너 초기화 (재활성화)
    public virtual void ResetSpawner()
    {
        PrefabIndex = 0;
        isCompleted = false;
    }



    // ===== 생성 / 위치 지정 =====

    // 오브젝트를 생성할 위치
    protected Vector3 spawnLocation;

    // 오브젝트 생성
    protected virtual void SpawnObject()
    {
        if (targetPrefabs.Count < 0) { Debug.Log("스포너 프리펩 인덱스 비어있음"); return; }
        // 현재 인덱스의 프리팹, 지정된 위치, 기본 회전값으로 생성
        Instantiate(targetPrefabs[PrefabIndex], spawnLocation, Quaternion.identity);

        // 다음 프리펩 인덱스 지정
        PrefabIndex += 1;
    }

    // 스폰 위치 지정
    public virtual void SetSpawnLocation()
    { spawnLocation = transform.position; }
}