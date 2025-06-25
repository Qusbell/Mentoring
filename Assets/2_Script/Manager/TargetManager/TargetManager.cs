using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TargetManager : MonoBehaviour
{
    public static TargetManager instance { get; private set; }

    public Transform target { get; private set; }

    [Header("타겟 조건")]
    [SerializeField] private string targetTag = "Target";
    [SerializeField] private string targetLayerName = "Target";
    [SerializeField] private LayerMask targetLayerMask;


    void Awake()
    {
        if (instance == null) { instance = this; }
        else
        {
            Destroy(gameObject);
            Debug.Log("TargetManager 다수 존재");
            return;
        }
    }

    void Start()
    { Targeting(); }

    // 타겟팅
    public Transform Targeting()
    {
        // string으로 입력받은 레이어 이름을 int로 변환
        int targetLayer = LayerMask.NameToLayer(targetLayerName);

        // 만약 targetLayerMask가 지정되어 있지 않다면, targetLayerName을 기반으로 생성
        if (targetLayerMask.value == 0 && targetLayer >= 0)
        { targetLayerMask = 1 << targetLayer; }

        // 태그로 후보군을 먼저 찾음
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (var obj in candidates)
        {
            // LayerMask를 bit 연산으로 체크
            if (((1 << obj.layer) & targetLayerMask.value) != 0)
            {
                target = obj.transform;
                break;
            }
        }

        return target;
    }
}
