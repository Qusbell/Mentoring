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
            Debug.LogError("�ݶ��̴� �������� ���� : " + gameObject.name);
            return;
        }

        // ���� ��ġ �̸� ����
        SetSpawnLocation();

        // �ڵ� ���� ���� - MonsterCube���� ȣ���� ������ ���
        Debug.Log($"[{gameObject.name}] MonsterSpawner �ʱ�ȭ �Ϸ�. �ܺ� ȣ�� ��� ��...");
    }
    // ===== ���� ��ġ =====
    // ���� ������Ʈ�� �ݶ��̴�
    protected Collider targetCollider;
    // ���� �߾� ���
    public override void SetSpawnLocation()
    {
        if (targetCollider == null) return;

        Bounds bounds = targetCollider.bounds;
        Vector3 topCenter = bounds.center + Vector3.up * bounds.extents.y;  // ���� �߾� ��ġ ���
        spawnLocation = topCenter;
        // ���� �Ĺ��� ���� �� �߻� ��, ���� �κ� ����
        //    // �ʿ��ϴٸ� �߰� ������ ���� (��: ��¦ ����)
        //    float heightOffset = 0.5f; // �ʿ信 ���� ����
        //    spawnLocation = topCenter + Vector3.up * heightOffset;
    }
    // ===== Ʈ���� / ���� / �Ϸ� =====
    // ���� �ֱ�
    [SerializeField] protected float spawnRate = 2f;
    // 1. ������ Ȱ��ȭ (MonsterCube���� ȣ��)
    // 2. ���� ��ġ ����
    // 3. ���� ����
    public override void SpawnTriggerOn()
    {
        Debug.Log($"[{gameObject.name}] MonsterSpawner Ȱ��ȭ��! ������ �����մϴ�.");

        base.SpawnTriggerOn();
        SetSpawnLocation(); // ���� ��ġ �缳��
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
        {
            StartCoroutine(Timer.StartTimer(spawnRate, SpawnObject));
        }
    }
    // ���� Ȯ��
    public override void CheckCompleted()
    {
        // ��� �������� �����ߴٸ�
        if (targetPrefabs.Count <= PrefabIndex + 1)
        {
            Debug.Log($"[{gameObject.name}] ���� ���� �Ϸ�");
            base.CheckCompleted();
            // <- �ֱ��� �����ʶ��: ���� �߻�
        }
    }
}