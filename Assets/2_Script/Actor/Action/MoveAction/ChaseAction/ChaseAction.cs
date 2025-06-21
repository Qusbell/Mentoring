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
    protected Transform target;

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

        // Ÿ�� ����
        target = TargetManager.instance.target;
    }


    // ===== �̵� =====

    // ��ǥ�� ���� �̵�
    public override void Move()
    {
        // target �������� ���� ����
        if (target == null)
        {
            isMove = false;
            nav.isStopped = true;
            return;
        }

        // ��ǥ�� ����
        nav.SetDestination(target.position);
        nav.isStopped = false;

        // �̵� ������ Ȯ��
        CheckMove();

        // ���ڸ� ������ �� ȸ��
        if (!isMove)
        { TurnToTarget(); }
    }

    // move ���� Ȯ��
    void CheckMove()
    {
        isMove = nav.hasPath &&
            !nav.pathPending &&
            nav.stoppingDistance < nav.remainingDistance;
    }


    // ȸ�� �ӵ�
    [SerializeField] protected float rotationSpeed = 3f;

    // ����� ���� ȸ��
    protected void TurnToTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}