using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(DamageReaction))]
[RequireComponent(typeof(CargoMoveAction))]
// 호위 큐브 (화물)
public class Cargo : Actor
{
    // 이동 메커니즘
    CargoMoveAction cargoMoveAction;

    // <- 목적지 도착 distance
    [SerializeField] protected int distance = 3;
    
    // 다음 목적지 이벤트
    public Action setNextDestination;

    // 목적지 도착 후, 다음 목적지 출발까지 시간
    [SerializeField] protected float nextStartTimer = 2f;


    protected override void Awake()
    {
        base.Awake();
        // 다운캐스트
        cargoMoveAction = moveAction as CargoMoveAction;
        if (cargoMoveAction == null)
        { Debug.Log("CargoMoveAction 할당되지 않음 : " + gameObject.name); }
    }



    // 목적지 설정
    public void SetDestination(Transform destination)
    { cargoMoveAction.SetTarget(destination); }


    private void Update()
    {
        // 현재는 단순 이동만 하도록 설정
        cargoMoveAction.Move();

        // 목적지 도착 시
        // 다음 목적지 설정
        if (cargoMoveAction.InDistance(distance))
        { setNextDestination(); } // <- 일정 시간 후 스타트
    }



}