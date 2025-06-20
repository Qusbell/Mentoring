using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    protected virtual void Update()
    { SetInput(); }
    
    //==================================================
    // 방향 입력
    //==================================================

    // 입력받는 방향
    protected float moveHorizontal;
    protected float moveVertical;
    public Vector3 moveVec { get; protected set; }
    
    // moveKey 입력
    public bool isMoveKeyDown
    {
        get
        { return moveVec != Vector3.zero; }
    }

    // 입력 데드존
    [SerializeField] protected float inputDeadZone = 0.1f;

    // 이동 방향 입력
    protected void InputWASD()
    // 입력(WASD, ↑↓←→)으로 방향 지정
    // 정규화된(모든 방향으로 크기가 1인) 방향벡터 생성
    {
        // 방향 입력받음
        moveHorizontal = Input.GetAxisRaw("Horizontal"); // x축 (좌우)
        moveVertical = Input.GetAxisRaw("Vertical");     // z축 (앞뒤)

        // 데드존 검사
        if (Mathf.Abs(moveHorizontal) < inputDeadZone) { moveHorizontal = 0; }
        if (Mathf.Abs(moveVertical) < inputDeadZone) { moveVertical = 0; }

        // 방향 대입
        // 45도(쿼터뷰) 틀어진 방향
        // <- 실제 카메라 각도에 대응하도록 바꿀 것
        moveVec = (Quaternion.Euler(0, 45, 0)  // 이동 방향을 y축 기준 45도 회전 (카메라 각도)
            * (new Vector3(moveHorizontal, 0, moveVertical)).normalized); // 입력된 방향벡터
    }



    //==================================================
    // 점프 입력
    //==================================================

    // 점프 입력 여부
    public bool isJumpKeyDown { get; protected set; }

    // 점프 여부 입력
    // 스페이스 바
    protected void InputJump()
    { isJumpKeyDown = Input.GetButtonDown("Jump"); }



    //==================================================
    // 공격 입력
    //==================================================

    // 공격 입력 여부
    public bool isAttackKeyDown { get; protected set; }

    // 공격 입력
    // 좌클릭
    protected void InputAttack()
    { isAttackKeyDown = Input.GetMouseButtonDown(0); }


    //==================================================
    // 통합 입력
    //==================================================

    // 각종 입력 대응
    // WASD || ↑↓←→
    // Jump(Space Bar)
    // AttackAction(좌클릭)
    protected void SetInput()
    { InputWASD(); InputJump(); InputAttack(); }
}
