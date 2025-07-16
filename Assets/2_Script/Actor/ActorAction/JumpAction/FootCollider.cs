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

    // 착지 판정
    public System.Action ground;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Cube")
        { ground?.Invoke(); }
    }
}