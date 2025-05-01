using System.Collections;   
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Monster : Actor
{
    // ����
    protected override void Awake()
    {
        // Rigidbody �ʱ�ȭ
        rigid = GetComponent<Rigidbody>();
        // null �ʱ�ȭ ���
        if (rigid == null)
        {
            Debug.LogError("Rigidbody ������Ʈ ����!", gameObject);
            enabled = false; // ���� ���
        }

        //  base.Start(); // <- ���� �ִϸ��̼� ������


        // <- �̵��ӵ� �� �ʱ�ȭ
        nav = GetComponent<NavMeshAgent>();
        if (nav == null)
        {
            Debug.LogError("NavMeshAgent ������Ʈ ����!", gameObject);
            enabled = false; // ���� ���
        }
    }


    private void Update()
    {
        // ��ǥ Ž��
        nav.SetDestination(target.position);
    }

    // ���ʿ��� ���� ����
    protected override void FreezeVelocity()
    {
        base.FreezeVelocity();
        rigid.velocity = Vector3.zero;
    }



    #region ���� AI �޼���

    // ������ ���
    [SerializeField] protected Transform target;

    // �׺���̼� AI
    NavMeshAgent nav;

    #endregion
}
