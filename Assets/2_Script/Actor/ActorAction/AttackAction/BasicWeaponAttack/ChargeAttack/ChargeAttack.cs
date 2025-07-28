using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ChargeAttack : BasicWeaponAttack
{
    protected Rigidbody rigid;
    [SerializeField] protected float chargeSpeed = 20f;

    [SerializeField] protected float decelerateSpeed = 1f;
    [SerializeField] protected float decelerateRate = 0.1f;

    protected float originalChargeSpeed = 0f;


    protected override void Awake()
    {
        base.Awake();
        rigid = GetComponent<Rigidbody>();
        this.enabled = false;
        originalChargeSpeed = chargeSpeed;
    }


    protected override void DoAttack()
    {
        base.DoAttack();

        // 돌진 활성화
        Timer.Instance.StartTimer(this, "_DoAttack", weaponBeforeDelay,
            () => {
                this.enabled = true;
                rigid.isKinematic = true;

                Timer.Instance.StartTimer(this, "_EndAttack", weaponActiveTime,
                    () => {
                        Timer.Instance.StopEndlessTimer(this, "_Decelerate");
                        rigid.isKinematic = false;
                        this.enabled = false;
                        chargeSpeed = originalChargeSpeed;
                        rigid.velocity = Vector3.zero;
                    });
            });

        // 0.1초마다 감속
        Timer.Instance.StartEndlessTimer(this, "_Decelerate", decelerateRate, () => { chargeSpeed -= decelerateSpeed; });
    }

    private void FixedUpdate()
    {
        Vector3 nextPos = rigid.position + transform.forward * chargeSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(nextPos);
    }
}