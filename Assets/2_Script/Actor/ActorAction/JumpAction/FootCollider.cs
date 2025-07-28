using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollider : MonoBehaviour
{
    private void Awake()
    {
        Collider collider = GetComponent<Collider>();

        // <- null인 경우?

        if (collider != null)
        { collider.isTrigger = true; }
    }


    // 현재 접촉 중인 지형들
    private List<Collider> rands = new List<Collider>();
    public bool isRand
    {
        get
        {
            return 0 < rands.Count;
        }
    }


    // 착지 판정 시 액션
    public List<System.Action> ground { get; set; } = new List<System.Action>();

    private void OnTriggerEnter(Collider other)
    {
        // 큐브인 경우
        if (other.tag == "Cube")
        {
            rands.Add(other);

            foreach (System.Action action in ground.ToArray())
            { action?.Invoke(); }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        // 큐브인 경우
        if (other.tag == "Cube")
        { rands.Remove(other); }
    }
}