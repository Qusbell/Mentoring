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

    public int healthPoint
    {
        get
        {
            return nowHp;
        }
        private set
        {
            // <- value 조건
            nowHp = value;
        }
    }



    // 외부에서부터 가져올 피격/사망 시 액션
    // 애니메이션 요소로 사용 중
    public Action hitAnimation { private get; set; }
    public Action dieAnimation { private get; set; }

    // 죽었을 때 바운스 거리
    [SerializeField] protected int bouncePowerWhenDie = 10;

    // 피격 시 넉백 높이
    [SerializeField] protected float knockBackHeight = 0.65f;


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


    // 피격 (야매)
    public virtual void TakeDamage(int damage, Actor enemy, float knockBackPower = 0f)
    {
        // 몬스터: 마지막으로 공격한 적을 타겟팅
        // <- 야매 코딩. 이후 수정
        Monster monster = GetComponent<Monster>();
        if (monster != null)
        {
            Transform tempTrans = monster.target;
            monster.target = enemy.transform;
        }

        // ----- 피해 적용 -----
        if (damage <= nowHp)
        { nowHp -= damage; }
        else
        { nowHp = 0; }


        // 넉백 준비
        Vector3 vector = (this.transform.position - enemy.transform.position).normalized;
        Rigidbody rigid = GetComponent<Rigidbody>(); // <- null인 경우 생각

        // ----- 피격/사망 시 처리 ------
        if (0 < nowHp)
        {
            Hit();
            vector.y += knockBackHeight; // 약간 위로 넉백
            rigid.AddForce(vector * knockBackPower, ForceMode.Impulse);
        }
        else
        {
            Die();
            rigid.AddForce(vector * bouncePowerWhenDie, ForceMode.Impulse);
        }
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

        // ----- 사망 시, 모든 ActorAction 비활성화 -----

        ActorAction[] actorActions = this.GetComponents<ActorAction>();
        if(actorActions != null)
        {
            foreach (var item in actorActions)
            { item.enabled = false; }
        }


        // 레이어 변경
        int targetLayer = LayerMask.NameToLayer("DieActorLayer");
        ChangeLayerRecursively(this.gameObject, targetLayer);


        // 2초 후 제거
        Timer.Instance.StartTimer(this, "_WhenDie", 2f, () => Destroy(this.gameObject)); // <- 이후 오브젝트 풀로 이동하는 걸 고려
        // StartCoroutine(Timer.StartTimer(2f, () => Destroy(this.gameObject)));

        // 모든 마테리얼 투명화 (2초)
        SetMaterials setMaterials = GetComponent<SetMaterials>();
        if(setMaterials != null)
        { setMaterials.SetAllMaterialsToFadeOut(); }
    }


    // 모든 레이어 변경
    // <- 유틸리티로 빼둘 생각?
    void ChangeLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            ChangeLayerRecursively(child.gameObject, newLayer);
        }
    }
}