using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 점프 공격
public class DropAttack : AttackAction
{
    // 낙하 공격
    protected Rigidbody rigid;

    // 무기로 사용할 개체
    // 반드시 인스펙터 창에서 지정되어 있어야 함
    [SerializeField] private GameObject myWeapon = null;

    // 낙하 속도
    [SerializeField] private float dropSpeed = 13f;

    // 최대 히트 가능 횟수
    [SerializeField] private int maxHitCount = 1;


    // BasicActorWeapon 캐시
    private DropActorWeapon weapon = null;


    protected override void Awake()
    {
        base.Awake();
        weapon = myWeapon.GetComponent<DropActorWeapon>();
        if (weapon == null)
        { weapon = myWeapon.AddComponent<DropActorWeapon>(); }
        weapon.SetWeapon(targetTag, attackRange, this.gameObject.layer);

        rigid = GetComponent<Rigidbody>();
    }


    protected override void DoAttack()
    {
        this.gameObject.layer = LayerMask.NameToLayer("IgnoreOtherActor");
        weapon.UseWeapon(attackDamage, maxHitCount, AttackWeaponType.DropAttack);
        rigid.AddForce(Vector3.down * dropSpeed, ForceMode.Impulse);
    }
}