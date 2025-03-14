using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float hAxis; // ��� �¿��̵� �Է�
    float vAxis; // ��� �յ��̵� �Է�

    // �̵��� ����(����ȭ ����)
    Vector3 direction;

    // 1�ʴ� �̵��� ĭ ��
    public float speed;

    // Update is called once per frame
    void Update()
    {
        // �̵� ���� ����
        InputWASD();
        // ����ȭ��(��� �������� ũ�Ⱑ 1��) ���⺤��
        direction = GetDirection();

        // ������Ʈ ������
        MoveTransform();
    }



    // �̵� ���� �Է�
    void InputWASD()
    {
        // �Է�(WASD, �����)���� ���� ����
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
    }

    // ���� ����
    Vector3 GetDirection()
    { return new Vector3(hAxis, 0, vAxis).normalized; }



    // ��ġ += ���� * ���ǵ�
    void MoveToDirection()
    {
        // transform: �ش� ���� ������Ʈ
        // .position: ���� ������Ʈ�� ��ġ
        // Time.deltaTime: �ϰ��� ������ ����
        transform.position
            += direction       // ����
            * speed            // �̵� ����
            * Time.deltaTime;  // �ð��� �ϰ��� �̵�
    }

    // ���� ������ �ٶ�
    void LookAtDirection()
    {
        // ȸ��
        // ���� ������ �ٶ�
        transform.LookAt(transform.position + direction);
    }

    // transform ��� �̵�/ȸ��
    void MoveTransform()
    {
        // ������ ���ؼ� �̵�
        MoveToDirection();
        // ������ ���ؼ� ȸ��
        LookAtDirection();
    }
}