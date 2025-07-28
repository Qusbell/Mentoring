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
        get { return 0 < rands.Count; }
    }


    // 착지 판정 시 이벤트
    public List<System.Action> whenGroundEvent { get; set; } = new List<System.Action>();

    private void OnTriggerEnter(Collider other)
    {
        // 큐브인 경우
        if (other.CompareTag("Cube"))
        {
            Debug.Log("착지!");
            rands.Add(other);

            // 이벤트 일괄 발생
            foreach (System.Action groundEvent in whenGroundEvent.ToArray())
            { groundEvent?.Invoke(); }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        // 큐브인 경우
        if (other.CompareTag("Cube"))
        {
            rands.Remove(other);
        }
    }
}