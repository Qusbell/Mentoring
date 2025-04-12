using UnityEngine;

// Ʈ���� ������Ʈ�� �ٿ��� �浹�� �����ϰ�,
// ���� �Ŵ������� �����ϴ� ������ �ϴ� ��ũ��Ʈ
public class CollisionReporter : MonoBehaviour
{
    public spawn spawnerManager; // ����� ���� �Ŵ��� (�ʼ�)

    // �Ϲ� �浹 �߻� �� ȣ���
    private void OnCollisionEnter(Collision collision)
    {
        // ������ �Ŵ����� �����ϰ� Ȱ��ȭ�� ��쿡�� �浹 ó��
        if (spawnerManager != null && spawnerManager.isActiveAndEnabled)
        {
            // �浹�� ��� ������ ���� �Ŵ����� ����
            spawnerManager.OnCollisionTrigger(gameObject, collision.gameObject);
        }
    }

    // Ʈ���� �浹�� �߻����� �� ȣ��� (�ɼ�)
    private void OnTriggerEnter(Collider other)
    {
        // ������ �Ŵ����� �����ϰ� Ȱ��ȭ�� ��쿡�� �浹 ó��
        if (spawnerManager != null && spawnerManager.isActiveAndEnabled)
        {
            spawnerManager.OnCollisionTrigger(gameObject, other.gameObject);
        }
    }
}
