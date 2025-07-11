using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ProjectileHitAttack))]
public class Projectile : Actor
{
    // 투사체 유지 시간
    [SerializeField] protected float projectileTimer = 10f;

    private void Start()
    {
        // 타이머 후 해당 투사체 삭제
        // StartCoroutine(Timer.StartTimer(projectileTimer, () => Destroy(this.gameObject)));
        Timer.Instance.StartTimer(projectileTimer, () => Destroy(this.gameObject));
    }


    // 매 프레임 이동
    protected virtual void Update()
    {
        if (moveAction.isMove) { moveAction.Move(); }
    }
}
