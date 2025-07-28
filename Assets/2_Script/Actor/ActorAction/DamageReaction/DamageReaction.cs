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


    public event System.Action whenHit = delegate { };
    public event System.Action whenDie = delegate { };


    // 외부에서부터 가져올 피격/사망 시 액션
    //  public List<System.Action> whenHit { get; set; } = new List<System.Action>();
    //  public List<System.Action> whenDie { get; set; } = new List<System.Action>();

    // 죽었을 때 바운스 거리
    [SerializeField] protected int bouncePowerWhenDie = 10;


    // 피격 (야매)
    public virtual void TakeDamage(int damage, Actor enemy, float knockBackPower = 0f, float knockBackHeight = 0f)
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
        Vector3 tempVector = (this.transform.position - enemy.transform.position).normalized;
        Rigidbody rigid = GetComponent<Rigidbody>(); // <- null인 경우 생각

        // ----- 피격/사망 시 처리 ------
        if (0 < nowHp)
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
        // --- 이벤트 호출 ---
        whenHit?.Invoke();
    }


    // 사망 처리
    protected virtual void Die()
    {
        // --- 사망 시 이벤트 호출 ---
        whenDie?.Invoke();


        // ----- 사망 시, 모든 ActorAction 비활성화 -----
        ActorAction[] actorActions = this.GetComponents<ActorAction>();
        if(actorActions != null)
        {
            foreach (var item in actorActions)
            { item.enabled = false; }
        }

        // 레이어 변경
        int targetLayer = LayerMask.NameToLayer("DieActorLayer");
        LayerChanger.ChangeLayerWithAll(this.gameObject, targetLayer);

        // 2초 후 제거
        Timer.Instance.StartTimer(this, "_WhenDie", 2f, () => Destroy(this.gameObject)); // <- 이후 오브젝트 풀로 이동하는 걸 고려
        // StartCoroutine(Timer.StartTimer(2f, () => Destroy(this.gameObject)));

        // 모든 마테리얼 투명화 (2초)
        SetMaterials setMaterials = GetComponent<SetMaterials>();
        if(setMaterials != null)
        { setMaterials.SetAllMaterialsToFadeOut(); }
    }
}