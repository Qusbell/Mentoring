using UnityEngine;
using System.Collections;


public class MonsterSpawner : Spawner
{
    // �ʱ�ȭ
    protected void Start()
    {
        targetCollider = GetComponent<Collider>();

        if (targetCollider == null)
        {
            Debug.LogError("Collider component missing!");
            return;
        }
    }


    // ===== ���� ��ġ =====

    // ���� ������Ʈ�� �ݶ��̴�
    protected Collider targetCollider;

    // ���� �߾� ���
    public override void SetSpawnLocation()
    {
        Bounds bounds = targetCollider.bounds;
        Vector3 topCenter = bounds.center + Vector3.up * bounds.extents.y;  // ���� �߾� ��ġ ���
        spawnLocation = topCenter;

        // ���� �Ĺ��� ���� �� �߻� ��, ���� �κ� ����
        //    // �ʿ��ϴٸ� �߰� ������ ���� (��: ��¦ ����)
        //    float heightOffset = 0.5f; // �ʿ信 ���� ����
        //    spawnLocation = topCenter + Vector3.up * heightOffset;
    }


    // ���� �ֱ�
    [SerializeField] protected float spawnRate = 2f;

    // 1. ������ Ȱ��ȭ
    // 2. ���� ��ġ ����
    // 3. ���� ����
    public override void SpawnTriggerOn()
    {
        base.SpawnTriggerOn();
        SetSpawnLocation();
        SpawnObject();
    }

    // ����
    protected override void SpawnObject()
    {
        base.SpawnObject();
        CheckCompleted();

        // ----- ���� üũ -----
        // �̿Ϸ� && ���� Ʈ���� On
        if (!isCompleted && spawnTrigger)
        { StartCoroutine(Timer.StartTimer(spawnRate, SpawnObject)); }
    }


    // ���� Ȯ��
    public override void CheckCompleted()
    {
        // ��� �������� �����ߴٸ�
        if (targetPrefabs.Count <= PrefabIndex++ + 1)
        {
            base.CheckCompleted();
            // <- �ֱ��� �����ʶ��: ���� �߻�
        }
    }
}