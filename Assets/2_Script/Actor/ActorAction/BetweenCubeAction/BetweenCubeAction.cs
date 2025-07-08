using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BetweenCubeAction : MonoBehaviour
{
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
    private Dictionary<Collider, ImpactDirection> collisionDirections = new Dictionary<Collider, ImpactDirection>();

    // collisionDirections에 반대 방향이 있는지 확인하는 함수
    //  true: 반대 방향 O
    // false: 반대 방향 X
    private bool HasOppositeDirection(ImpactDirection dir)
    {
        ImpactDirection opposite = GetOppositeDirection(dir);

        foreach (var value in collisionDirections.Values)
        { if (value == opposite) { return true; } }

        return false;
    }



    // 어디에서부터 충돌했는지 방향 구하기
    private ImpactDirection GetDirectionFromWhereToThis(Vector3 normal)
    {
        float absX = Mathf.Abs(normal.x);
        float absY = Mathf.Abs(normal.y);
        float absZ = Mathf.Abs(normal.z);

        // X축이 가장 큰 경우
        if (absX > absY && absX > absZ)
        { return normal.x > 0 ? ImpactDirection.Left : ImpactDirection.Right; }

        // Y축이 가장 큰 경우
        if (absY > absZ)
        { return normal.y > 0 ? ImpactDirection.Bottom : ImpactDirection.Top; }

        // Z축이 가장 큰 경우 (기본값)
        return normal.z > 0 ? ImpactDirection.Back : ImpactDirection.Forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        EnterUpdate(collision);
        // <- 상반되는 방향 확인
    }

    private void OnCollisionExit(Collision collision)
    { ExitUpdate(collision); }


    // 콜라이더 접촉 시 추가
    private void EnterUpdate(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Collider otherCollider = contact.otherCollider;
    
            // 키 추가
            ImpactDirection dir = GetDirectionFromWhereToThis(contact.normal);

            Debug.Log("between 분리 : " + collision.gameObject.name + " " + dir);

            if (HasOppositeDirection(dir))
            {
                // <- 텔포
            }
            else
            { collisionDirections[otherCollider] = dir; }
        }
    }



    // 접촉해있었던 콜라이더 제거
    private void ExitUpdate(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Collider otherCollider = contact.otherCollider;

            // 키가 있으면 제거
            collisionDirections.Remove(otherCollider);

            Debug.Log("between 분리 : " + collision.gameObject.name);
        }
    }
}