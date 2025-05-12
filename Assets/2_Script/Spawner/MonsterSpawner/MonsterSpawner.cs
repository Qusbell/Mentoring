using UnityEngine;


public class MonsterSpawner : Spawner
{
    [Header("���� ���� ��ġ ����")]
    [SerializeField] private float detectionDistance = 10f; // �Ʒ� �������� �˻��� �Ÿ�
    // [SerializeField] private LayerMask groundLayer; // ���� ���̾� (���߿� �߰�)
    [SerializeField] private float heightOffset = 1.0f; // ���͸� �󸶳� ���� ������ų ���ΰ�

    // ������ ���� ����
    protected int monsterNum;


    protected override void Awake()
    {
        base.Awake();
        // ������ ���� ����
        monsterNum = targetPrefabs.Count - 1;
    }


    // ������Ʈ
    protected override void Update()
    {
        // �Ʒ� ���� ����
        SetSpawnLocation();

        // SpawnTriggerOn(); // ����׿� �ӽ�

        // ���� üũ
        base.Update();
    }


    // ���� ��ġ ����
    public override void SetSpawnLocation()
    {
        // ����ĳ��Ʈ�� �Ʒ� ���� ����
        RaycastHit hit;

        // ���߿� ���̾� �߰� ��: Physics.Raycast(spawnLocation.position, Vector3.down, out hit, detectionDistance, groundLayer)
        if (Physics.Raycast(spawnLocation, Vector3.down, out hit, detectionDistance))
        {
            // ���� ��ġ�� ���� ���� ����
            spawnLocation = new Vector3(
                hit.point.x,
                hit.point.y + heightOffset,
                hit.point.z
            );
        }
        else
        { base.SetSpawnLocation(); }
    }


    // ���� Ȯ��
    public override void CheckCompleted()
    {
        // ��� �������� �����ߴٸ�
        if (monsterNum <= PrefabIndex++)
        {
            // ���� üũ
            base.CheckCompleted();

            // ������ ��Ȱ��ȭ
            gameObject.SetActive(false);
        }
    }


    // �߰� ���: �����Ϳ��� ����ĳ��Ʈ �ð�ȭ (������)
    private void OnDrawGizmos()
    {
        if (spawnLocation != null)
        {
            // ����ĳ��Ʈ ���
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, spawnLocation + Vector3.down * detectionDistance);

            // ���� ��ġ
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnLocation, 0.5f);

            // ������ ��ü�� ��ġ
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }


}