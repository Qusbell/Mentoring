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


    protected override void Awake()
    {
        base.Awake();
        actionStatus = SpawnState; // �������� ����
    }

    private void Start()
    { target = TargetManager.instance.Targeting(); }

    private void Update()
    { actionStatus(); }

    // ���� ���� ���� ���
    // <- ���а�� �ʿ� X. ���� ���� >= ���� �� ���·� ����ȭ ����
    bool InAttackRange()
    { return attackAction.attackRange >= Vector3.Distance(target.position, this.transform.position); }



    // ���� ����
    private void SpawnState()
    {
        if (!animatior.CheckAnimationName("Spawn")) // ���� �ִϸ��̼� ���� ��
        { actionStatus = IdleStatus; } // ���
    }

    // ��� ����
    private void IdleStatus()
    {
        if (InAttackRange())  // ���� ���� ���¶��
        { actionStatus = AttackStatus; } // ��������
        else
        { actionStatus = MoveStatus; }  // �ƴϸ� �̵�
    }


    // �̵� ����
    private void MoveStatus()
    {
        if (InAttackRange())
        {
            animatior.isMove = false;
            moveAction.isMove = false;
            actionStatus = AttackStatus;
        }
        else
        {
            animatior.isMove = true;
            moveAction.isMove = true;
            moveAction.Move();
        }
    }


    // ����, ���ε� ���� Ȯ��
    bool doAttack = false;
    bool doReload = false;

    // ���� ����
    private void AttackStatus()
    {
        // ���� �����ϴٸ�
        if (attackAction.isCanAttack)
        {
            attackAction.Attack();
            animatior.isAttack = true; // ���� �ִϸ��̼� ���
        }


        if (animatior.CheckAnimationName("Attack"))
        { doAttack = true; }
        else if (doAttack)
        {
            doAttack = false;
            Debug.Log("Attack ��� �� �ƴ�");
            actionStatus = ReloadStatus; // ���� �ĵ����̷� ���� }
        }
    }


    // ���� �ĵ����� �ִϸ��̼� ���
    private void ReloadStatus()
    {
        if (animatior.CheckAnimationName("Reload"))
        { doReload = true; }
        else if (doReload)
        {
            doReload = false;
            Debug.Log("Reload ��� �� �ƴ�");
            actionStatus = IdleStatus;
        }
    }
}