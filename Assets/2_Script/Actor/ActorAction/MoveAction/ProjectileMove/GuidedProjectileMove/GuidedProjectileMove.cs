using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


public class GuidedProjectileMove : ProjectileMove
{
    // 회전 속도
    [SerializeField] protected float turnSpeed = 3f;

    protected Transform target;

    // 목표 대상을 입력받는 메서드
    // 끝까지 추격
    public void SetTargetTransform(Transform p_target)
    {
        // 타이머 후 해당 투사체 삭제
        StartCoroutine(Timer.StartTimer(projectileTimer, () => Destroy(this.gameObject)));
        isMove = true;

        target = p_target;
    }


    public override void Move()
    {
        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, dir, turnSpeed * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

}
