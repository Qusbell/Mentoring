using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollider : MonoBehaviour
{
    private void Awake()
    {
        Collider collider = GetComponent<Collider>();

        // <- null인 경우

        if (collider != null)
        { collider.isTrigger = true; }
    }

    // 착지 판정으로 정정
    public System.Action ground;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cube" ||
            other.gameObject.layer == LayerMask.NameToLayer("Cube")) // <- 나중에 레이어 체크는 제거해도 상관없지 않나?
        { ground?.Invoke(); }
    }
}