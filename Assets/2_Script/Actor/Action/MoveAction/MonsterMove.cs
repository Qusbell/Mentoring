using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;






[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterMove : MoveAction
{
    // ������ ���
    [SerializeField] protected Transform target;

    // �׺���̼� AI
    NavMeshAgent nav;


    // ����
    protected override void Awake()
    {
        // Rigidbody �ʱ�ȭ
        rigid = GetComponent<Rigidbody>();

        // <- �̵��ӵ� �� �ʱ�ȭ
        nav = GetComponent<NavMeshAgent>();
    }


    protected virtual void Update()
    {
        Move();
    }


    private void FixedUpdate()
    {
        rigid.angularVelocity = Vector3.zero;
        rigid.velocity = Vector3.zero;
    }



    public override void Move()
    {
        if (target != null)
        {
            // ��ǥ Ž��
            nav.SetDestination(target.position);
        }
    }
}
