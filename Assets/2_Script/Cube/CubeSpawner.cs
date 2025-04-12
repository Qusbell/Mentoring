using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 큐브를 자동으로 생성해주는 역할을 함
public class CubeSpawner : MonoBehaviour
{
    [Header("큐브 종류 설정")]
    public GameObject[] cubePrefabs;         // 생성할 큐브 프리팹들 (여러 종류 중 하나 무작위 선택 가능)

    [Header("큐브 생성 위치")]
    public Transform[] spawnPoints;          // 큐브가 생성될 위치들

    [Header("큐브 이동 설정")]
    public Vector3 moveDirection = Vector3.right; // 큐브가 이동할 방향
    public float moveSpeed = 2f;                  // 큐브의 이동 속도

    [Header("자동 생성 - 시간 조건")]
    public bool useTimeTrigger = true;       // 시간 간격으로 자동 생성할지 여부
    public float spawnInterval = 2f;         // 생성 시간 간격 (초)

    [Header("자동 생성 - 플레이어 위치 조건")]
    public bool usePlayerTrigger = false;    // 플레이어 위치 도달 시 생성할지 여부
    public Transform player;                 // 플레이어 오브젝트
    public Vector3 triggerPosition;          // 플레이어가 도달해야 생성되는 위치

    private float timer = 0f;                // 시간 측정을 위한 변수
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>(); // 이미 큐브가 있는 위치 기록용

    void Update()
    {
        // 시간 조건으로 큐브 생성
        if (useTimeTrigger)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnCube();
                timer = 0f;
            }
        }

        // 플레이어 위치 조건으로 큐브 생성
        if (usePlayerTrigger && player != null)
        {
            if (Vector3.Distance(player.position, triggerPosition) < 0.1f)
            {
                SpawnCube();
            }
        }
    }

    // 큐브를 생성하는 함수
    void SpawnCube()
    {
        GameObject prefab = cubePrefabs[Random.Range(0, cubePrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = spawnPoint.position;

        // 이미 큐브가 있는 위치면 생성하지 않음
        if (occupiedPositions.Contains(spawnPosition))
            return;

        // 큐브 생성
        GameObject cube = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // 큐브 크기를 1×1×1로 고정
        cube.transform.localScale = Vector3.one;

        // 위치 기록
        occupiedPositions.Add(spawnPosition);

        // 큐브에 이동 기능 부여
        CubeMover mover = cube.GetComponent<CubeMover>();
        if (mover != null)
        {
            mover.SetMovement(moveDirection, moveSpeed);
        }
    }
}
