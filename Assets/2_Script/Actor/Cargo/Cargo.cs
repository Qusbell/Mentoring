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

    // 목적지 도착을 판정할 distance
    [SerializeField] protected int distance = 2;

    // 목적지 도착 후, 다음 목적지 출발까지 시간 // <- 미구현
    [SerializeField] protected float nextStartTimer = 2f;

    // 다음 목적지 이벤트
    public Action setNextDestination;


    protected override void Awake()
    {
        base.Awake();
        // 다운캐스트
        cargoMoveAction = moveAction as CargoMoveAction;
        if (cargoMoveAction == null)
        { Debug.Log("CargoMoveAction 할당되지 않음 : " + gameObject.name); }
    }


    // 목적지 설정
    // CargoDestinationManager::SetNextDestination()에서 필요
    public void SetDestination(Transform destination)
    { cargoMoveAction.SetTarget(destination); }


    private void Update()
    {
        // 목적지 도착 시
        // 다음 목적지 설정
        if (cargoMoveAction.InDistance(distance))
        { setNextDestination(); }

        // 도착하지 않은 경우 : Move
        else
        { cargoMoveAction.Move(); }
    }


}