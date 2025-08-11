using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


//==================================================
// 피격으로 인한 피해 반응 / 사망 시 처리
//==================================================
public class DamageReaction : ActorAction
{
    [SerializeField] private int maxHp = 10;  // 최대 생명력
    [SerializeField] private int nowHp = 10;  // 현재 생명력

    // 생명력 통합
    public int healthPoint
    {
        get
        { return nowHp; }
        protected set
        {
            // 최대/최소값 보정
            if (value < 0) { value = 0; }
            else if (value > maxHp) { value = maxHp; }

            nowHp = value;
            whenHealthChange.Invoke();
        }
    }

    public bool isDie
    {
        get { return healthPoint <= 0; }
    }


    // 죽었을 때 바운스 거리
    [SerializeField] protected int bouncePowerWhenDie = 10;


    // hit/die 이벤트
    public MyEvent whenHit = new MyEvent();
    public MyEvent whenDie = new MyEvent();

    // 체력 변경 시마다
    public MyEvent whenHealthChange = new MyEvent();

    // 피격
    public virtual void TakeDamage(int damage, Actor enemy, float knockBackPower = 0f, float knockBackHeight = 0f)
    {
        // --- 음수 데미지 체크 ---
        if (damage < 0)
        {
            Debug.Log($"{enemy.gameObject.name}의 공격 데미지가 {damage}");
            return;
        }

        // --- 피해 적용 ---
        healthPoint -= damage;

        // --- 피격/사망 시 처리 ---
        if (isDie) { Die(); }
        else { Hit(); }

        // --- 넉백 ---
        KnockBackImpulse(enemy.gameObject, knockBackPower, knockBackHeight);
    }


    // 넉백 따로 만들기
    public virtual void KnockBackImpulse(GameObject enemy, float knockBackPower, float knockBackHeight)
    {
        // 키네마틱 상태면 return
        if (thisActor.rigid.isKinematic) { return; }

        // 넉백 준비
        Vector3 tempVector = (this.transform.position - enemy.transform.position).normalized;
        
        tempVector *= knockBackPower;
        tempVector.y = knockBackHeight + thisActor.rigid.velocity.y; // 상/하 넉백
        if (27f < tempVector.y) { tempVector.y = 27f; }    // 과도한 vector 조절 (현재 27f)
        thisActor.rigid.velocity = tempVector; // 넉백 적용

        // 사망 시 추가넉백
        if (isDie)
        { thisActor.rigid.velocity = tempVector * bouncePowerWhenDie; }
    }



    protected void Hit()
    { whenHit.Invoke(); }


    // 사망 처리
    protected virtual void Die()
    {
        // --- die 이벤트 호출 ---
        whenDie.Invoke();

        // 이벤트 전부 클리어
        whenHit.ClearAll();
        whenDie.ClearAll();

        // --- 모든 ActorAction 비활성화 ---
        ActorAction[] actorActions = this.GetComponentsInChildren<ActorAction>();
        if(actorActions != null)
        {
            foreach (var item in actorActions)
            { item.enabled = false; }
        }

        // --- 물리 적용 ---
        thisActor.rigid.isKinematic = false;

        // 레이어 변경
        int targetLayer = LayerMask.NameToLayer("DieActorLayer");
        LayerChanger.ChangeLayerWithAll(this.gameObject, targetLayer);
    }

    public void Heal(int amount)
    {
        healthPoint += amount; 
    }

    public int maxHealthPoint
    {
        get { return maxHp; }
    }
}