using UnityEngine;
using System.Collections.Generic;


// CubeController끼리 연결시키는 커넥터
public class CubeControllerConnector : MonoBehaviour
{
    private List<CubeController> controllerSequence = new List<CubeController>();

    // 각 큐브 컨트롤러 간 관계 설정
    void Awake()
    {
        // 모든 자식 컨트롤러 수집 (비활성 포함)
        GetComponentsInChildren<CubeController>(true, controllerSequence);

        // 디버그: 찾은 컨트롤러 수 출력
        Debug.Log($"Found {controllerSequence.Count} controllers.");

        // <- (필요하다면) 순서 보정 (예: 이름 기준 정렬)
        // controllerSequence.Sort((a, b) => a.gameObject.name.CompareTo(b.gameObject.name));

        // 컨트롤러 연결
        for (int i = 0; i < controllerSequence.Count - 1; i++)
        {
            CubeController current = controllerSequence[i];
            CubeController next = controllerSequence[i + 1];

            // 다음 컨트롤러 참조 설정
            current.nextController = next; // <- 이벤트와 2중으로 연결되어 있음. 제거 고려 가능

            // 이벤트 연결
            current.nextCubeControllerActivate.AddListener(next.StartController);
        }

        if (controllerSequence[0] != null)
        { controllerSequence[0].StartController(); }
        else
        { Debug.Log("자식 오브젝트에 CubeController 컴포넌트 존재하지 않음."); }
    }
}
