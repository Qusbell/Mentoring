using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;






[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterMove : MoveAction
{
    // 추적할 대상
    [SerializeField] protected Transform target;

    // 네비게이션 AI
    NavMeshAgent nav;


    // 생성
    protected override void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();

        // <- 이동속도 등 초기화
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
            // 목표 탐색
            nav.SetDestination(target.position);
        }
    }
}
