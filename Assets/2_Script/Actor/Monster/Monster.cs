using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(ChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Monster : Actor
{
    // �׺���̼� ai
    protected NavMeshAgent nav;

    // Ÿ�� ��ġ
    protected Transform target;


    protected override void Awake()
    {
        base.Awake();
        nav = GetComponent<NavMeshAgent>();

        // <- Ÿ�� (�÷��̾�) ����
    }


    protected virtual void Update()
    {
        moveAction.Move();

        // <- target�� �������� �ʴ� ����, ���� �����ϴ� ����
        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance) // <- �ִϸ��̼� ���� ���� X
        {
            attackAction.Attack();
            // <- �ִϸ��̼� ����
        }
    }
}