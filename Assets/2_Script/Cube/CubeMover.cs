using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

// 1. 중복된 의미의 변수, 메서드, 프로퍼티 통일
// 쉽게 말해서 하나만 써라

// 2. 업데이트에 들어가있는 함수들을 사용이 끝나면 아예 this.enabled = false로 만들어버린다던가
// 아니면 애초에 조건 만족 시 딱 1번만 실행하게 만든다던가
// 또는 프로퍼티로 호출 시에 체크한다던가
// Trigger 컴포넌트 <-> Work 컴포넌트 분리  


// 3. 디버그를 업데이트를 넣을때에는 확실히 이게 안되는 부분만 써라

// 4. 상속구조를 한번 활용해봐라 


/// <summary>
/// 큐브 이동을 관리하는 컴포넌트
/// 미리 배치된 큐브가 시작 시 꺼지고, 활성화될 때 지정한 위치에서 시작하여 원래 배치된 위치로 돌아옴
/// 이동 경로를 레이저로 시각화 (에디터에서만)
/// </summary>
public class CubeMover : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("시작 위치 (배치된 위치 기준으로 더해짐)")]
    public Vector3 startPositionOffset = new Vector3(10, 0, 0);

    [Tooltip("이동 속도 (초당 유닛)")]
    public float moveSpeed = 3f;

    [Header("시각화 설정")]
    [Tooltip("씬에서 이동 경로 시각화")]
    public bool showPath = true;

    // 이동 상태를 외부에서 확인할 수 있는 프로퍼티 (WarningSystem에서 사용)
    public bool IsCurrentlyMoving
    {
        get { return isMovingToOriginal && !hasArrived; }
    }

    // 도착 여부를 외부에서 확인할 수 있는 프로퍼티
    public bool HasArrived
    {
        get { return hasArrived; }
    }

    // 비공개 변수들
    private Vector3 originalPosition;      // 처음 배치된 위치
    private Vector3 startPosition;         // 계산된 시작 위치
    private bool isMovingToOriginal;       // 원래 위치로 이동 중
    private bool hasArrived;               // 원래 위치에 도착했는지 여부

    // 시작 시 초기화
    void Awake()
    {
        originalPosition = transform.position;
        startPosition = originalPosition + startPositionOffset;

#if UNITY_EDITOR
        // 에디터에서만 LineRenderer 설정
        SetupLaserRenderer();
#endif

        // 시작 시 비활성화
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            ChangeLayersRecursively(this.transform, LayerMask.NameToLayer("Default"));
        }
    }

    // 활성화될 때 호출됨
    void OnEnable()
    {
        // 시작 위치로 이동
        transform.position = startPosition;

        // 이동 시작
        isMovingToOriginal = true;
        hasArrived = false;

#if UNITY_EDITOR
        // 에디터에서만 레이저 경로 업데이트
        UpdateLaserPath();
#endif
    }

    // 매 프레임마다 실행
    void Update()
    {
        // 이미 도착했으면 컴포넌트 비활성화 (성능 최적화)
        if (hasArrived)
        {
            this.enabled = false;
            return;
        }

        // 원래 위치로 이동 중일 때
        if (isMovingToOriginal && !hasArrived)
        {
            // 현재 위치에서 목표 위치로 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                originalPosition,
                moveSpeed * Time.deltaTime
            );

            // 목표 위치에 도달했는지 확인
            if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
            {
                transform.position = originalPosition;  // 정확한 위치로 설정
                hasArrived = true;                      // 도착 상태로 변경
                ChangeLayersRecursively(this.transform, LayerMask.NameToLayer("Cube"));

                // NavMesh 리빌드-이동 끝 발판 생성
                if (NavMeshManager.instance != null) // null 체크 추가 (안정성)
                {
                    NavMeshManager.instance.Rebuild();
                }

                // 이동 완료 후 컴포넌트 비활성화
                this.enabled = false;
            }

#if UNITY_EDITOR
            // 에디터에서만 레이저 경로 업데이트
            UpdateLaserPath();
#endif
        }
    }

    // 레이어 변경 재귀 함수 (임시, 확인용)
    private void ChangeLayersRecursively(Transform trans, int layer)
    {
        trans.gameObject.layer = layer;
        foreach (Transform child in trans)
        {
            ChangeLayersRecursively(child, layer);
        }
    }

    // 큐브 초기화 (재사용 목적)
    public void Reset()
    {
        isMovingToOriginal = false;
        hasArrived = false;
        this.enabled = true; // 리셋 시 다시 활성화

#if UNITY_EDITOR
        // 에디터에서만 레이저 경로 업데이트
        UpdateLaserPath();
#endif
    }

