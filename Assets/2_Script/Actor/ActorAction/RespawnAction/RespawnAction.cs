using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RespawnAction : ActorAction
{
    protected override void Awake()
    {
        base.Awake();

        // 큐브 레이어만 타겟
        layerMask = LayerMask.GetMask("Cube");
    }

    // 방문한 큐브
    private HashSet<Collider> visitedCubes = new HashSet<Collider>();

    // 검사할 레이어
    protected int layerMask;


    // 안전한 위치 탐색 및 복귀
    protected bool TryReturnToSafePos(float radius = 10f)
    {
        Vector3 centerPos = thisActor.lastestRandedPos;

        // --- 큐브 탐색 ---
        Collider[] cubes = Physics.OverlapSphere(centerPos, radius, layerMask);

        // --- 적절한 큐브 탐색 ---
        var newCubes = cubes.Where(c => !visitedCubes.Contains(c))
                            .OrderBy(c => Vector3.Distance(centerPos, c.transform.position))
                            .ToList();

        foreach (Collider cube in newCubes)
        {
            visitedCubes.Add(cube);   // 방문 처리

            CubeCollapser collapser = cube.GetComponent<CubeCollapser>();
            if (collapser != null && collapser.IsSafe)
            {
                // 위치 변경
                this.transform.position = collapser.transform.position + new Vector3(0, 5, 0);
                // 벡터값 zero
                thisActor.rigid.velocity = Vector3.zero;
                // 방문 큐브 클리어
                visitedCubes.Clear();
                return true; // 복귀 성공
            }
        }

        // 복귀 실패
        return false;
    }


    public void ReturnToSafePos()
    {
        float radius = 10f;

        while (radius <= 1000f)
        {
            if (TryReturnToSafePos(radius))
            {
                break;
            }

            else
            { radius *= 2; }
        }
    }


}
