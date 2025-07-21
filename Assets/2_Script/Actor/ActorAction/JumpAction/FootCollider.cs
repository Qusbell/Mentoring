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

        // <- null인 경우?

        if (collider != null)
        { collider.isTrigger = true; }

        // rigid = GetComponentInParent<Rigidbody>();
    }

    // 착지 판정 시 액션
    public List<System.Action> ground { get; set; } = new List<System.Action>();

    private void OnTriggerEnter(Collider other)
    {
        // 큐브인 경우
        if (other.tag == "Cube")
        {
            foreach (System.Action action in ground.ToArray())
            { action?.Invoke(); }
        }

        // <- 몬스터인 경우?
        //  else if (other.tag == "Monster")
        //  {
        //      // 불필요한 물리 초기화
        //      //  rigid.velocity = Vector3.zero;
        //      //  rigid.AddForce(Vector3.up * 13, ForceMode.Impulse);
        //  }
    }
}