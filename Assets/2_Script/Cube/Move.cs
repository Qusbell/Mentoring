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
    public float raycastDistance = 0.5f; // �浹 ���� �Ÿ�

    private bool isMoving = false; // ���� �̵� ������ ����

    void OnEnable()
    {
        isMoving = true; // ������Ʈ Ȱ��ȭ �� �̵� ����
    }

    void Update()
    {
        if (!isMoving) return;  // �̵� ���� �ƴϸ� ����ĳ��Ʈ�� �������� ����

        // ����ĳ��Ʈ�� �̵� �������θ� ����
        if (Physics.Raycast(transform.position, moveDirection.normalized, out RaycastHit hit, raycastDistance))
        {
            if (hit.collider.CompareTag("CollidedCube"))
            {
                isMoving = false; // ����
                gameObject.tag = "CollidedCube"; // �ڽ��� �±׵� ����
                return;
            }
        }

        // �浹 ������ ��� �̵�
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    }

    // ����׿�: ������ ���� ������ ������
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // ����ĳ��Ʈ�� �̵� �������θ� ����
        Gizmos.DrawLine(transform.position, transform.position + moveDirection.normalized * raycastDistance);
    }
}
