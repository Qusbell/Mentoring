using System.Collections;   
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : Actor
{
    // ����
    protected override void Awake()
    {
        base.Awake();
        // <- �̵��ӵ� �� �ʱ�ȭ
        nav = GetComponent<NavMeshAgent>();
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
