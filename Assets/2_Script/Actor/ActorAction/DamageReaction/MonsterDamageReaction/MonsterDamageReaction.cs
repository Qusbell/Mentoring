using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDamageReaction : DamageReaction
{
    [SerializeField] float redTimeWhenHit = 0.2f;
    [SerializeField] float redTimeWhenDie = 0.3f;
    [SerializeField] float redColor = 0.6f;


    protected override void Awake()
    {
        base.Awake();

        // 피격 시 빨개지기
        ColorChangeAction colorChangeAction = GetComponent<ColorChangeAction>();
        if (colorChangeAction == null)
        { colorChangeAction = this.gameObject.AddComponent<ColorChangeAction>(); }

        // 빨간색 세팅
        colorChangeAction.SetRed(redColor);


        // 히트/다이 이벤트 세팅
        System.Action hitRedAction = null;

        hitRedAction = () => {
                colorChangeAction.ChangeToRed();
                Timer.Instance.StartTimer(this, redTimeWhenHit, colorChangeAction.RestoreOriginalColors); };
        whenHit.AddMulti(hitRedAction);

        hitRedAction = () => {
            colorChangeAction.ChangeToRed();
            Timer.Instance.StartTimer(this, redTimeWhenDie, colorChangeAction.RestoreOriginalColors); };
        whenDie.AddMulti(hitRedAction);
    }


    public override void TakeDamage(int damage, Actor enemy, float knockBackPower = 0, float knockBackHeight = 0)
    {
        Monster monster = GetComponent<Monster>();
        if (monster != null)
        {
            Transform tempTrans = monster.target;
            monster.target = enemy.transform;
        }

        base.TakeDamage(damage, enemy, knockBackPower, knockBackHeight);
    }


    protected override void Die()
    {
        base.Die();

        // --- 가라앉기 / 제거 ---

        // 사망 시간
        float deathTime = 2f;
        // 가라앉는 시간
        float fallTime = 1f;

        // 가라앉기 (콜라이더를 위로 빼기)
        CapsuleCollider[] cols = GetComponentsInChildren<CapsuleCollider>();

        // 1초 뒤부터 가라앉기 시작
        Timer.Instance.StartTimer(this, deathTime - fallTime,
            () => {
                // 모든 콜라이더 수집 후 일괄 작동
                int i = 0;
                foreach (var col in cols)
                {
                    i++;
                    Timer.Instance.StartRepeatTimer(this, "_FallDown" + i, fallTime, () => col.center -= Vector3.down * 0.005f);
                }
            });

        // 2초 후 제거
        Timer.Instance.StartTimer(this, "_WhenDie", deathTime, () => Destroy(this.gameObject));
    }


}
