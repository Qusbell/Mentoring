using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//==================================================
// ���� / ����
//==================================================
[RequireComponent(typeof(Rigidbody))]
public class JumpAction : MonoBehaviour
{
    // ������Ʈ�� ���� ����ȿ��
    protected Rigidbody rigid;

    // ���� �� �ʱ�ȭ
    protected virtual void Awake()
    {
        // Rigidbody �ʱ�ȭ
        rigid = GetComponent<Rigidbody>();
        // null �ʱ�ȭ ���
        if (rigid == null)
        {
            Debug.LogError("Rigidbody ������Ʈ ����!", gameObject);
            enabled = false; // ���� ���
        }

        // �ٴ� ���� ������ ����
        // �ʹ� ������, �ٸ� ť��� ��ġ�� ��� 2�� ���� �� ���� �߻�
        // �ʹ� ������, ��Ʈ�Ӹ��� ������ �� ���� �Ұ����� ���� �߻�
        raySpacing = (transform.localScale.x + transform.localScale.z) * 0.22f;

        // ���� Ȯ��
        bottomRayDistance = transform.localScale.y * 1.05f;
    }


    //==================================================
    // ���� �޼���
    //==================================================

    // ���� ����
    [SerializeField] float jumpHeight = 13;

    // �����ߴ°��� ���� �Ÿ� ����
    float bottomRayDistance;

    // ���� ����ĳ��Ʈ (���� ����)
    // �� ���� ������ ����
    float raySpacing;


    // ����
    // ��ġ += ���� ���� * ��������
    // ���� ���� (����ȿ��)
    public virtual void Jump()
    {
        // ���� ���°� �ƴ϶��
        if (IsGrounded())
        {
            // ���ʿ��� ���� �ʱ�ȭ
            rigid.velocity = Vector3.zero;
            // ���� �������� jumpHeight��ŭ ���� ����
            rigid.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
    }


    // ���� �������� ����
    public bool IsGrounded()
    {
        // ��/�� ����ĳ��Ʈ
        return
            // ���� ����ĳ��Ʈ
            Physics.Raycast(transform.position + (transform.forward * raySpacing),
            Vector3.down,
            bottomRayDistance) ||

            // ���� ����ĳ��Ʈ
            Physics.Raycast(transform.position - (transform.forward * raySpacing),
            Vector3.down,
            bottomRayDistance);
    }

}
