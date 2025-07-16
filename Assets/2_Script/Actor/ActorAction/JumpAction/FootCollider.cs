using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollider : MonoBehaviour
{
    // Rigidbody rigid; // <- 몬스터 밟고 튀어오르기

    private void Awake()
    {
        Collider collider = GetComponent<Collider>();

        // <- null인 경우

        if (collider != null)
        { collider.isTrigger = true; }

        // rigid = GetComponentInParent<Rigidbody>();
    }

    // 착지 판정으로 정정
    public System.Action ground;

    private void OnTriggerEnter(Collider other)
    {
        // 큐브인 경우
        if (other.tag == "Cube" ||
            other.gameObject.layer == LayerMask.NameToLayer("Cube")) // <- 나중에 레이어 체크는 제거해도 상관없지 않나?
        { ground?.Invoke(); }

        // <- 몬스터인 경우?
        else if (other.tag == "Monster")
        {
            // 불필요한 물리 초기화
            //  rigid.velocity = Vector3.zero;
            //  rigid.AddForce(Vector3.up * 13, ForceMode.Impulse);
        }
    }
}