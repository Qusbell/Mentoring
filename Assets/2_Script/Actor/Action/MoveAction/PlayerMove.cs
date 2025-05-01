using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MoveAction
{
    // 앞쪽 거리 판단
    protected float frontRayDistance;

    // <- 레이어 마스크


    protected override void Awake()
    {
        base.Awake();
        // 전방 주시 거리
        frontRayDistance = transform.localScale.z * 0.6f;
    }


    // 전방 확인
    protected virtual bool CanMove()
    { return Physics.Raycast(transform.position, moveVec, frontRayDistance); }

    // 회전
    // 진행 방향을 바라봄
    // 공격 방향을 바라봐야 할까?
    public virtual void Turn()
    { transform.LookAt(transform.position + moveVec); }


    // 이동
    public override void Move()
    {
        // 이동 방향이 없다면 : 업데이트 X
        if (moveVec == Vector3.zero) { return; }

        // 회전
        Turn();

        // 레이캐스트를 쏘고, 앞에 뭐가 없으면 이동
        if (CanMove()) { return; }

        // 이동
        base.Move();
    }
}
