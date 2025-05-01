using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MoveAction
{
    // ���� �Ÿ� �Ǵ�
    protected float frontRayDistance;

    // <- ���̾� ����ũ


    protected override void Awake()
    {
        base.Awake();
        // ���� �ֽ� �Ÿ�
        frontRayDistance = transform.localScale.z * 0.6f;
    }


    // ���� Ȯ��
    protected virtual bool CanMove()
    { return Physics.Raycast(transform.position, moveVec, frontRayDistance); }

    // ȸ��
    // ���� ������ �ٶ�
    // ���� ������ �ٶ���� �ұ�?
    public virtual void Turn()
    { transform.LookAt(transform.position + moveVec); }


    // �̵�
    public override void Move()
    {
        // �̵� ������ ���ٸ� : ������Ʈ X
        if (moveVec == Vector3.zero) { return; }

        // ȸ��
        Turn();

        // ����ĳ��Ʈ�� ���, �տ� ���� ������ �̵�
        if (CanMove()) { return; }

        // �̵�
        base.Move();
    }
}
