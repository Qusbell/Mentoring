using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("�̵� ���� ����")]
    public Vector3 moveDirection = Vector3.right; // �⺻ �̵� ����

    [Header("�̵� �ӵ� ����")]
    public float moveSpeed = 2f; // �̵� �ӵ�

    [Header("����ĳ��Ʈ �Ÿ� ����")]
    public float raycastDistance = 0.4f; // �浹 ���� �Ÿ�

    private bool isMoving = false; // ���� �̵� ������ ����

    // ����ĳ��Ʈ ���� �迭 (6����: ��, ��, ��, ��, ��, �Ʒ�)
    private readonly Vector3[] directions = new Vector3[]
    {
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right,
        Vector3.up,
        Vector3.down
    };

    void OnEnable()
    {
        isMoving = true; // ������Ʈ Ȱ��ȭ �� �̵� ����
    }

    void Update()
    {
        if (!isMoving) return;

        // ��� �������� Ray�� ���� �浹 �˻�
        foreach (var dir in directions)
        {
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, raycastDistance))
            {
                if (hit.collider.CompareTag("CollidedCube"))
                {
                    isMoving = false; // ����
                    gameObject.tag = "CollidedCube"; // �ڽ��� �±׵� ����
                    return;
                }
            }
        }

        // �浹 ������ ��� �̵�
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    // ����׿�: ������ ���� ������ ������
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (var dir in directions)
        {
            Gizmos.DrawLine(transform.position, transform.position + dir * raycastDistance);
        }
    }
}
