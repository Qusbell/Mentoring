using UnityEngine;
using System.Collections.Generic;

public class CubeControllerManager : MonoBehaviour
{
    private List<CubeController> controllerSequence = new List<CubeController>();

    void Awake()
    {
        // 모든 자식 컨트롤러 수집 (비활성 포함)
        GetComponentsInChildren<CubeController>(true, controllerSequence);

        // 디버그: 찾은 컨트롤러 수 출력
        Debug.Log($"Found {controllerSequence.Count} controllers.");

        // 순서 보정 (예: 이름 기준 정렬)
        //  controllerSequence.Sort((a, b) => a.gameObject.name.CompareTo(b.gameObject.name));

        // 컨트롤러 연결
        for (int i = 0; i < controllerSequence.Count - 1; i++)
        {
            CubeController current = controllerSequence[i];
            CubeController next = controllerSequence[i + 1];

            // 다음 컨트롤러 참조 설정
            current.nextController = next;
            // 이벤트 연결
            current.nextCubeControllerActivate.AddListener(next.StartController);
            Debug.Log($"[{current.name}] → [{next.name}] 연결 완료");
        }

        controllerSequence[0].StartController();
    }
}
