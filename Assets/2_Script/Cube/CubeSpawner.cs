using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ť�긦 �ڵ����� �������ִ� ������ ��
public class CubeSpawner : MonoBehaviour
{
    [Header("ť�� ���� ����")]
    public GameObject[] cubePrefabs;         // ������ ť�� �����յ� (���� ���� �� �ϳ� ������ ���� ����)

    [Header("ť�� ���� ��ġ")]
    public Transform[] spawnPoints;          // ť�갡 ������ ��ġ��

    [Header("ť�� �̵� ����")]
    public Vector3 moveDirection = Vector3.right; // ť�갡 �̵��� ����
    public float moveSpeed = 2f;                  // ť���� �̵� �ӵ�

    [Header("�ڵ� ���� - �ð� ����")]
    public bool useTimeTrigger = true;       // �ð� �������� �ڵ� �������� ����
    public float spawnInterval = 2f;         // ���� �ð� ���� (��)

    [Header("�ڵ� ���� - �÷��̾� ��ġ ����")]
    public bool usePlayerTrigger = false;    // �÷��̾� ��ġ ���� �� �������� ����
    public Transform player;                 // �÷��̾� ������Ʈ
    public Vector3 triggerPosition;          // �÷��̾ �����ؾ� �����Ǵ� ��ġ

    private float timer = 0f;                // �ð� ������ ���� ����
    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>(); // �̹� ť�갡 �ִ� ��ġ ��Ͽ�

    void Update()
    {
        // �ð� �������� ť�� ����
        if (useTimeTrigger)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnCube();
                timer = 0f;
            }
        }

        // �÷��̾� ��ġ �������� ť�� ����
        if (usePlayerTrigger && player != null)
        {
            if (Vector3.Distance(player.position, triggerPosition) < 0.1f)
            {
                SpawnCube();
            }
        }
    }

    // ť�긦 �����ϴ� �Լ�
    void SpawnCube()
    {
        GameObject prefab = cubePrefabs[Random.Range(0, cubePrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPosition = spawnPoint.position;

        // �̹� ť�갡 �ִ� ��ġ�� �������� ����
        if (occupiedPositions.Contains(spawnPosition))
            return;

        // ť�� ����
        GameObject cube = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // ť�� ũ�⸦ 1��1��1�� ����
        cube.transform.localScale = Vector3.one;

        // ��ġ ���
        occupiedPositions.Add(spawnPosition);

        // ť�꿡 �̵� ��� �ο�
        CubeMover mover = cube.GetComponent<CubeMover>();
        if (mover != null)
        {
            mover.SetMovement(moveDirection, moveSpeed);
        }
    }
}
