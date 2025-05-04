using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//==================================================
// �پ��� ���� ���ǰ� ����� ������ �� �ִ� �߻� Ŭ����
// �ڽ� Ŭ�������� SpawnTrigger()�� �������̵��Ͽ� ���� ���� ����
//==================================================
abstract public class Spawner : MonoBehaviour
{
    // ������ ������Ʈ (������ �����յ�� ��ġ ����)
    [SerializeField] protected List<GameObject> targetPrefabs = new List<GameObject>(); // ���������� ������ ������ ���
    [SerializeField] protected Vector3 spawnLocation;  // ������Ʈ�� ������ ��ġ(�� ������Ʈ ��) �ν����Ϳ��� �Ҵ�

    // ���� ���� ���� ����
    protected bool spawnTrigger = false;   // ���� ���� ���� ���� (�̰� true�� ����� == ����)
    protected int currentPrefabIndex = 0;  // ���� ������ �������� �ε��� (�� ��° ����������)
    protected bool isCompleted = false;    // ��� �������� ���� �Ϸ�Ǿ����� ���� (true�� �� �̻� ���� �� ��)


    // ���� ��ġ �ӽ� �ʱ�ȭ
    protected virtual void Awake()
    { SetSpawnLocation(); }


    protected virtual void Update()
    {
        // [1] ���� üũ
        // ��� ������ ������ �Ϸ�Ǿ��ų� || ���� ������ �������� ������ ���� X
        if (isCompleted || !spawnTrigger) { return; }

        // [2] ����
        SpawnObject();         // ���� �ε����� ������ ����
        currentPrefabIndex++;  // ���� ������ �ε����� �̵�
        spawnTrigger = false;  // 1�� ���� �� : �ٽ� ���

        // [3] �Ϸ� üũ
        if (currentPrefabIndex >= targetPrefabs.Count)
        { isCompleted = true; }  // ��� ������ ���� �Ϸ� (�� �̻� ���� �� ��)
    }


    // ���� ���� ���� �� ȣ��
    public virtual void SpawnTriggerOn()
    { spawnTrigger = true; }


    // ������Ʈ ����
    protected virtual void SpawnObject()
    {
        // ����Ʈ�� ������� �ʰ�, ���� �ε����� ��ȿ�� �������� Ȯ��
        if (targetPrefabs.Count > 0 && currentPrefabIndex < targetPrefabs.Count)
        // ���� �ε����� ������, ������ ��ġ, �⺻ ȸ�������� ����
        { Instantiate(targetPrefabs[currentPrefabIndex], spawnLocation, Quaternion.identity); }
    }


    // ������ �ʱ�ȭ - �ܺο��� ȣ���Ͽ� ������ ����
    public virtual void ResetSpawner()
    {
        currentPrefabIndex = 0;    // �ε��� �ʱ�ȭ (ù ��° �����պ��� �ٽ� ����)
        isCompleted = false;       // �Ϸ� ���� �ʱ�ȭ (�ٽ� ���� �����ϰ� ����)
    }


    // ���� ��ġ ����
    public virtual void SetSpawnLocation()
    { spawnLocation = transform.position; }
}