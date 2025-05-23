using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ť�� �̵��� �����ϴ� ������Ʈ
/// �̸� ��ġ�� ť�갡 ���� �� ������, Ȱ��ȭ�� �� ������ ��ġ���� �����Ͽ� ���� ��ġ�� ��ġ�� ���ƿ�
/// �̵� ��θ� �������� �ð�ȭ (�����Ϳ�����)
/// </summary>
public class CubeMover : MonoBehaviour
{
    [Header("�̵� ����")]
    [Tooltip("���� ��ġ (��ġ�� ��ġ �������� ������)")]
    public Vector3 startPositionOffset = new Vector3(10, 0, 0);

    [Tooltip("�̵� �ӵ� (�ʴ� ����)")]
    public float moveSpeed = 3f;

    [Header("�ð�ȭ ����")]
    [Tooltip("������ �̵� ��� �ð�ȭ")]
    public bool showPath = true;

#if UNITY_EDITOR
    [Tooltip("�����Ϳ����� ������ ȿ���� ��� ǥ��")]
    public bool showLaserPath = true;

    [Tooltip("�����Ϳ��� ��� �̸����� (�� �� ����)")]
    public bool showEditorPreview = true;
#endif

    // ����� ������
    private Vector3 originalPosition;      // ó�� ��ġ�� ��ġ
    private Vector3 startPosition;         // ���� ���� ��ġ
    private bool isMovingToOriginal;       // ���� ��ġ�� �̵� ��
    private bool hasArrived;               // ���� ��ġ�� �����ߴ��� ����

#if UNITY_EDITOR
    // ������ ��ο� LineRenderer (������ ����)
    private LineRenderer pathLaser;
#endif

    // ���� �̵� ������ Ȯ���ϴ� ������Ƽ (WarningSystem���� ���)
    public bool IsCurrentlyMoving
    {
        get { return isMovingToOriginal && !hasArrived; }
    }

    // ���� �� �ʱ�ȭ
    void Awake()
    {
        originalPosition = transform.position;
        startPosition = originalPosition + startPositionOffset;

#if UNITY_EDITOR
        // �����Ϳ����� LineRenderer ����
        SetupLaserRenderer();
#endif

        // ���� �� ��Ȱ��ȭ
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    // Ȱ��ȭ�� �� ȣ���
    void OnEnable()
    {
        // ���� ��ġ�� �̵�
        transform.position = startPosition;

        // �̵� ����
        isMovingToOriginal = true;
        hasArrived = false;

#if UNITY_EDITOR
        // �����Ϳ����� ������ ��� ������Ʈ
        UpdateLaserPath();
#endif
    }

    // �� �����Ӹ��� ����
    void Update()
    {
        // ���� ��ġ�� �̵� ���� ��
        if (isMovingToOriginal && !hasArrived)
        {
            // ���� ��ġ���� ��ǥ ��ġ�� �̵�
            transform.position = Vector3.MoveTowards(
                transform.position,
                originalPosition,
                moveSpeed * Time.deltaTime
            );

            // ��ǥ ��ġ�� �����ߴ��� Ȯ��
            if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
            {
                transform.position = originalPosition;  // ��Ȯ�� ��ġ�� ����
                hasArrived = true;                      // ���� ���·� ����
            }

#if UNITY_EDITOR
            // �����Ϳ����� ������ ��� ������Ʈ
            UpdateLaserPath();
#endif
        }
    }

#if UNITY_EDITOR
    // ������ ������ ���� (������ ����)
    private void SetupLaserRenderer()
    {
        pathLaser = GetComponent<LineRenderer>();
        if (pathLaser == null && showLaserPath)
        {
            pathLaser = gameObject.AddComponent<LineRenderer>();

            // ������ �⺻ ����
            pathLaser.positionCount = 2; // �������� ����

            // �������� ���� ����
            pathLaser.material = new Material(Shader.Find("Sprites/Default"));

            // ������ �ʺ� ����
            pathLaser.startWidth = 0.1f;
            pathLaser.endWidth = 0.1f;

            // ������ ���� ���� (�⺻: �Ķ���)
            pathLaser.startColor = Color.blue;
            pathLaser.endColor = Color.blue;
        }

        UpdateLaserPath();
    }

    // ������ ��� ������Ʈ (������ ����)
    private void UpdateLaserPath()
    {
        if (pathLaser != null && showLaserPath)
        {
            pathLaser.enabled = true;

            // ���� ���¿� ���� ������ ��� ����
            if (isMovingToOriginal && !hasArrived)
            {
                // ���� ��ġ���� ���� ��ġ����
                pathLaser.SetPosition(0, transform.position);
                pathLaser.SetPosition(1, originalPosition);
            }
            else if (hasArrived)
            {
                // ���� �Ŀ��� ������ ��Ȱ��ȭ
                pathLaser.enabled = false;
            }
            else
            {
                // ���� ������ ���� ��ü ��� ǥ��
                pathLaser.SetPosition(0, startPosition);
                pathLaser.SetPosition(1, originalPosition);
            }
        }
        else if (pathLaser != null)
        {
            pathLaser.enabled = false;
        }
    }
#endif

    // ť�� �ʱ�ȭ (���� ����)
    public void Reset()
    {
        isMovingToOriginal = false;
        hasArrived = false;

#if UNITY_EDITOR
        // �����Ϳ����� ������ ��� ������Ʈ
        UpdateLaserPath();
#endif
    }

#if UNITY_EDITOR
    // �����Ϳ��� ��� �̸����� (�� �信���� ǥ��)
    void OnDrawGizmos()
    {
        if (!showEditorPreview) return;

        // ���� ��ġ�� ���� ��ġ ���
        Vector3 startPos, endPos;

        if (Application.isPlaying)
        {
            // ���� ���� ���� ����� ��ġ ���
            startPos = originalPosition + startPositionOffset;
            endPos = originalPosition;
        }
        else
        {
            // �����Ϳ����� ���� ��ġ�� �������� ���
            startPos = transform.position + startPositionOffset;
            endPos = transform.position;
        }

        // ��� �� �׸���
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.5f); // ������ �Ķ���
        Gizmos.DrawLine(startPos, endPos);

        // �������� ������ ���� ��ü ǥ��
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f); // ������ �ʷϻ�
        Gizmos.DrawSphere(startPos, 0.1f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // ������ ������
        Gizmos.DrawSphere(endPos, 0.1f);

        // ȭ��ǥ ǥ�� (���� ǥ��)
        Vector3 direction = (endPos - startPos).normalized;
        Vector3 arrowPos = Vector3.Lerp(startPos, endPos, 0.5f);

        // ȭ��ǥ ��� �׸���
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.2f;
        Vector3 left = -right;
        Vector3 back = -direction * 0.4f;

        Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // ������ �����
        Gizmos.DrawLine(arrowPos, arrowPos + back + right);
        Gizmos.DrawLine(arrowPos, arrowPos + back + left);
        Gizmos.DrawLine(arrowPos + back + right, arrowPos + back + left);
    }
#endif
}