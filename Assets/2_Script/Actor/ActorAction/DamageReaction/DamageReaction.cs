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

    // 외부에서부터 가져올 피격/사망 시 액션
    // 애니메이션 요소로 사용 중
    public Action hitAnimation { private get; set; }
    public Action dieAnimation { private get; set; }

    // 피격
    public virtual void TakeDamage(int damage)
    {
        // ===== 데미지 판정 =====
        if (damage <= nowHp)
        { nowHp -= damage; }
        else
        { nowHp = 0; }

        // ===== 생명력이 남아있다면: 피격 판정 =====
        if (0 < nowHp)
        { Hit(); }

        // ===== 생명력이 바닥났다면: 사망 판정 =====
        else
        { Die(); }
    }


    // 피격
    public virtual void TakeDamage(int damage, GameObject enemy)
    {
        // 마지막으로 공격한 적을 타겟팅
        // <- 게으른 코딩. 이후 수정
        GameObject lastAttackedEnemy = enemy;
        Monster monster = GetComponent<Monster>();
        if (monster != null)
        {
            Transform tempTrans = monster.target;
            monster.target = lastAttackedEnemy.transform;
        }

        // 피격
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
        if (hitAnimation != null)
        { hitAnimation(); }
    }


    // 사망 처리
    protected virtual void Die()
    {
        if (dieAnimation != null)
        { dieAnimation(); }

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

        // 3초 후 제거
        Timer.Instance.StartTimer(this, "_WhenDie", 3f, () => Destroy(this.gameObject)); // <- 이후 오브젝트 풀로 이동하는 걸 고려
        // StartCoroutine(Timer.StartTimer(3f, () => Destroy(this.gameObject)));

        // 모든 마테리얼 투명화 (2초)
        //GetComponent<SetMaterials>().SetAllMaterialsToFadeOut();
    }
}