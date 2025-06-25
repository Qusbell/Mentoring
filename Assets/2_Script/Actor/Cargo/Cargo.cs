using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[RequireComponent(typeof(DamageReaction))]
[RequireComponent(typeof(CargoMoveAction))]
// ȣ�� ť�� (ȭ��)
public class Cargo : Actor
{
    // �̵� ��Ŀ����
    CargoMoveAction cargoMoveAction;

    // ������ ������ ������ distance
    [SerializeField] protected int distance = 2;

    // ������ ���� ��, ���� ������ ��߱��� �ð� // <- �̱���
    [SerializeField] protected float nextStartTimer = 2f;

    // ���� ������ �̺�Ʈ
    public Action setNextDestination;


    protected override void Awake()
    {
        base.Awake();
        // �ٿ�ĳ��Ʈ
        cargoMoveAction = moveAction as CargoMoveAction;
        if (cargoMoveAction == null)
        { Debug.Log("CargoMoveAction �Ҵ���� ���� : " + gameObject.name); }
    }


    // ������ ����
    // CargoDestinationManager::SetNextDestination()���� �ʿ�
    public void SetDestination(Transform destination)
    { cargoMoveAction.SetTarget(destination); }


    private void Update()
    {
        // ������ ���� ��
        // ���� ������ ����
        if (cargoMoveAction.InDistance(distance))
        { setNextDestination(); }

        // �������� ���� ��� : Move
        else
        { cargoMoveAction.Move(); }
    }



}