#if UNITY_EDITOR
    [Tooltip("에디터에서만 레이저 효과로 경로 표시")]
    public bool showLaserPath = true;

    [Tooltip("에디터에서 경로 미리보기 (씬 뷰 전용)")]
    public bool showEditorPreview = true;

    // 레이저 경로용 LineRenderer (에디터 전용)
    private LineRenderer pathLaser;

    // 레이저 렌더러 설정 (에디터 전용)
    private void SetupLaserRenderer()
    {
        pathLaser = GetComponent<LineRenderer>();
        if (pathLaser == null && showLaserPath)
        {
            pathLaser = gameObject.AddComponent<LineRenderer>();

            // 레이저 기본 설정
            pathLaser.positionCount = 2; // 시작점과 끝점

            // 레이저의 재질 설정
            pathLaser.material = new Material(Shader.Find("Sprites/Default"));

            // 레이저 너비 설정
            pathLaser.startWidth = 0.1f;
            pathLaser.endWidth = 0.1f;

            // 레이저 색상 설정 (기본: 파란색)
            pathLaser.startColor = Color.blue;
            pathLaser.endColor = Color.blue;
        }

        UpdateLaserPath();
    }

    // 레이저 경로 업데이트 (에디터 전용)
    private void UpdateLaserPath()
    {
        if (pathLaser != null && showLaserPath)
        {
            pathLaser.enabled = true;

            // 현재 상태에 따라 레이저 경로 설정
            if (isMovingToOriginal && !hasArrived)
            {
                // 현재 위치에서 원래 위치까지
                pathLaser.SetPosition(0, transform.position);
                pathLaser.SetPosition(1, originalPosition);
            }
            else if (hasArrived)
            {
                // 도착 후에는 레이저 비활성화
                pathLaser.enabled = false;
            }
            else
            {
                // 정지 상태일 때는 전체 경로 표시
                pathLaser.SetPosition(0, startPosition);
                pathLaser.SetPosition(1, originalPosition);
            }
        }
        else if (pathLaser != null)
        {
            pathLaser.enabled = false;
        }
    }

    // 에디터에서 경로 미리보기 (씬 뷰에서만 표시)
    void OnDrawGizmos()
    {
        if (!showEditorPreview) return;

        // 원래 위치와 시작 위치 계산
        Vector3 startPos, endPos;

        if (Application.isPlaying)
        {
            // 실행 중일 때는 저장된 위치 사용
            startPos = originalPosition + startPositionOffset;
            endPos = originalPosition;
        }
        else
        {
            // 에디터에서는 현재 위치를 기준으로 계산
            startPos = transform.position + startPositionOffset;
            endPos = transform.position;
        }

        // 경로 선 그리기
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f); // 반투명 파란색
        Gizmos.DrawLine(startPos, endPos);

        // 시작점과 끝점에 작은 구체 표시
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // 반투명 초록색
        Gizmos.DrawSphere(startPos, 0.1f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // 반투명 빨간색
        Gizmos.DrawSphere(endPos, 0.1f);

        // 화살표 표시 (방향 표시)
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 arrowPos = Vector3.Lerp(startPos, endPos, 0.5f);

        // 화살표 헤드 그리기
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.2f;
        Vector3 left = -right;
        Vector3 back = -direction * 0.4f;

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // 반투명 노란색
        Gizmos.DrawLine(arrowPos, arrowPos + back + right);
        Gizmos.DrawLine(arrowPos, arrowPos + back + left);
        Gizmos.DrawLine(arrowPos + back + right, arrowPos + back + left);
    }
#endif
}