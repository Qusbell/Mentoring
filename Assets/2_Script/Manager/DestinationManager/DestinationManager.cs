using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// ȣ�� ť���� �������� ������� �Է¹���
public class DestinationManager : MonoBehaviour
{
    // ������ ���
    private List<Destination> destinations = new List<Destination>();

    // ���� �� ��° ������ ��������
    private int indexCount = 0;

    // ȣ�� ť��
    private Cargo cargo;

    

    // �� ť�� ��Ʈ�ѷ� �� ���� ����
    void Awake()
    {
        // ��� �ڽ� ������Ʈ�κ���
        // Destination ������Ʈ ���� �� ����
        GetComponentsInChildren<Destination>(true, destinations);
    }

    void Start()
    {
        // ȣ�� ť�긦 �����ͼ� ����
        Cargo[] cargos = FindObjectsOfType<Cargo>();
        foreach (var tempCargo in cargos)
        { cargo = tempCargo; }

        cargo.setNextDestination = SetNextDestination;
    }

    // �޼��� : ���� Destination���� target ����
    // if (indexCount < destinations.Count)
    // ȣ��ť��.target = destinations[indexCount++]
    void SetNextDestination()
    {
        if (indexCount < destinations.Count)
        { cargo.SetDestination(destinations[indexCount++].transform); }
        // <- else ���� ó��
    }

}
