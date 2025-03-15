using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float hAxis;  // ��� �¿��̵� �Է�
    float vAxis;  // ��� �յ��̵� �Է�

    bool isJumpKeyDown;  // ���� �Է� ����


    // ���� ����ĳ��Ʈ
    // �� ���� ������ ����
    float raySpacing;
    // ���� ��, �����ߴ��� �Ÿ� �Ǵ�
    float rayDistance;


    // ����ȿ��
    Rigidbody rigid;

    // �̵��� ����(����ȭ ����)
    Vector3 direction;

    // 1�ʴ� �̵��� ĭ �� (WASD)
    public float speed;
    // ���� ����
    public float jumpHeight;


    void Awake()
    {
        // Rigidbody �ʱ�ȭ
        rigid = GetComponent<Rigidbody>();

        // ���� ������ ����
        // 0.4�� �� �γ��� ����
        // 0.3�� �� ������ ����
        raySpacing = (transform.localScale.x + transform.localScale.z) * 0.4f;
        // ���� Ȯ�� ����
        // y ������ 0.5
        rayDistance = transform.localScale.y * 0.5f;
    }


    // Update is called once per frame
    void Update()
    {
        // �Է�
        SetInput();
        // ����ȭ��(��� �������� ũ�Ⱑ 1��) ���⺤�� ����
        SetDirection();

        // �Էµ� ��������
        // ������Ʈ ������/ȸ��/����
        Move();
        Turn();
        Jump();
    }


    // �̵� ���� �Է�
    void InputWASD()
    {
        // �Է�(WASD, �����)���� ���� ����
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
    }

    // ���� ���� �Է�
    void InputJump()
    { isJumpKeyDown = Input.GetButtonDown("Jump"); }
    
    
    // ���� �Է� ����
    void SetInput()
    {
        InputWASD();  // WASD ���� �Է�
        InputJump();  // ���� �Է�
    }


    // ���� ����
    // ����ȭ�� ����
    void SetDirection()
    { direction = new Vector3(hAxis, 0, vAxis).normalized; }


    // �̵�
    // ��ġ += ���� * ���ǵ�
    void Move()
    {
        // transform: �ش� ���� ������Ʈ
        // .position: ���� ������Ʈ�� ��ġ
        // Time.deltaTime: �ϰ��� ������ ����
        transform.position
            += direction       // ����
            * speed            // �̵� ����
            * Time.deltaTime;  // �ð��� �ϰ��� �̵�
    }

    // ȸ��
    // ���� ������ �ٶ�
    void Turn()
    { transform.LookAt(transform.position + direction); }

    // ����
    // ��ġ += ���� ���� * ��������
    // ���� ���� (����ȿ��)
    void Jump()
    {
        // ������ �Է��ߴٸ� && ���� ���¶��
        if (isJumpKeyDown && IsGrounded())
        {
            // �߷°��ӵ� �ʱ�ȭ
            rigid.velocity = Vector3.zero;
            // ���� �������� jumpHeight��ŭ ���� ����
            rigid.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }

    }


    bool IsGrounded()
    {
        // ������Ʈ ��ġ (���)
        Vector3 origin = transform.position;

        // ��/���/�� ����ĳ��Ʈ
        return
            // ���� ����ĳ��Ʈ
            Physics.Raycast(origin + (transform.forward * raySpacing),
            Vector3.down,
            rayDistance)||

            // �߰� ����ĳ��Ʈ
            Physics.Raycast(origin,
            Vector3.down,
            rayDistance) ||

            // ���� ����ĳ��Ʈ
            Physics.Raycast(origin - (transform.forward * raySpacing),
            Vector3.down,
            rayDistance);
    }

}