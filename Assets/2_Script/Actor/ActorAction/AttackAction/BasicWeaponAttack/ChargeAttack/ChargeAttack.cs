using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ChargeAttack : BasicWeaponAttack
{
    [SerializeField] protected float chargeSpeed = 20f;    // 속도
    [SerializeField] protected float chargeDistance = 20f; // 총 거리
    [SerializeField] protected float warningWidth = 1.5f;  // 경고 발판 

    // 경고 발판
    protected GameObject warningPlane = null;

    // 최초 위치
    private Vector3 originPos = Vector3.zero;


    protected override void Awake()
    {
        base.Awake();
        checkLayer = 1 << LayerMask.NameToLayer("Cube"); // cube를 만나면 돌진 정지
        this.enabled = false;

        // 사망 시 돌진발판 즉시 제거
        thisActor.damageReaction.whenDie.AddMulti(CancelAttack, true);
    }


    protected override void BeforeAttack()
    {
        // --- 경고 발판 생성 ---
        warningPlane = WarningPlaneSetter.SetWarning(this, 1.5f,
            chargeDistance,
            weaponBeforeDelay,
            transform.position,
            transform.forward);

        // --- 지면 상태인 경우 : 키네마틱 활성화
        if (thisActor.isRand)
        { thisActor.rigid.isKinematic = true; }
    }

    protected override void CancelAttack()
    {
        base.CancelAttack();
        WarningPlaneSetter.DelWarning(this, ref warningPlane);
    }

    protected override void DoAttack()
    {
        // --- 원래 위치 확인 ---
        originPos = transform.position;

        // --- 발판 반환 ---
        WarningPlaneSetter.DelWarning(this, ref warningPlane);

        // -- 사망 시 리턴 ---
        if (thisActor.damageReaction.isDie) { return; }

        // --- 물리 조정 && 돌진 활성화 ---
        StartCharge();
        // Timer.Instance.StartTimer(this, "_EndAttack", weaponActiveTime, EndCharge);
    }


    private void StartCharge()
    {
        this.enabled = true;
        UseWeapon();
    }

    protected virtual void EndCharge()
    {
        Timer.Instance.StopTimer(this, "_EndAttack");
        this.enabled = false;
        thisActor.rigid.isKinematic = false;
        thisActor.rigid.velocity = Vector3.zero;

        NotUseWeapon(); // <- 돌진 종료 시 무기 비활성화
    }

    
    protected virtual void EndChargeWhenCube()
    { EndCharge(); }



    private LayerMask checkLayer;     // 체크할 레이어
    private float checkRadius = 0.1f; // 체크 범위
    private Collider[] cubes = new Collider[3]; // 큐브 콜라이더 캐시

    private void FixedUpdate()
    {
        // --- 착지 중: 물리 무시 ---
        if (thisActor.isRand) { thisActor.rigid.isKinematic = true; }
        else { thisActor.rigid.isKinematic = false; }

        // --- 현재 위치와 최초 위치간 거리 계산 ---
        float traveledDistance = Vector3.Distance(thisActor.rigid.position, originPos);
        if (traveledDistance >= chargeDistance)
        {
            EndCharge();
            return;
        }

        // ---┐
        // <- AI 제작 구간 (속도 조절)
        // --- 기본 속도: 마지막 20% 전까지는 고정, 이후 감속 ---
        float progress = traveledDistance / chargeDistance;
        float curSpeed = chargeSpeed;

        if (progress >= 0.8f) // 마지막 20% 구간
        {
            // 0.8~1.0 구간에서 점진적 감소 (곡선은 적절히 조절)
            float slowProgress = (progress - 0.8f) / 0.2f; // 0~1로 정규화
                                                           // 부드럽게 0.3배까지 감속 (0.3은 남길 최소 속도)
            float minSpeedRate = 0.3f;
            float speedRate = Mathf.Lerp(1f, minSpeedRate, slowProgress);
            curSpeed = chargeSpeed * speedRate;
        }
        // ---┘

        // --- 다음 위치 계산 ---
        Vector3 nextPos = thisActor.rigid.position + transform.forward * curSpeed * Time.fixedDeltaTime;

        // --- 다음 위치 장애물 확인 (위쪽) ---
        int count = Physics.OverlapSphereNonAlloc(nextPos + Vector3.up * checkRadius * 2, checkRadius, cubes, checkLayer);
        if (count > 0)
        {
            Debug.Log("장애물 정지");
            EndChargeWhenCube();
            return;
        }

        // --- 다음 위치 낭떠러지 여부 확인 (아래쪽) ---
        count = Physics.OverlapSphereNonAlloc(nextPos - new Vector3(0, checkRadius, 0), checkRadius, cubes, checkLayer);
        if (count == 0)
        {
            Debug.Log("낭떠러지 정지");
            EndCharge();
            return;
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

    // <- 임시 (낙사 시 발판 반환)
    protected void OnDestroy()
    { WarningPlaneSetter.DelWarning(this, ref warningPlane); }


    private void OnDrawGizmos()
    {
        // 현재 오브젝트가 활성화되어 있지 않아도 위치 표시를 위해 transform 사용
        Vector3 currentPos = transform.position;

        // 현재 위치와 최초 위치 사이의 거리 계산
        float traveledDistance = Vector3.Distance(currentPos, originPos);

        // 다음 위치 예측 (기본 속도 사용)
        float curSpeed = chargeSpeed;
        if (traveledDistance / chargeDistance >= 0.8f)
        {
            // 마지막 20% 감속 고려 (간단히 최소 0.3배 속도 적용)
            curSpeed = chargeSpeed * 0.3f;
        }
        Vector3 nextPos = currentPos + transform.forward * curSpeed * Time.fixedDeltaTime;

        // Gizmo 색상 및 구체 크기 설정
        float radius = checkRadius;

        // 1. 장애물 확인용 OverlapSphere 위치 (nextPos 위쪽)
        Gizmos.color = Color.red * new Color(1, 1, 1, 0.5f); // 반투명 빨강
        Vector3 obstacleCheckPos = nextPos + Vector3.up * radius * 2;
        Gizmos.DrawSphere(obstacleCheckPos, radius);

        // 2. 낭떠러지 확인용 OverlapSphere 위치 (nextPos 아래쪽)
        Gizmos.color = Color.blue * new Color(1, 1, 1, 0.5f); // 반투명 파랑
        Vector3 cliffCheckPos = nextPos - new Vector3(0, radius, 0);
        Gizmos.DrawSphere(cliffCheckPos, radius);

        // 3. 경로 거리 표시 (최초 위치 originPos가 Vector3.zero가 아닌 경우만)
        if (originPos != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(originPos, currentPos);
        }
    }
}