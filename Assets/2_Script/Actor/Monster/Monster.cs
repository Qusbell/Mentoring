using System.Collections;   
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class Monster : Actor
{
    // 생성
    protected override void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();
        // null 초기화 방어
        if (rigid == null)
        {
            Debug.LogError("Rigidbody 컴포넌트 누락!", gameObject);
            enabled = false; // 생성 취소
        }

        //  base.Start(); // <- 아직 애니메이션 비존재


        // <- 이동속도 등 초기화
        nav = GetComponent<NavMeshAgent>();
        if (nav == null)
        {
            Debug.LogError("NavMeshAgent 컴포넌트 누락!", gameObject);
            enabled = false; // 생성 취소
        }
    }


    private void Update()
    {
        // 목표 탐색
        nav.SetDestination(target.position);
    }

    // 불필요한 물리 제거
    protected override void FreezeVelocity()
    {
        base.FreezeVelocity();
        rigid.velocity = Vector3.zero;
    }



    #region 추적 AI 메서드

    // 추적할 대상
    [SerializeField] protected Transform target;

    // 네비게이션 AI
    NavMeshAgent nav;

    #endregion
}
