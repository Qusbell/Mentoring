using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollider : MonoBehaviour
{
    private void Awake()
    {
        Collider collider = GetComponent<Collider>();

        if(collider == null)
        { Debug.LogError($"{gameObject.name}에 착지 판정용 콜라이더 부재"); }

        if (collider != null)
        { collider.isTrigger = true; }
    }


    // 현재 접촉 중인 지형들
    private HashSet<Collider> rands = new HashSet<Collider>();
    public bool isRand
    {
        get
        {
            return 0 < rands.Count;
        }
    }


    // 착지 판정 시 이벤트
    public List<System.Action> whenGroundEvent { get; set; } = new List<System.Action>();

    private void OnTriggerEnter(Collider other)
    {
        // 큐브인 경우
        if (other.CompareTag("Cube"))
        {
            // Debug.Log("착지!");
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
            // 일시적인 유예시간 후, 착지 중인 콜라이더 목록에서 삭제
            Timer.Instance.StartTimer(this, 0.05f, () => { rands.Remove(other); });
        }
    }
}