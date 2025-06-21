using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Monster : Actor
{
    // ���� ���� ���� �ൿ
    protected Action actionStatus;

    // <- ���� �ִϸ��̼� ��� �޼���


    protected override void Awake()
    {
        base.Awake();
        // <- actionStatus = Spawn (���� �ִϸ��̼� �޼��� ����)
    }


    private void Update()
    {
        actionStatus();
    }

    // �̵� ����
    private void MoveStatus()
    {
        moveAction.Move();
        if (!moveAction.isMove &&
            attackAction.isCanAttack)
        { actionStatus = AttackStatus; }
    }

    // ���� ����
    private void AttackStatus()
    {
        attackAction.Attack();
        if (attackAction.isCanAttack) // ���� ��Ÿ�� ���ĺ��� �ٽ� �̵� ���� (�ӽ� ��ġ)
        { actionStatus = MoveStatus; }
    }
}