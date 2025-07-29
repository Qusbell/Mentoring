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


    protected override void Awake()
    {
        base.Awake();
        rigid = GetComponent<Rigidbody>();
        this.enabled = false;
    }


    private System.Action whenNoLongerChargeAttack = null;

    protected override void DoAttack()
    {
        base.DoAttack();
        float originalChargeSpeed = chargeSpeed;

        // 돌진 활성화
        Timer.Instance.StartTimer(this, "_DoAttack", weaponBeforeDelay,
            () => {
                this.enabled = true;
                rigid.isKinematic = true;
                whenNoLongerChargeAttack = () =>
                {
                    Timer.Instance.StopEndlessTimer(this, "_Decelerate");
                    rigid.isKinematic = false;
                    this.enabled = false;
                    chargeSpeed = originalChargeSpeed;
                    rigid.velocity = Vector3.zero;
                };

                Timer.Instance.StartTimer(this, "_EndAttack", weaponActiveTime, whenNoLongerChargeAttack);
            });

        // 0.1초마다 감속
        Timer.Instance.StartEndlessTimer(this, "_Decelerate", decelerateRate, () => { chargeSpeed -= decelerateSpeed; });
    }


    private void FixedUpdate()
    {
        Vector3 nextPos = rigid.position + transform.forward * chargeSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(nextPos);
    }


    // 돌진 중 튕겨내기
    private void OnCollisionStay(Collision collision)
    {
        if (this.enabled && collision.gameObject.CompareTag("Monster"))
        {
            Rigidbody otherRigid = collision.gameObject.GetComponent<Rigidbody>();
            if (otherRigid != null)
            {
                // 좌/우 방향벡터
                Vector3 rightDir = transform.right;
                Vector3 leftDir = -transform.right;

                // 충돌 위치와 자신의 위치 벡터
                Vector3 fromSelfToCollision = collision.transform.position - transform.position;

                // fromSelfToCollision이 오른쪽 방향인지 왼쪽 방향인지 판단
                float dot = Vector3.Dot(fromSelfToCollision, rightDir);

                // 임펄스 크기 설정
                float impulseStrength = 4f;

                // 충돌 대상이 오른쪽에 있으므로 오른쪽 임펄스
                if (dot > 0f)
                { otherRigid.AddForce(rightDir * impulseStrength, ForceMode.Impulse); }
                // 왼쪽에 있으므로 왼쪽 임펄스
                else
                { otherRigid.AddForce(leftDir * impulseStrength, ForceMode.Impulse); }
            }
        }
    }

}