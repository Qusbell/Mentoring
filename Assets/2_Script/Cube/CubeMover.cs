using System.Collections;
using UnityEngine;

// ť�긦 ������ �������� �̵���Ŵ
public class CubeMover : MonoBehaviour
{
    private Vector3 direction;      // �̵��� ����
    private float speed;            // �̵� �ӵ�
    private bool isMoving = false;  // ���� �̵� ������ ����

    private Vector3 startPosition;  // ó�� ������ ��ġ �����
    public float destroyDistance = 20f; // �� �Ÿ� �̻� �̵��ϸ� �ı�

    // ť�� �̵��� �����ϴ� �Լ�
    public void SetMovement(Vector3 dir, float spd)
    {
        direction = dir.normalized;     // ���� ���͸� ����ȭ (�ӵ� �ϰ��� ����)
        speed = spd;                    // �̵� �ӵ� ����
        isMoving = true;                // �̵� ����
        startPosition = transform.position; // ���� ��ġ ����
    }

    void Update()
    {
        if (isMoving)
        {
            // ������ �������� �����Ӵ� �̵�
            transform.Translate(direction * speed * Time.deltaTime);

            // �ʹ� �ָ� �̵��ϸ� �ڵ� �ı� (����ȭ ����)
            if (Vector3.Distance(startPosition, transform.position) > destroyDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // �浹�� ������Ʈ�� �±׸� �����ɴϴ�
        string tag = collision.gameObject.tag;

        // �浹 �� �̵��� ���߰� �� ����� �±� ���
        if (tag == "Cube" || tag == "Wall" || tag == "Obstacle")
        {
            // ������ �±� �� �ϳ��� �浹�� ���, �̵��� ����ϴ�
            isMoving = false;
        }
    }
}
