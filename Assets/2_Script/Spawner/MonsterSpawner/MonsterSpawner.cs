using UnityEngine;


public class MonsterSpawner : Spawner
{
    [Header("몬스터 스폰 위치 설정")]
    [SerializeField] private float detectionDistance = 10f; // 아래 방향으로 검사할 거리
    // [SerializeField] private LayerMask groundLayer; // 지형 레이어 (나중에 추가)
    [SerializeField] private float heightOffset = 1.0f; // 몬스터를 얼마나 위에 스폰시킬 것인가

    // 생성할 몬스터 숫자
    protected int monsterNum;


    protected override void Awake()
    {
        base.Awake();
        // 생성할 몬스터 숫자
        monsterNum = targetPrefabs.Count - 1;
    }


    // 업데이트
    protected override void Update()
    {
        // 아래 지형 감지
        SetSpawnLocation();

        // SpawnTriggerOn(); // 디버그용 임시

        // 생성 체크
        base.Update();
    }


    // 스폰 위치 지정
    public override void SetSpawnLocation()
    {
        // 레이캐스트로 아래 지형 감지
        RaycastHit hit;

        // 나중에 레이어 추가 시: Physics.Raycast(spawnLocation.position, Vector3.down, out hit, detectionDistance, groundLayer)
        if (Physics.Raycast(spawnLocation, Vector3.down, out hit, detectionDistance))
        {
            // 스폰 위치를 지형 위로 변경
            spawnLocation = new Vector3(
                hit.point.x,
                hit.point.y + heightOffset,
                hit.point.z
            );
        }
        else
        { base.SetSpawnLocation(); }
    }


    // 종료 확인
    public override void CheckCompleted()
    {
        // 모든 프리펩을 생성했다면
        if (monsterNum <= PrefabIndex++)
        {
            // 종료 체크
            base.CheckCompleted();

            // 스포너 비활성화
            gameObject.SetActive(false);
        }
    }


    // 추가 기능: 에디터에서 레이캐스트 시각화 (디버깅용)
    private void OnDrawGizmos()
    {
        if (spawnLocation != null)
        {
            // 레이캐스트 경로
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, spawnLocation + Vector3.down * detectionDistance);

            // 스폰 위치
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnLocation, 0.5f);

            // 스포너 자체의 위치
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }


}