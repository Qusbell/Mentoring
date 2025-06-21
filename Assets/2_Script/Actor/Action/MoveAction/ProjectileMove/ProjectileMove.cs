using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MoveAction
{
    // 투사체 유지 시간
    [SerializeField] protected float projectileTimer = 10f;


    // 초기화
    protected override void Awake()
    {
        base.Awake();
        // 중력 영향 제거
        rigid.useGravity = false;
    }

    // 목표 위치를 입력받는 메서드
    public void SetTarget(Vector3 targetPos)
    {
        // 방향 벡터 계산 (정규화)
        Vector3 direction = (targetPos - transform.position).normalized;
        moveVec = direction;
        isMove = true;

        // 타이머 후 해당 투사체 삭제
        StartCoroutine(Timer.StartTimer(projectileTimer, () => Destroy(this.gameObject)));
    }

    // 매 프레임 이동
    private void Update()
    {
        if (isMove) { Move(); }
    }
}
