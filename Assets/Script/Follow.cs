using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    // 따라갈 대상
    [SerializeField] private Transform target;
    // 대상에서의 위치
    [SerializeField] private Vector3 offset;



    // Update is called once per frame
    void Update()
    {
        // 이 오브젝트(카메라)의 위치는
        // 대상으로부터 offeset만큼 떨어진 위치
        transform.position = target.position + offset;
    }
}
