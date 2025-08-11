using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicWeaponAttack : AttackAction
{
    // 무기로 사용할 개체
    // 반드시 인스펙터 창에서 지정되어 있어야 함
    [SerializeField] private GameObject myWeapon = null;

    // 무기가 활성화되어있을 시간
    [SerializeField] protected float weaponActiveTime = 1f;


    // BasicActorWeapon 캐시
    private BasicActorWeapon _weapon = null;
    protected BasicActorWeapon weapon
    {
        get
        {
            if (_weapon == null)
            {
                _weapon = myWeapon.GetComponent<BasicActorWeapon>();
                if (_weapon == null ) { _weapon = myWeapon.AddComponent<BasicActorWeapon>(); }
            }
            return _weapon;
        }
    }


    protected override void Awake()
    {
        base.Awake();
        weapon.SetWeapon(targetTag, thisActor);
        attackRate += weaponActiveTime;
    }


    protected virtual void UseWeapon()
    { weapon.UseWeapon(attackDamage, maxHitCount, knockBackPower, knockBackHeight, hitEffect, effectDestoryTime); }

    protected virtual void NotUseWeapon()
    { weapon.NotUseWeapon(); }


    protected override void DoAttack()
    {
        // --- 무기 활성화 ---
        UseWeapon();

        // --- 일정 시간 후 무기 비활성화 ---
        Timer.Instance.StartTimer(this, weaponActiveTime, NotUseWeapon);
    }
}