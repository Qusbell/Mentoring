using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] protected Transform target; // 따라갈 타겟(플레이어)
    [SerializeField] protected Vector3 offset;

    void LateUpdate()
    {
        if (target != null)
        {
            // 위치 이동
            transform.position = target.position + offset;
            // 타겟을 항상 바라보게
            transform.LookAt(target.position);
        }
    }
}