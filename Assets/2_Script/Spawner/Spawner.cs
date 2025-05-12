using System;
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
    private bool spawnTrigger = false;     // ���� ���� ���� ���� (�̰� true�� ����� == ����)
    protected bool isCompleted = false;    // ������ �Ϸ� ���� Ȯ��

    // ������ �������� �ε��� (�� ��° ����������)
    private int _prefabIndex = 0;
    public int PrefabIndex
    {
        get { return _prefabIndex; }
        protected set
        {
            // null �˻� && �ε��� �˻�
            if (targetPrefabs != null && 0 <= value && value < targetPrefabs.Count)
            { _prefabIndex = value; }
        }
    }


    // ���� ��ġ �ӽ� �ʱ�ȭ
    protected virtual void Awake()
    { SetSpawnLocation(); }


    // ==================== ���� üũ ====================
    protected virtual void Update()
    {
        // -------------------- ���� üũ --------------------
        // ������  || ���� ������ �������� ������ ���� X
        if (isCompleted || !spawnTrigger) { return; }

        // -------------------- ���� --------------------
        SpawnObject(); // ������Ʈ ����

        // -------------------- �Ϸ� Ȯ�� --------------------
        CheckCompleted();
    }


    // ==================== ���� ���� ���� ���� ====================

    // ���� ���� ���� / Ʈ���� �ѱ�
    public virtual void SpawnTriggerOn()
    { spawnTrigger = true; }

    // ���� Ʈ���� ����
    public virtual void SpawnTriggerOFF()
    { spawnTrigger = false; }



    // ==================== ���� / ��ġ ���� ====================

    // ������Ʈ ����
    protected virtual void SpawnObject()
    {
        if (targetPrefabs.Count < 0)
        { Debug.Log("������ ������ �ε��� �������"); return; }

        // ���� �ε����� ������, ������ ��ġ, �⺻ ȸ�������� ����
        Instantiate(targetPrefabs[PrefabIndex], spawnLocation, Quaternion.identity);
    }

    // ���� ��ġ ����
    public virtual void SetSpawnLocation()
    { spawnLocation = transform.position; }



    // ==================== �Ϸ� / ��Ȱ�� ====================

    // ������ �Ϸ� ����
    public virtual void CheckCompleted()
    { isCompleted = true; }

    // ������ �ʱ�ȭ (��Ȱ��ȭ)
    public virtual void ResetSpawner()
    { isCompleted = false; }
}