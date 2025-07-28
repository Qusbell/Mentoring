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
            // 1차 검사: 닿아있는 지형 갯수
            if (0 < rands.Count)
            { return true; }

            // 2차 검사: 짧은 레이캐스트
            else
            {
                // 트리거 충돌 중이 아니더라도, 바로 아래에 지형이 있는지 레이캐스트로 보조 판정
                RaycastHit hit;
                float rayDistance = 0.25f; // 발바닥 기준 y 아래로 얼마만큼 체크할지
                bool hitBool = Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance);

                // 디버그용 : 레이 표시 (씬 뷰에서만 보임)
                // Debug.DrawRay(transform.position, Vector3.down * rayDistance, Color.red, 5f);

                if (hitBool && hit.collider.CompareTag("Cube"))
                { return true; }
                else
                { return false; }
            }
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