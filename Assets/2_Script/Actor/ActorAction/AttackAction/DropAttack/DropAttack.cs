using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

// 점프 공격
public class DropAttack : AttackAction
{
    // 낙하 공격 rigid.AddForce
    protected Rigidbody rigid;

    // 무기로 사용할 개체
    // 반드시 인스펙터 창에서 지정되어 있어야 함
    [SerializeField] private GameObject myWeapon = null;

    // 낙하 속도
    [SerializeField] private float dropSpeed = 13f;

    // 낙하 가능한 최대 거리
    [SerializeField] private float maxDropDistance = 20f;

    // 낙하 직전 시간
    [SerializeField] private float dropBeforeTime = 0.2f;


    private DropActorWeapon _weapon = null;
    protected DropActorWeapon weapon
    {
        get
        {
            if (_weapon == null)
            {
                _weapon = myWeapon.GetComponent<DropActorWeapon>();
                if(_weapon == null)
                { _weapon = myWeapon.AddComponent<DropActorWeapon>(); }
            }
            return _weapon;
        }
    }



    protected override void Awake()
    {
        base.Awake();
        weapon.SetWeapon(targetTag, attackRange, this.gameObject.layer, this.gameObject);
        rigid = GetComponent<Rigidbody>();
    }


    protected override void DoAttack()
    {
        this.gameObject.layer = LayerMask.NameToLayer("IgnoreOtherActor");
        weapon.UseWeapon(attackDamage, maxHitCount, knockBackPower, hitEffect, effectDestoryTime);

        System.Action dropAttackAction = () =>
        {
            // 레이캐스트 정보
            RaycastHit tempHit;
            Vector3 rayOrigin = this.transform.position;
            Vector3 rayDirection = Vector3.down;

            // 낙하 시도 여부
            bool isCanUseDropAttack = Physics.Raycast(rayOrigin, rayDirection, out tempHit, maxDropDistance);
            if (isCanUseDropAttack) { rigid.AddForce(Vector3.down * dropSpeed, ForceMode.Impulse); }

            // 디버그
            // Ray 그리기(맞으면 빨간색, 아니면 노란색)
            Color rayColor = isCanUseDropAttack ? Color.red : Color.yellow;
            Debug.DrawRay(rayOrigin, rayDirection * maxDropDistance, rayColor, 1.0f);
        };

        Timer.Instance.StartTimer(this, "_DropAttackAction", dropBeforeTime, dropAttackAction);
    }
}