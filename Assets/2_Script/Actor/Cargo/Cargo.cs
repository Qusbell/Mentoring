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

    // <- ������ ���� distance
    [SerializeField] protected int distance = 3;
    
    // ���� ������ �̺�Ʈ
    public Action setNextDestination;

    // ������ ���� ��, ���� ������ ��߱��� �ð�
    [SerializeField] protected float nextStartTimer = 2f;


    protected override void Awake()
    {
        base.Awake();
        // �ٿ�ĳ��Ʈ
        cargoMoveAction = moveAction as CargoMoveAction;
        if (cargoMoveAction == null)
        { Debug.Log("CargoMoveAction �Ҵ���� ���� : " + gameObject.name); }
    }



    // ������ ����
    public void SetDestination(Transform destination)
    { cargoMoveAction.SetTarget(destination); }


    private void Update()
    {
        // ����� �ܼ� �̵��� �ϵ��� ����
        cargoMoveAction.Move();

        // ������ ���� ��
        // ���� ������ ����
        if (cargoMoveAction.InDistance(distance))
        { setNextDestination(); } // <- ���� �ð� �� ��ŸƮ
    }



}