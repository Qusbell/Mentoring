using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 큐브 이동을 관리하는 컴포넌트
/// 미리 배치된 큐브가 시작 시 꺼지고, 활성화될 때 지정한 위치에서 시작하여 원래 배치된 위치로 돌아옴
/// 이동 경로를 레이저로 시각화
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

    [Tooltip("레이저 효과로 경로 표시")]
    public bool showLaserPath = true;

    // 비공개 변수들
    private Vector3 originalPosition;      // 처음 배치된 위치
    private Vector3 startPosition;         // 계산된 시작 위치
    private bool isMovingToOriginal;       // 원래 위치로 이동 중
    private bool hasArrived;               // 원래 위치에 도착했는지 여부

    // 레이저 경로용 LineRenderer
    private LineRenderer pathLaser;

    // 시작 시 초기화
    void Awake()
    {
        originalPosition = transform.position;
        startPosition = originalPosition + startPositionOffset;

        // LineRenderer가 없으면 추가
        SetupLaserRenderer();

        // 시작 시 비활성화
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
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

        // 레이저 경로 업데이트
        UpdateLaserPath();
    }

    // 매 프레임마다 실행
    void Update()
    {
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
            }

            // 레이저 경로 업데이트
            UpdateLaserPath();
        }
    }

    // 레이저 렌더러 설정
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

    // 레이저 경로 업데이트
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

    // 큐브 초기화 (재사용 목적)
    public void Reset()
    {
        isMovingToOriginal = false;
        hasArrived = false;

        // 레이저 경로 업데이트
        UpdateLaserPath();
    }
}