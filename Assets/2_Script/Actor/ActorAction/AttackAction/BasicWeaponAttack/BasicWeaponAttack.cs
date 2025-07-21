using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class BasicWeaponAttack : AttackAction
{
    // 무기로 사용할 개체
    // 반드시 인스펙터 창에서 지정되어 있어야 함
    [SerializeField] private GameObject myWeapon = null;

    // 무기 활성화 이전 시간
    [SerializeField] private float weaponBeforeDelay = 0.2f;

    // 무기가 활성화되어있을 시간
    [SerializeField] private float weaponActiveTime = 1f;

    // 최대 히트 가능 횟수
    [SerializeField] private int maxHitCount = 1;

    // BasicActorWeapon 캐시
    private BasicActorWeapon _weapon = null;
    protected BasicActorWeapon weapon
    {
        get
        {
            if (_weapon == null)
            {
                _weapon = myWeapon.GetComponent<BasicActorWeapon>();
                if(_weapon == null ) { _weapon = myWeapon.AddComponent<BasicActorWeapon>(); }
            }
            return _weapon;
        }
    }


    protected override void Awake()
    {
        base.Awake();
        weapon.SetWeapon(targetTag, this.gameObject);
        // <- 여기서 애니메이션 길이를 체크하고
        // ActiveTime이 애니메이션 길이보다 더 길다면 맞춰주기?
    }

    protected override void DoAttack()
    {
        Timer.Instance.StartTimer(
                this, "_Use",
                weaponBeforeDelay,
                () => weapon.UseWeapon(attackDamage, maxHitCount));

        Timer.Instance.StartTimer(
                this, "_NotUse",
                weaponActiveTime,
                weapon.NotUseWeapon);

        //  StartCoroutine(Timer.StartTimer(weaponBeforeDelay, weapon.UseWeapon));
        //  StartCoroutine(Timer.StartTimer(weaponBeforeDelay + weaponActiveTime, weapon.NotUseWeapon));
    }
}