using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class ChaseAction : MoveAction
{
    // ������ ���
    [SerializeField] protected Transform target;

    // ȸ�� �ӵ�
    [SerializeField] protected float rotationSpeed = 3f;

    // �׺���̼� AI
    protected NavMeshAgent nav;


    // ����
    protected override void Awake()
    {
        base.Awake();

        // �׺���̼� �ʱ�ȭ
        nav = GetComponent<NavMeshAgent>();
        // �̵��ӵ� ����
        nav.speed = moveSpeed;

        // <- ���� ��Ÿ� : AttackAction �ʿ��� ����
        //  nav.stoppingDistance = GetComponent<AttackAction>().attackRange;
    }


    // ��ǥ�� ���� �̵�
    public override void Move()
    {
        // target �������� ���� ����
        if (target == null) { nav.isStopped = true; return; }

        // ��ǥ�� ����
        nav.SetDestination(target.position);

        // ���ڸ� ������ �� ȸ��
        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
        {
            TurnToTarget();
        }
    }


    // ����� ���� ȸ��
    protected void TurnToTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}