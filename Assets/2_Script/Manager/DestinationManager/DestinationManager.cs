using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// 호위 큐브의 목적지를 순서대로 입력받음
public class DestinationManager : MonoBehaviour
{
    // 목적지 목록
    private List<Destination> destinations = new List<Destination>();

    // 현재 몇 번째 목적지 차례인지
    private int indexCount = 0;

    // 호위 큐브
    private Cargo cargo;

    

    // 각 큐브 컨트롤러 간 관계 설정
    void Awake()
    {
        // 모든 자식 오브젝트로부터
        // Destination 오브젝트 추출 후 저장
        GetComponentsInChildren<Destination>(true, destinations);
    }

    void Start()
    {
        // 호위 큐브를 가져와서 저장
        Cargo[] cargos = FindObjectsOfType<Cargo>();
        foreach (var tempCargo in cargos)
        { cargo = tempCargo; }

        cargo.setNextDestination = SetNextDestination;
    }

    // 메서드 : 다음 Destination으로 target 변경
    // if (indexCount < destinations.Count)
    // 호위큐브.target = destinations[indexCount++]
    void SetNextDestination()
    {
        if (indexCount < destinations.Count)
        { cargo.SetDestination(destinations[indexCount++].transform); }
        // <- else 종료 처리
    }

}
