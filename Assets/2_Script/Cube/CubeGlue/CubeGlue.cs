using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeGlue : MonoBehaviour
{
    private void Start()
    {
        beforePos = this.transform.position;
    }

    // 콜라이더가 접촉해있는 동안
    private void OnCollisionStay(Collision collision)
    { 
        // Player, Monster, Item 태그 허용
        if (!collision.transform.CompareTag("Player") &&
            !collision.transform.CompareTag("Monster") &&
            !collision.transform.CompareTag("Item"))
        { return; }

        // 접촉한 콜라이더 오브젝트에
        // RigidBody가 있다면
        Rigidbody rigid = collision.gameObject.GetComponent<Rigidbody>();
        if (rigid == null) { return; }

        // 해당 오브젝트를 이동시킨다
        // 어디로?
        rigid.MovePosition(rigid.position + direction);
    }

    // 이전 위치
    Vector3 beforePos;
    // 현재 방향
    Vector3 direction;

    // 매 물리 연산마다 업데이트
    private void FixedUpdate()
    {
        // 현재 향하고 있는 방향
        direction = GetDirection();
        // 현재 위치를 저장해둠
        // 다음 업데이트에서는 과거 위치가 되겠지
        beforePos = this.transform.position;
    }

    // 이전 위치 - 현재 위치
    // 이러면 Vector3값으로 현재 향하고 있는 방향이 나옴
    private Vector3 GetDirection()
    { return this.transform.position - beforePos; }
}