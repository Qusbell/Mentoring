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

    // 경고 발판
    private GameObject warningPlane = null;


    protected override void Awake()
    {
        base.Awake();
        rigid = GetComponent<Rigidbody>();
        checkLayer = 1 << LayerMask.NameToLayer("Cube"); // cube를 만나면 돌진 정지
        this.enabled = false;
    }


    private System.Action whenNoLongerChargeAttack = null;

    protected override void DoAttack()
    {
        float originalChargeSpeed = chargeSpeed;
        warningPlane = WarningPlaneSetter.SetWarning(this, 1f, attackRange, weaponBeforeDelay, transform.position, transform.forward);
        base.DoAttack();


        // 돌진 활성화
        Timer.Instance.StartTimer(this, "_DoAttack", weaponBeforeDelay,
            () => {
                this.enabled = true;
                rigid.isKinematic = true;
                WarningPlaneSetter.DelWarning(this, warningPlane);

                whenNoLongerChargeAttack = () =>
                {
                    Timer.Instance.StopEndlessTimer(this, "_Decelerate");
                    rigid.isKinematic = false;
                    this.enabled = false;
                    chargeSpeed = originalChargeSpeed;
                    rigid.velocity = Vector3.zero;
                };

                Timer.Instance.StartTimer(this, "_EndAttack", weaponActiveTime, whenNoLongerChargeAttack);

                // 0.1초마다 감속
                Timer.Instance.StartEndlessTimer(this, "_Decelerate", decelerateRate, () => { chargeSpeed -= decelerateSpeed; });
            });

    }


    private LayerMask checkLayer;
    private float checkRadius = 0.1f;


    private void FixedUpdate()
    {
        // --- 다음 위치 계산
        Vector3 nextPos = rigid.position + transform.forward * chargeSpeed * Time.fixedDeltaTime;
        
        // --- 다음 위치의 장애물 확인 ---
        Collider[] hits = Physics.OverlapSphere(nextPos + new Vector3(0, checkRadius, 0), checkRadius, checkLayer);

        foreach (var hit in hits)
        {
            // "Cube" 태그이거나, 검사 레이어에 속한다면
            if (hit.CompareTag("Cube") || ((checkLayer.value & (1 << hit.gameObject.layer)) != 0))
            {
                // 자기 자신이 아닐 때(중복체크 방지)
                if (hit.gameObject != this.gameObject)
                {
                    whenNoLongerChargeAttack();
                    return; // 더 이상 실행할 필요 없습니다
                }
            }
        }

        // --- 이동 ---
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