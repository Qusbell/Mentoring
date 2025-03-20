using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;


public class Cube : MonoBehaviour
{
    // �����̴� ť���ΰ�
    [SerializeField]
    bool isMoving = false;

    // ��� �������� ������ ���ΰ�
    [SerializeField]
    Vector3 moveVec;

    // �̵� �ӵ�
    [SerializeField]
    float moveSpeed;



    void FixedUpdate()
    {
        Move();
    }


    // �̵�
    // ��ġ += ���� * ���ǵ�
    void Move()
    {
        if (isMoving)
        {
            // ���� ��ġ += ���� * �̵� ���� * �̵� ���� ����
           transform.position += moveVec * moveSpeed * Time.fixedDeltaTime;
        }
    }



    // �ݶ��̴��� �����Ǹ�
    // Cube <-> Cube: ����
    // Cube <-> �÷��̾� �� ����: ������
    // ������ ���о��� �׳� ������ ����
    void OnCollisionEnter(Collision collision)
    {
        // ������ ����
        isMoving = false;
    }
}
