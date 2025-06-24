using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.AI.Navigation;
using UnityEngine;


[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshManager : MonoBehaviour
{
    // 싱글톤
    public static NavMeshManager instance = null;

    // 네비게이션
    NavMeshSurface surface = null;

    private void Awake()
    { 
        // 싱글톤 초기화
        if (instance == null)
        { instance = this; }
        else { Destroy(this.gameObject); return; }

        // NavMeshSurface 설정
        surface = GetComponent<NavMeshSurface>();
        if(surface == null) { Debug.Log("NavMeshSurface가 존재하지 않음 : " + gameObject.name); }
        surface.collectObjects = CollectObjects.Children;

        Rebuild();
    }

    // 지형 갱신
    public void Rebuild()
    {
        surface.BuildNavMesh();
        StartCoroutine(Timer.StartTimer(0.1f, Rebuild)); // <- 테스트 : 0.1f마다 지형 갱신
    }
}