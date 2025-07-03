using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;



//==================================================
// 피격으로 인한 피해 반응 / 사망 시 처리
//==================================================
public class DamageReaction : ActorAction
{
    [SerializeField] protected int maxHp = 10;  // 최대 생명력
    [SerializeField] protected int nowHp = 10;  // 현재 생명력


    // 외부에서부터 가져올 피격 시 액션
    // 애니메이션 요소로 사용 중
    public Action hitAction { private get; set; }
    public Action dieAction { private get; set; }


    // 피격
    public virtual void TakeDamage(int damage)
    {
        if (damage <= nowHp)
        { nowHp -= damage; }
        else
        { nowHp = 0; }


        if (0 < nowHp)
        { Hit(); }
        else
        { Die(); }
    }


    protected void Hit()
    {
        if (hitAction != null)
        { hitAction(); }
    }


    // 사망 처리
    protected virtual void Die()
    {
        if(dieAction != null)
        { dieAction(); }

        // ----- 사망 시, 모든 ActorAction / 콜라이더 비활성화 -----

        ActorAction[] actorActions = this.GetComponents<ActorAction>();
        if(actorActions != null)
        {
            foreach (var item in actorActions)
            { item.enabled = false; }
        }

        Collider[] colliders = this.GetComponentsInChildren<Collider>();
        if(colliders != null)
        {
            foreach (var item in colliders)
            { item.enabled = false; }
        }

        // 일시적으로 물리 영향 X
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        { rb.isKinematic = true; }

        StartCoroutine(Timer.StartTimer(3f, () => Destroy(gameObject)));
    }
}