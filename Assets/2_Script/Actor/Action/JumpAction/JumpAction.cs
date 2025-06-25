using System.Collections;
using System.Collections.Generic;
using UnityEngine;




//==================================================
// 도약 / 점프
//==================================================
[RequireComponent(typeof(Rigidbody))]
public class JumpAction : MonoBehaviour
{
    // 오브젝트에 대한 물리효과
    protected Rigidbody rigid;

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

        // 바닥 레이 사이의 간격
        // 너무 넓으면, 다른 큐브와 걸치는 경우 2중 점프 등 문제 발생
        // 너무 좁으면, 끄트머리에 걸쳤을 때 점프 불가능한 문제 발생
        raySpacing = (transform.localScale.x + transform.localScale.z) * 0.22f;

        // 착지 확인
        bottomRayDistance = transform.localScale.y * 1.05f;
    }


    //==================================================
    // 점프 메서드
    //==================================================

    // 점프 높이
    [SerializeField] float jumpHeight = 13;

    // 착지했는가에 대한 거리 기준
    protected float bottomRayDistance;

    // 다중 레이캐스트 (착지 판정)
    // 각 레이 사이의 간격
    protected float raySpacing;


    private void Update()
    {
        isJump = IsJump();
        // <- 애니메이션 변경? 아니면 Actor 쪽에서 직접 애니메이션 제어?
    }


    // 점프
    // 위치 += 위쪽 방향 * 점프높이
    // 힘을 가함 (물리효과)
    public virtual void Jump()
    {
        // 점프 상태가 아니라면
        if (!isJump)
        {
            // 불필요한 물리 초기화
            rigid.velocity = Vector3.zero;
            // 위쪽 방향으로 jumpHeight만큼 힘을 가함
            rigid.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
    }

    // 점프 상태 확인
    private bool _isJump = false;
    public bool isJump
    {
        get { return _isJump; }
        protected set { _isJump = value; }
    }

    // 착지 상태인지 판정
    protected bool IsJump()
    {
        // 앞/뒤 레이캐스트
        return !(
            // 앞쪽 레이캐스트
            Physics.Raycast(transform.position + (transform.forward * raySpacing),
            Vector3.down,
            bottomRayDistance) ||

            // 뒤쪽 레이캐스트
            Physics.Raycast(transform.position - (transform.forward * raySpacing),
            Vector3.down,
            bottomRayDistance)
            );
    }
}
