using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 큐브 이동 + 글루 기능 통합 (간단 버전)
/// CubeGlueActivator를 자동으로 추가하여 글루 활성화/비활성화 관리
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

    // 외부에서 확인할 수 있는 프로퍼티들
    public bool IsCurrentlyMoving => isMovingToOriginal && !hasArrived;
    public bool HasArrived => hasArrived;

    // 글루 상태 (CubeGlueActivator에서 제어)
    public bool IsGlueEnabled { get; set; } = false;

    // 이동 관련 변수들
    private Vector3 originalPosition;
    private Vector3 startPosition;
    private Vector3 previousPosition;
    private bool isMovingToOriginal;
    private bool hasArrived;

    // 글루 관련 변수들
    private List<Rigidbody> attachedRigidbodies = new List<Rigidbody>();

    void Awake()
    {
        originalPosition = transform.position;
        startPosition = originalPosition + startPositionOffset;
        previousPosition = transform.position;

        // CubeGlueActivator 자동 추가
        if (GetComponent<CubeGlueActivator>() == null)
        {
            gameObject.AddComponent<CubeGlueActivator>();
        }

#if UNITY_EDITOR
        SetupLaserRenderer();
#endif

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        transform.position = startPosition;
        previousPosition = startPosition;
        isMovingToOriginal = true;
        hasArrived = false;
        IsGlueEnabled = false;

#if UNITY_EDITOR
        UpdateLaserPath();
#endif
    }

    void Update()
    {
        if (isMovingToOriginal && !hasArrived)
        {
            // 이전 위치 저장
            previousPosition = transform.position;

            // 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                originalPosition,
                moveSpeed * Time.deltaTime
            );

            // 글루가 활성화되어 있으면 붙어있는 오브젝트들을 함께 이동
            if (IsGlueEnabled)
            {
                Vector3 movement = transform.position - previousPosition;
                MoveAttachedObjects(movement);
            }

            // 도착 확인
            if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
            {
                transform.position = originalPosition;
                hasArrived = true;

                // 정지 시 글루 관련 모든 것 비활성화
                DisableAllGlue();

                // NavMesh 리빌드
                if (NavMeshManager.instance != null)
                {
                    NavMeshManager.instance.Rebuild();
                }
            }

#if UNITY_EDITOR
            UpdateLaserPath();
#endif
        }
    }

    // 충돌 시 글루 대상 추가 (CubeGlueActivator에서 호출)
    void OnCollisionStay(Collision collision)
    {
        if (!IsGlueEnabled || !IsCurrentlyMoving) return;

        Rigidbody rb = collision.rigidbody;
        if (rb != null && !attachedRigidbodies.Contains(rb))
        {
            attachedRigidbodies.Add(rb);
        }
    }

    // 붙어있는 오브젝트들을 함께 이동
    private void MoveAttachedObjects(Vector3 movement)
    {
        if (movement.magnitude < 0.001f) return;

        for (int i = attachedRigidbodies.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = attachedRigidbodies[i];
            if (rb != null && rb.gameObject.activeInHierarchy)
            {
                rb.MovePosition(rb.position + movement);
            }
            else
            {
                attachedRigidbodies.RemoveAt(i);
            }
        }
    }

    // 모든 글루 기능 비활성화 (정지 시 호출)
    private void DisableAllGlue()
    {
        IsGlueEnabled = false;
        attachedRigidbodies.Clear();

        // CubeGlueActivator도 비활성화
        CubeGlueActivator activator = GetComponent<CubeGlueActivator>();
        if (activator != null)
        {
            activator.enabled = false;
        }
    }

    // 글루 활성화 (CubeGlueActivator에서 호출)
    public void EnableGlue()
    {
        if (IsCurrentlyMoving)
        {
            IsGlueEnabled = true;
        }
    }

    // 글루 비활성화 (CubeGlueActivator에서 호출)
    public void DisableGlue()
    {
        IsGlueEnabled = false;
        attachedRigidbodies.Clear();
    }

    public void Reset()
    {
        isMovingToOriginal = false;
        hasArrived = false;
        DisableAllGlue();

#if UNITY_EDITOR
        UpdateLaserPath();
#endif
    }

#if UNITY_EDITOR
    [Tooltip("에디터에서만 레이저 효과로 경로 표시")]
    public bool showLaserPath = true;

    private LineRenderer pathLaser;

    private void SetupLaserRenderer()
    {
        pathLaser = GetComponent<LineRenderer>();
        if (pathLaser == null && showLaserPath)
        {
            pathLaser = gameObject.AddComponent<LineRenderer>();
            pathLaser.positionCount = 2;
            pathLaser.material = new Material(Shader.Find("Sprites/Default"));
            pathLaser.startWidth = 0.1f;
            pathLaser.endWidth = 0.1f;
            pathLaser.startColor = Color.blue;
            pathLaser.endColor = Color.blue;
        }
        UpdateLaserPath();
    }

    private void UpdateLaserPath()
    {
        if (pathLaser != null && showLaserPath)
        {
            pathLaser.enabled = true;

            if (isMovingToOriginal && !hasArrived)
            {
                pathLaser.SetPosition(0, transform.position);
                pathLaser.SetPosition(1, originalPosition);
            }
            else if (hasArrived)
            {
                pathLaser.enabled = false;
            }
            else
            {
                pathLaser.SetPosition(0, startPosition);
                pathLaser.SetPosition(1, originalPosition);
            }
        }
        else if (pathLaser != null)
        {
            pathLaser.enabled = false;
        }
    }

    void OnDrawGizmos()
    {
        if (!showPath) return;

        Vector3 startPos, endPos;

        if (Application.isPlaying)
        {
            startPos = originalPosition + startPositionOffset;
            endPos = originalPosition;
        }
        else
        {
            startPos = transform.position + startPositionOffset;
            endPos = transform.position;
        }

        // 경로 선
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f);
        Gizmos.DrawLine(startPos, endPos);

        // 시작점/끝점
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(startPos, 0.1f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(endPos, 0.1f);

        // 화살표
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 arrowPos = Vector3.Lerp(startPos, endPos, 0.5f);
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.2f;
        Vector3 left = -right;
        Vector3 back = -direction * 0.4f;

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
        Gizmos.DrawLine(arrowPos, arrowPos + back + right);
        Gizmos.DrawLine(arrowPos, arrowPos + back + left);
        Gizmos.DrawLine(arrowPos + back + right, arrowPos + back + left);
    }
#endif
}