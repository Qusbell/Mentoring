using System.Collections;   
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(MonsterChaseAction))]
[RequireComponent(typeof(NavMeshAgent))]
abstract public class Monster : Actor
{
    // �׺���̼� ai
    protected NavMeshAgent nav;


    protected override void Awake()
    {
        base.Awake();
        nav = GetComponent<NavMeshAgent>();
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