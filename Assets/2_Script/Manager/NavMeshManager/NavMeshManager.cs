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
        surface.collectObjects = CollectObjects.Children; // 자식 오브젝트만 맵 생성
        surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders; // 콜라이더 기반 맵 생성
        //   surface.layerMask = LayerMask.GetMask("Cube"); // Cube 레이어만 맵 생성
    }
    private void Start()
    { Rebuild(); }
    // 지형 갱신
    public void Rebuild()
    {
        surface.BuildNavMesh();
    }
}