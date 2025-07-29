using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BetweenCubeAction : MonoBehaviour
{
    [SerializeField] private int sandwichedDamage = 3;



    // 콜라이더가 접촉해온 방향
    private enum ImpactDirection
    {
        Top, Bottom,
        Left, Right,
        Forward, Back
    }

    // ImpactDirection의 반대 방향을 반환하는 함수
    private ImpactDirection GetOppositeDirection(ImpactDirection dir)
    {
        switch (dir)
        {
            case ImpactDirection.Top: return ImpactDirection.Bottom;
            case ImpactDirection.Bottom: return ImpactDirection.Top;
            case ImpactDirection.Left: return ImpactDirection.Right;
            case ImpactDirection.Right: return ImpactDirection.Left;
            case ImpactDirection.Forward: return ImpactDirection.Back;
            case ImpactDirection.Back: return ImpactDirection.Forward;
            default: return dir; // <- 존재X
        }
    }


    // 콜라이더와, 그 콜라이더가 접촉한 방향
    private Dictionary<Collider, HashSet<ImpactDirection>> collisionDirections = new Dictionary<Collider, HashSet<ImpactDirection>>();


    // collisionDirections에 반대 방향이 있는지 확인하는 함수
    //  true: 반대 방향 O
    // false: 반대 방향 X
    private bool HasOppositeDirection(ImpactDirection dir)
    {
        ImpactDirection opposite = GetOppositeDirection(dir);

        foreach (var dirSet in collisionDirections.Values)
        {
            if (dirSet.Contains(opposite))
            { return true; }
        }
        return false;
    }


    // 어디에서부터 충돌했는지 방향 구하기
    private ImpactDirection GetDirectionFromWhereToThis(Vector3 normal)
    {
        float absX = Mathf.Abs(normal.x);
        float absY = Mathf.Abs(normal.y);
        float absZ = Mathf.Abs(normal.z);

        if (absX > absY && absX > absZ)
        { return normal.x > 0 ? ImpactDirection.Right : ImpactDirection.Left; }

        if (absY > absZ)
        { return normal.y > 0 ? ImpactDirection.Top : ImpactDirection.Bottom; }

        return normal.z > 0 ? ImpactDirection.Forward : ImpactDirection.Back;
    }

    private void OnCollisionEnter(Collision collision)
    { EnterUpdate(collision); }

    private void OnCollisionExit(Collision collision)
    { ExitUpdate(collision); }


    // 콜라이더 접촉 시 추가 및 이벤트
    private void EnterUpdate(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Cube")) { return; }

        Collider otherCollider = collision.collider;

        // --- 존재하지 않던 콜라이더인지 체크 ---
        if (!collisionDirections.ContainsKey(otherCollider))
        { collisionDirections[otherCollider] = new HashSet<ImpactDirection>(); }

        // --- 접촉한 부위들 체크 ---
        foreach (ContactPoint contact in collision.contacts)
        {
            ImpactDirection dir = GetDirectionFromWhereToThis(contact.normal);
            collisionDirections[otherCollider].Add(dir);
        }

        // --- 전체 충돌 콜라이더를 순회하며 반대 방향 존재 여부를 체크 ---
        foreach (var kvp in collisionDirections)
        {
            foreach (var dir in kvp.Value)
            {
                if (HasOppositeDirection(dir))
                {
                    Debug.Log("충돌 발생");

                    // 자해로 데미지 처리
                    DamageReaction damageReaction = GetComponent<DamageReaction>();
                    Actor actor = GetComponent<Actor>();
                    if (damageReaction != null)
                    { damageReaction.TakeDamage(sandwichedDamage, actor); }

                    // 텔레포트
                    // <-

                    return; // 중복 처리 방지를 위해 빠져나오기
                }
            }
        }

    }


    // 접촉해있었던 콜라이더 제거
    private void ExitUpdate(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Cube")) { return; }

        Collider otherCollider = collision.collider;
        if (collisionDirections.ContainsKey(otherCollider))
        {
            collisionDirections.Remove(otherCollider);
        }
    }


    private void Update()
    {
        // --- null key 체크 ---
        foreach (var key in collisionDirections.Keys.ToList())
        {
            if (key == null)
            { collisionDirections.Remove(key); }
        }
    }


}