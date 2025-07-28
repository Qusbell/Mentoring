using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;


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
        {
            return nowHp;
        }
        protected set
        {
            // 최대/최소값 보정
            if (value < 0) { value = 0; }
            else if (value > maxHp) { value = maxHp; }

            nowHp = value;
        }
    }

    // 죽었을 때 바운스 거리
    [SerializeField] protected int bouncePowerWhenDie = 10;

    // hit/die 이벤트
    public List<System.Action> whenHitEvent = new List<Action>();
    public List<System.Action> whenDieEvent = new List<Action>();


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

        // 넉백 준비
        Vector3 tempVector = (this.transform.position - enemy.transform.position).normalized;
        Rigidbody rigid = GetComponent<Rigidbody>(); // <- null인 경우 생각

        // --- 피격/사망 시 처리 ---
        if (0 < healthPoint)
        {
            Hit();
            tempVector *= knockBackPower;
            tempVector.y = knockBackHeight; // 약간 위로 넉백
            rigid.velocity = tempVector;
        }
        else
        {
            Die();
            rigid.velocity = tempVector * bouncePowerWhenDie;
        }
    }


    protected void Hit()
    {
        // --- hit 이벤트 호출 ---
        foreach (var hitEvent in whenHitEvent)
        { hitEvent?.Invoke(); }
    }


    // 사망 처리
    protected virtual void Die()
    {
        // --- die 이벤트 호출 ---
        foreach (var dieEvent in whenDieEvent)
        { dieEvent?.Invoke(); }

        // ----- 사망 시, 모든 ActorAction 비활성화 -----
        ActorAction[] actorActions = this.GetComponents<ActorAction>();
        if(actorActions != null)
        {
            foreach (var item in actorActions)
            { item.enabled = false; }
        }

        // 이벤트 전부 클리어
        whenHitEvent.Clear();
        whenDieEvent.Clear();

        // 레이어 변경
        int targetLayer = LayerMask.NameToLayer("DieActorLayer");
        LayerChanger.ChangeLayerWithAll(this.gameObject, targetLayer);

        // 2초 후 제거
        Timer.Instance.StartTimer(this, "_WhenDie", 2f, () => Destroy(this.gameObject)); // <- 이후 오브젝트 풀로 이동하는 걸 고려

        //  // 모든 마테리얼 투명화 (2초)
        //  SetMaterials setMaterials = GetComponent<SetMaterials>();
        //  if(setMaterials != null)
        //  { setMaterials.SetAllMaterialsToFadeOut(); }
    }

    public void Heal(int amount)
    {
        healthPoint += amount; // 내부에서는 private set 사용 가능
    }
}