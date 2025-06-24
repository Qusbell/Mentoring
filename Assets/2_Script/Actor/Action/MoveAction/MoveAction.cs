using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



//==================================================
// 이동 행동
// moveVec을 외부에서 입력하면, moveSpeed의 속도로 이동
//==================================================
[RequireComponent(typeof(Rigidbody))]
public class MoveAction : MonoBehaviour
{
    // 오브젝트에 대한 물리효과
    protected Rigidbody rigid;

    // 초기화
    protected virtual void Awake()
    {
        // Rigidbody 초기화
        rigid = GetComponent<Rigidbody>();
    }
    

    //==================================================
    // 이동 메서드
    //==================================================

    // 이동할 방향
    public Vector3 moveVec { get; set; }

    // 이동 속도
    [SerializeField] protected float moveSpeed = 5;

    // 이동 상태 여부
    public bool isMove { get; set; } = false;


    // 이동 메서드
    // 현재위치 += 방향 * 이동 간격 * 이동 간격 보정
    public virtual void Move()
    { rigid.MovePosition(rigid.position + moveVec * moveSpeed * Time.deltaTime); }

    // 회전
    protected virtual void Turn()
    { transform.LookAt(transform.position + moveVec); }
}