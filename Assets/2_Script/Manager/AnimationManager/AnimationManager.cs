using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    // 애니메이터
    protected Animator animator;

    // 초기화
    protected virtual void Awake()
    { animator = GetComponent<Animator>(); }


    protected virtual void LateUpdate()
    {
        // 애니메이션 적용
        SetAnimation();
    }


    protected virtual void SetAnimation()
    { }
}