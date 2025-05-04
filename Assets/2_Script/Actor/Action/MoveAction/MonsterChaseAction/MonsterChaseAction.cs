using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;



// ���� �ൿ�� �����Ѵٰ� �����ϰ� �ۼ���
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AttackAction))]
public class MonsterChaseAction : MoveAction
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
        // Rigidbody �ʱ�ȭ
        rigid = GetComponent<Rigidbody>();

        // �׺���̼� �ʱ�ȭ
        nav = GetComponent<NavMeshAgent>();
        // �̵��ӵ� �� ���� ��Ÿ� ����
        nav.speed = moveSpeed;
        nav.stoppingDistance = GetComponent<AttackAction>().attackRange;
    }


    // ��ǥ�� ���� �̵�
    public override void Move()
    {
        // target �������� ���� ����
        if (target == null)
        { nav.isStopped = true; return; }

        // ��ǥ�� ����
        nav.SetDestination(target.position);


        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
        {
            TurnToTarget();
            // <- �ٸ� ������Ʈ�� �˸� �޼���
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