using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 커스텀 스포너 매니저 클래스 - 오브젝트 생성 조건을 관리하고, 조건에 따라 오브젝트를 활성화함
public class spawn : MonoBehaviour
{
    // 스폰 조건과 관련된 설정을 저장하는 클래스
    [System.Serializable]
    public class SpawnData
    {
        public GameObject targetObject;          // 생성할 오브젝트 (비활성화된 상태에서 시작)
        public TriggerType triggerType;          // 생성 트리거 종류 (시간 or 충돌)
        public float delayTime;                  // 시간 트리거일 경우, 기다릴 시간
        public GameObject triggerColliderObject; // 충돌 트리거일 경우, 충돌 감지할 오브젝트 (플레이어가 여기에 닿으면 생성)

        [HideInInspector] public float timer = 0f;        // 시간 트리거 측정을 위한 타이머
        [HideInInspector] public bool hasSpawned = false; // 이미 생성되었는지 여부
    }

    // 여러 개의 스폰 조건들을 배열로 저장
    public SpawnData[] spawnSettings;

    // 매 프레임마다 조건 확인 (시간 트리거 처리용)
    void Update()
    {
        foreach (var data in spawnSettings)
        {
            // 이미 생성된 오브젝트는 스킵
            if (data.hasSpawned) continue;

            // 트리거 타입에 따라 분기
            switch (data.triggerType)
            {
                case TriggerType.TimeTrigger:
                    data.timer += Time.deltaTime; // 시간 누적
                    if (data.timer >= data.delayTime)
                    {
                        SpawnObject(data); // 시간 조건 만족 시 생성
                    }
                    break;

                case TriggerType.CollisionTrigger:
                    // 충돌은 외부에서 OnCollisionTrigger로 처리됨
                    break;
            }
        }
    }

    // 충돌 감지 시 외부(CollisionReporter 등)에서 호출되는 함수
    public void OnCollisionTrigger(GameObject triggerObject, GameObject other)
    {
        // 충돌한 대상이 "Player" 태그를 가지고 있는지 확인
        if (!other.CompareTag("Player")) return;

        // 트리거 오브젝트가 일치하는 모든 스폰 데이터를 검사
        foreach (var data in spawnSettings)
        {
            // 충돌 조건 + 트리거 오브젝트가 일치 + 아직 생성되지 않은 경우
            if (!data.hasSpawned &&
                data.triggerType == TriggerType.CollisionTrigger &&
                data.triggerColliderObject == triggerObject)
            {
                SpawnObject(data); // 해당 오브젝트 생성
            }
        }
    }

    // 오브젝트 생성 (활성화 처리)
    void SpawnObject(SpawnData data)
    {
        if (data.targetObject != null)
        {
            data.targetObject.SetActive(true);  // 오브젝트 활성화
            data.hasSpawned = true;             // 중복 생성을 막기 위해 상태 저장
        }
    }
}

// 트리거 조건 타입 정의 - 시간 기반 또는 충돌 기반
public enum TriggerType
{
    TimeTrigger,      // 시간 트리거: 일정 시간 경과 후 오브젝트 생성
    CollisionTrigger  // 충돌 트리거: 특정 오브젝트(Player)가 닿았을 때 생성
}
