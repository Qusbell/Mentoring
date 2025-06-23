using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Monster : Actor
{
    // ���� ���� ���� �ൿ
    protected Action actionStatus;

    // Ÿ��
    Transform target;

    // <- ���� �ִϸ��̼� ��� �޼���
    // �ִϸ��̼� ����� ���� ��, MoveStatus�� ��ȯ


    protected override void Awake()
    {
        base.Awake();
        actionStatus = MoveStatus;
        // <- actionStatus = SpawnStatus (���� �ִϸ��̼� �޼��� ����, �ӽ÷� MoveStatus ���� ��)
    }

    private void Start()
    { target = TargetManager.instance.Targeting(); }

    private void Update()
    { actionStatus(); }


    // ���� ���� ���ζ��
    // <- ���а�� �ʿ� X. ���� ���� >= ���� �� ���·� ����ȭ ����
    bool InAttackRange()
    { return attackAction.attackRange >= Vector3.Distance(target.position, this.transform.position); }


    // �̵� ����
    private void MoveStatus()
    {
        moveAction.Move();
        if (InAttackRange() && // <- InAttackRange �������� �ٲٱ�
            attackAction.isCanAttack)
        { actionStatus = AttackStatus; }
    }

    // ���� ����
    private void AttackStatus()
    {
        // ���� ���� ���� �ִٸ�
        if (InAttackRange())
        {
            attackAction.Attack(); // ����
            // <- ���� �ĵ����� ���·� ��ȯ
        }

        // ���� ���� ���� ����
        // <- attackAfterDelay �� �߰� ����
        else if (attackAction.isCanAttack) // ���� ��Ÿ�� ���ĺ��� �ٽ� �̵� ���� (�ӽ� ��ġ)
        { actionStatus = MoveStatus; } // <- �ٽ� �̵�
    }



    // <- SpawnStatus

    // <- IdleStatus

    // <- ReloadStatus
}