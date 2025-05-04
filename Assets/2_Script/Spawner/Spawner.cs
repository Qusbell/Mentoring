using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//==================================================
// 다양한 스폰 조건과 방식을 구현할 수 있는 추상 클래스
// 자식 클래스에서 SpawnTrigger()를 오버라이드하여 스폰 조건 구현
//==================================================
abstract public class Spawner : MonoBehaviour
{
    // 지정한 오브젝트 (생성할 프리팹들과 위치 지정)
    [SerializeField] protected List<GameObject> targetPrefabs = new List<GameObject>(); // 순차적으로 생성할 프리팹 목록
    [SerializeField] protected Vector3 spawnLocation;  // 오브젝트를 생성할 위치(빈 오브젝트 등) 인스펙터에서 할당

    // 스폰 상태 관리 변수
    protected bool spawnTrigger = false;   // 스폰 조건 만족 여부 (이걸 true로 만들면 == 생성)
    protected int currentPrefabIndex = 0;  // 현재 생성할 프리팹의 인덱스 (몇 번째 프리팹인지)
    protected bool isCompleted = false;    // 모든 프리팹이 생성 완료되었는지 여부 (true면 더 이상 생성 안 함)


    // 스폰 위치 임시 초기화
    protected virtual void Awake()
    { SetSpawnLocation(); }


    protected virtual void Update()
    {
        // [1] 조건 체크
        // 모든 프리팹 생성이 완료되었거나 || 스폰 조건이 만족되지 않으면 실행 X
        if (isCompleted || !spawnTrigger) { return; }

        // [2] 생성
        SpawnObject();         // 현재 인덱스의 프리팹 생성
        currentPrefabIndex++;  // 다음 프리팹 인덱스로 이동
        spawnTrigger = false;  // 1개 생성 후 : 다시 잠금

        // [3] 완료 체크
        if (currentPrefabIndex >= targetPrefabs.Count)
        { isCompleted = true; }  // 모든 프리팹 생성 완료 (더 이상 생성 안 함)
    }


    // 생성 조건 만족 시 호출
    public virtual void SpawnTriggerOn()
    { spawnTrigger = true; }


    // 오브젝트 생성
    protected virtual void SpawnObject()
    {
        // 리스트가 비어있지 않고, 현재 인덱스가 유효한 범위인지 확인
        if (targetPrefabs.Count > 0 && currentPrefabIndex < targetPrefabs.Count)
        // 현재 인덱스의 프리팹, 지정된 위치, 기본 회전값으로 생성
        { Instantiate(targetPrefabs[currentPrefabIndex], spawnLocation, Quaternion.identity); }
    }


    // 스포너 초기화 - 외부에서 호출하여 스포너 재사용
    public virtual void ResetSpawner()
    {
        currentPrefabIndex = 0;    // 인덱스 초기화 (첫 번째 프리팹부터 다시 시작)
        isCompleted = false;       // 완료 상태 초기화 (다시 스폰 가능하게 설정)
    }


    // 스폰 위치 지정
    public virtual void SetSpawnLocation()
    { spawnLocation = transform.position; }
}