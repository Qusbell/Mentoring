using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 경고 표시
public class WarningPlainPool : SingletonT<WarningPlainPool>
{
    // 재사용할 발판들을 보관하는 풀
    private List<GameObject> warningPool = new List<GameObject>();


    private void Awake()
    {
        CreateWarningPlanes(10); // <- 10개 생성, 나중에 정정?
    }

    // 복수 생성
    private void CreateWarningPlanes(int num)
    {
        for(int i = 0; i < num; i++)
        { CreateWarningPlane(); }
    }

    // 단일 생성 및 반환
    private GameObject CreateWarningPlane()
    {
        // Unity 기본 평면(Quad) 생성
        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Quad);
        warning.name = "PooledWarning_" + warningPool.Count;
        // 비활성화해서 풀에 보관 (화면에 보이지 않음)
        warning.SetActive(false);
        warningPool.Add(warning);

        return warning;
    }


    // === public 메서드 ===

    // 오브젝트 풀 사용
    public GameObject GetWarningPlaneFromPool()
    {
        // 풀에서 비활성화된(사용 안 중인) 경고 표시 찾기
        foreach (GameObject warning in warningPool)
        {
            // 비활성화 상태 = 재사용 가능
            if (!warning.activeInHierarchy)
            { return warning; }
        }

        return CreateWarningPlane();
    }


    // 반환
    public void ReturnWarningPlaneToPool(GameObject warning)
    {
        if (warning != null)
        {
            Debug.Log($"{warning.name} 반환");
            warning.SetActive(false);
        }
    }


}
