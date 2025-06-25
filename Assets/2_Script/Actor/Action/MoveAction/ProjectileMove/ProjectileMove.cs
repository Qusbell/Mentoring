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
        // 각종 물리 제거
        rigid.useGravity = false;
        rigid.freezeRotation = true;
    }

    // 목표 위치를 입력받는 메서드
    public void SetTarget(Vector3 targetPos)
    {
        // 방향 벡터 계산 (정규화)
        moveVec = (targetPos - transform.position).normalized;
        isMove = true;

        // 타이머 후 해당 투사체 삭제
        StartCoroutine(Timer.StartTimer(projectileTimer, () => Destroy(this.gameObject)));

        Turn(); // <- 딱 1회, 해당 방향 바라봄
    }
}
