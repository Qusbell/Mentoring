using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    // �ִϸ�����
    protected Animator animator;

    // �ʱ�ȭ
    protected virtual void Awake()
    { animator = GetComponent<Animator>(); }


    protected virtual void LateUpdate()
    {
        // �ִϸ��̼� ����
        SetAnimation();
    }


    protected virtual void SetAnimation()
    { }
}