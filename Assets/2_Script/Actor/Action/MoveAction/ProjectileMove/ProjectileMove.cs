using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MoveAction
{
    // ����ü ���� �ð�
    [SerializeField] protected float projectileTimer = 10f;


    // �ʱ�ȭ
    protected override void Awake()
    {
        base.Awake();
        // �߷� ���� ����
        rigid.useGravity = false;
    }

    // ��ǥ ��ġ�� �Է¹޴� �޼���
    public void SetTarget(Vector3 targetPos)
    {
        // ���� ���� ��� (����ȭ)
        Vector3 direction = (targetPos - transform.position).normalized;
        moveVec = direction;
        isMove = true;

        // Ÿ�̸� �� �ش� ����ü ����
        StartCoroutine(Timer.StartTimer(projectileTimer, () => Destroy(this.gameObject)));
    }

    // �� ������ �̵�
    private void Update()
    {
        if (isMove) { Move(); }
    }
}
