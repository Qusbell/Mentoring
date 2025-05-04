using UnityEngine;

public class MonsterSpawner : Spawner
{
    [Header("���� ���� ��ġ ����")]
    [SerializeField] private float detectionDistance = 10f; // �Ʒ� �������� �˻��� �Ÿ�
    // [SerializeField] private LayerMask groundLayer; // ���� ���̾� (���߿� �߰�)
    [SerializeField] private float heightOffset = 1.0f; // ���͸� �󸶳� ���� ������ų ���ΰ�


    // ���� ��ġ �ʱ�ȭ
    protected override void Awake()
    {
        base.Awake();
        SetSpawnLocation();
    }


    // �ݶ��̴� Ʈ����
    private void OnTriggerEnter(Collider other)
    {
        // ����: �÷��̾ Ʈ���� ������ ������ ����
        if (other.CompareTag("Player") && !isCompleted)
        { SpawnTriggerOn(); }
    }


    // ������Ʈ
    protected override void Update()
    {
        // <- trigger ����

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


    // �߰� ���: �����Ϳ��� ����ĳ��Ʈ �ð�ȭ (������)
    private void OnDrawGizmos()
    {
        if (spawnLocation != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(spawnLocation, spawnLocation + Vector3.down * detectionDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnLocation, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}