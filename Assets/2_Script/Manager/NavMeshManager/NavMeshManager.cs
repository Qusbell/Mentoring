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

    // 전역 플래그: 몬스터가 스폰되었는지 확인 (추가됨)
    public static bool hasAnyMonsterSpawned = false;

    private void Awake()
    {
        // 싱글톤 초기화
        if (instance == null)
        { instance = this; }
        else { Destroy(this.gameObject); return; }

        // NavMeshSurface 설정
        surface = GetComponent<NavMeshSurface>();
        if (surface == null) { Debug.Log("NavMeshSurface가 존재하지 않음 : " + gameObject.name); }
        surface.collectObjects = CollectObjects.Children;
        // 콜라이더 기반 맵 생성
        surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
    }

    private void Start()
    {
        // 게임 시작 시 베이크 안 함 (수정됨)
        // Rebuild();
    }

    // 지형 갱신
    public void Rebuild()
    {
        surface.BuildNavMesh();
    }
}