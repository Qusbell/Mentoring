using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CubeGlue : MonoBehaviour
{
    private void Start()
    {
        beforePos = this.transform.position;
    }


    private void OnCollisionStay(Collision collision)
    {
        Rigidbody rigid = collision.gameObject.GetComponent<Rigidbody>();
        if (rigid == null) { return; }

        rigid.MovePosition(rigid.position + direction);
    }


    // 이전 위치
    Vector3 beforePos;

    // 현재 방향
    Vector3 direction;



    private void FixedUpdate()
    {
        direction = GetDirection();
        beforePos = this.transform.position;
    }


    private Vector3 GetDirection()
    {
        return this.transform.position - beforePos;
    }
}
