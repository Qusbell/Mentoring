using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ChargeAttack : BasicWeaponAttack
{
    [SerializeField] protected float chargeSpeed = 20f;

    // 경고 발판
    private GameObject warningPlane = null;
    private float warningDistance = 16f;

    private Vector3 chargeVec = Vector3.zero;


    protected override void Awake()
    {
        base.Awake();
        checkLayer = 1 << LayerMask.NameToLayer("Cube"); // cube를 만나면 돌진 정지
        this.enabled = false;
        thisActor.damageReaction.whenDie.AddOnce(() => { WarningPlaneSetter.DelWarning(this, ref warningPlane); });

        // 경고 발판 길이
        warningDistance = chargeSpeed;
    }


    private System.Action whenNoLongerChargeAttack = null;


    protected override void BeforeAttack()
    {
        // --- 경고 발판 생성 ---
        warningPlane = WarningPlaneSetter.SetWarning(this, 1.5f, warningDistance, weaponBeforeDelay, transform.position, transform.forward);
    }



    protected override void CancelAttack()
    {
        base.CancelAttack();
        WarningPlaneSetter.DelWarning(this, ref warningPlane);
    }


    protected override void DoAttack()
    {
        base.DoAttack();

        // --- 돌진 방향 지정 ---
        chargeVec = transform.forward;

        // --- 발판 반환 ---
        WarningPlaneSetter.DelWarning(this, ref warningPlane);

        // -- 사망 시 리턴 ---
        if (thisActor.damageReaction.isDie) { return; }

        // --- 물리 조정 && 돌진 활성화 ---
        this.enabled = true;
        thisActor.rigid.isKinematic = true;

        whenNoLongerChargeAttack = () =>
        {
            thisActor.rigid.isKinematic = false;
            this.enabled = false;
            thisActor.rigid.velocity = Vector3.zero;
        };

        Timer.Instance.StartTimer(this, "_EndAttack", weaponActiveTime, whenNoLongerChargeAttack);
    }


    private LayerMask checkLayer;
    private float checkRadius = 0.1f;

    private void FixedUpdate()
    {
        // --- 다음 위치 계산
        Vector3 nextPos = thisActor.rigid.position + chargeVec * chargeSpeed * Time.fixedDeltaTime;
        
        // --- 다음 위치의 장애물 확인 ---
        Collider[] hits = Physics.OverlapSphere(nextPos + new Vector3(0, checkRadius, 0), checkRadius, checkLayer);

        foreach (var hit in hits)
        {
            // "Cube" 태그이거나, 검사 레이어에 속한다면
            if (((checkLayer.value & (1 << hit.gameObject.layer)) != 0) || hit.CompareTag("Cube"))
            {
                // 자기 자신이 아닐 때(중복체크 방지)
                if (hit.gameObject != this.gameObject)
                {
                    whenNoLongerChargeAttack();
                    return; // 더 이상 실행 X
                }
            }
        }

        // --- 이동 ---
        thisActor.rigid.MovePosition(nextPos);
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
                float pushPower = 4f;

                // 충돌 대상이 오른쪽에 있으므로 오른쪽 임펄스
                if (dot > 0f)
                { otherRigid.AddForce(rightDir * pushPower, ForceMode.Impulse); }
                // 왼쪽에 있으므로 왼쪽 임펄스
                else
                { otherRigid.AddForce(leftDir * pushPower, ForceMode.Impulse); }
            }
        }
    }

}