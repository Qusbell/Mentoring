using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



// 공격 명칭
public enum AttackName
{
    Player_BasicAttack,
    Player_JumpComboAttack,
    Player_DodgeComboAttack,

    Player_WhenDodge,

    Monster_MinionNormalAttack,
    Monster_ShieldChargeAttack,
    Monster_ArcherFireAttack,
    Monster_MageSpellAttack
}


//==================================================
// 공격 행동
//==================================================
abstract public class AttackAction : ActorAction
{
    // 해당 공격의 명칭
    [field: SerializeField] public AttackName attackName { get; private set; }
    
    // 공격력
    [SerializeField] protected int attackDamage = 1;

    // 공격 사거리
    // Monster의 사거리 요소로도 사용 중
    [field: SerializeField] public float attackRange { get; protected set; } = 3f;

    // 공격 대상 태그 (해당 태그를 가진 오브젝트만 공격)
    [SerializeField] protected string targetTag = "";

    // 최대 히트 가능 횟수
    [SerializeField] protected int maxHitCount = 1;

    // <- 공격 대상의 레이어

    // 공격 간격 (== 공격 속도)
    [SerializeField] protected float attackRate = 0.5f;

    // 넉백 거리
    [SerializeField] protected float knockBackPower = 0f;
    [SerializeField] protected float knockBackHeight = 0f;

    // 공격 코스트
    [field: SerializeField] public int attackCost { get; set; } = 0;

    // 이펙트 프리펩
    [SerializeField] protected GameObject hitEffect = null;
    [SerializeField] protected float effectDestoryTime = 1f; // <- LeftTime 설정 고려

    // 공격 선딜레이
    [SerializeField] protected float weaponBeforeDelay = 0.2f;

    // 피격당했을 경우, 이 공격을 취소할 것인가?
    [SerializeField] protected bool isCancelWhenHit = false;


    // 공격 가능 여부
    protected bool _isCanAttack = true;
    public virtual bool isCanAttack
    {
        get
        { return _isCanAttack; }
        protected set
        { _isCanAttack = value; }
    }


    protected override void Awake()
    {
        base.Awake();

        // 타겟태그 검사
        // 배정되지 않은 경우 : 기초적인 재배정
        if (targetTag == "")
        {
            if (gameObject.tag == "Monster") { targetTag = "Player"; }
            else if (gameObject.tag == "Player") { targetTag = "Monster"; }
        }

        // 공격 선딜레이가 공격 간격에 포함
        attackRate += weaponBeforeDelay;

        // 공격받았을 경우 캔슬 여부
        if (isCancelWhenHit)
        { thisActor?.damageReaction?.whenHit.AddMulti(CancelAttack); }
    }


    // 실제로 호출할 메서드
    public void Attack()
    {
        // 공격 가능 시간 확인 후
        // (참이라면) 실제 공격 발생
        if (CheckCanAttack())
        {
            // 공격 선딜레이 중 동작
            BeforeAttack();

            // 선딜레이 후 발생
            Timer.Instance.StartTimer(this, "_DoAttack", weaponBeforeDelay, DoAttack);
        }
    }


    // 공격 가능 시간 확인
    // 공격 가능 : true
    // 공격 불가 : false
    private bool CheckCanAttack()
    {
        if (isCanAttack)
        {
            isCanAttack = false;
            Timer.Instance.StartTimer(
                this, "_CanAttack",
                attackRate,
                () => { isCanAttack = true; });

            return true;
        }
        else { return false; }
    }


    // 공격 캔슬
    protected virtual void CancelAttack()
    { Timer.Instance.StopTimer(this, "_DoAttack"); }

    // 공격 전 동작
    protected virtual void BeforeAttack() { }

    // 실제 Attack 구현
    protected abstract void DoAttack();
}