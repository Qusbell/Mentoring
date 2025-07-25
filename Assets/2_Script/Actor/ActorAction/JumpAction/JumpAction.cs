using System.Collections;
using System.Collections.Generic;
using UnityEngine;




//==================================================
// 도약 / 점프
//==================================================
[RequireComponent(typeof(Rigidbody))]
public class JumpAction : ActorAction
{
    // 오브젝트에 대한 물리효과
    protected Rigidbody rigid;

    // 콜라이더
    private Collider myCollider;
    // 마찰계수 <- 나중에 Dodge와 통합
    private PhysicMaterial originalMaterial;   // 원래 Material 저장
    private PhysicMaterial zeroFrictionMaterial;

    // 지형과 접촉 판정을 내릴 콜라이더
    FootCollider foot;


    // 생성 시 초기화
    protected virtual void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();
        // null 초기화 방어
        if (rigid == null)
        {
            Debug.LogError("Rigidbody 컴포넌트 누락!", gameObject);
            enabled = false; // 생성 취소
        }


        // ----- 마찰 -----
        myCollider = GetComponent<Collider>();

        // 기존 마찰값 백업
        originalMaterial = myCollider.material;

        // 마찰 0인 Material 생성
        zeroFrictionMaterial = new PhysicMaterial();
        zeroFrictionMaterial.dynamicFriction = 0f;
        zeroFrictionMaterial.staticFriction = 0f;
        zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;


        // ----- 바닥 콜라이더 설정 -----
        foot = GetComponentInChildren<FootCollider>();
        if (foot == null) { Debug.Log(this.gameObject.name + " : 착지 판정용 콜라이더 부재"); }
        foot.ground.Add(Grounded);
    }


    //==================================================
    // 점프 메서드
    //==================================================

    // 점프 높이
    [SerializeField] float jumpHeight = 13;
    [SerializeField] float maxHorizontalSpeed = 6f;  // x,z 속도 최대 제한값

    // 점프
    // 위치 += 위쪽 방향 * 점프높이
    // 힘을 가함 (물리효과)
    public virtual void Jump()
    {
        // 점프 상태가 아니라면
        if (!isJump)
        {
            // 마찰계수 없애기
            myCollider.material = zeroFrictionMaterial;

            // 현재 힘 확인
            Vector3 nowVelocity = rigid.velocity;

            // 현재 x, z 속도 크기 계산 (수평속도)
            Vector2 horizontalVel = new Vector2(nowVelocity.x, nowVelocity.z);
            float horizontalSpeed = horizontalVel.magnitude;

            // 최대 속도 이상이면 비율에 맞게 축소
            if (maxHorizontalSpeed < horizontalSpeed)
            {
                float scale = maxHorizontalSpeed / horizontalSpeed;
                nowVelocity.x *= scale;
                nowVelocity.z *= scale;
            }

            // 상승 힘 생성
            nowVelocity.y = jumpHeight;

            rigid.velocity = nowVelocity;
        }
    }

    public bool isJump
    {
        get { return !foot.isRand; }
    }
    
    // FootCollider으로 전달되는 용도
    protected void Grounded()
    {
        myCollider.material = originalMaterial;
        // <- 착지 시마다?
        // this.rigid.velocity = Vector3.zero;
    }
}
