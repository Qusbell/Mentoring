using UnityEngine;
using System.Collections;

/// <summary>
/// 멧돼지 이동 관리 (CubeMover 스타일)
/// </summary>
public class BoarMovement : MonoBehaviour
{
    private BoarCube main;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool isLaunching = false;

    public void Initialize(BoarCube mainComponent)
    {
        main = mainComponent;
        UpdatePositions(main.startPositionOffset);

        // 시작 위치로 이동
        transform.position = startPosition;
    }

    public void UpdatePositions(Vector3 offset)
    {
        Vector3 currentPosition = transform.position;
        endPosition = currentPosition;
        startPosition = currentPosition + offset;
    }

    public void Reset()
    {
        isLaunching = false;
        transform.position = startPosition;
    }

    public IEnumerator ExecuteLaunch()
    {
        isLaunching = true;

        Vector3 startPos = transform.position;
        float journey = 0f;
        float journeyLength = Vector3.Distance(startPos, endPosition);

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] 멧돼지 돌진 시작: {startPos} → {endPosition}");

        while (journey < journeyLength)
        {
            journey += main.launchSpeed * Time.deltaTime;
            float progress = journey / journeyLength;

            transform.position = Vector3.Lerp(startPos, endPosition, progress);

            // 넉백 시스템
            if (main.enableKnockback)
            {
                GetComponent<BoarKnockback>().CheckKnockback();
            }

            yield return null;
        }

        transform.position = endPosition;
        isLaunching = false;

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] 멧돼지 돌진 완료!");
    }
}