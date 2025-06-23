using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MoveAction
{
    // <- ���̾� ����ũ

    protected override void Awake()
    {
        base.Awake();
        // ���� �ֽ� �Ÿ�
        frontRayDistance = transform.localScale.z * 0.6f;
    }



    // ���� ����ĳ��Ʈ
    RaycastHit frontRayHit;

    // ���� �Ÿ� �Ǵ�
    protected float frontRayDistance;

    // ���� Ȯ�� - �̵� �����ϸ� true ��ȯ
    protected virtual bool CanMove()
    {
        // <- ���̾��ũ : ť��
        if (Physics.Raycast(transform.position, moveVec, out frontRayHit, frontRayDistance))
        {
            // Ʈ���� �ݶ��̴��� ���� (��� ����)
            if (frontRayHit.collider.isTrigger) { return true; }
            // �Ϲ� �ݶ��̴��� ��� �Ұ�
            else { return false; }
        }

        return true; // �ƹ��͵� �������� ������ �̵� ����
    }


    // �̵�
    public override void Move()
    {
        // �̵� ������ ���ٸ� : ������Ʈ X
        if (moveVec == Vector3.zero) { isMove = false; return; }

        // ȸ��
        Turn();

        // �̵� �Ұ����ϸ� ����
        isMove = CanMove();
        if (!isMove) { return; }

        // �̵�
        base.Move();
    }

}