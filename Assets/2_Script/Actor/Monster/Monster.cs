using System.Collections;   
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Monster : Actor
{
    // 생성
    protected override void Awake()
    {
        base.Awake();
        // <- 이동속도 등 초기화
        nav = GetComponent<NavMeshAgent>();
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
