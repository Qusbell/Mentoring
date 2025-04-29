using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] protected Transform target; // 따라갈 타겟(플레이어)
    [SerializeField] protected Vector3 offset;
    [SerializeField] protected Vector3 rotation;
    
    void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(rotation);
    }
}