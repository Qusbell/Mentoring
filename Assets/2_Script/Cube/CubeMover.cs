using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ť�� �̵��� �����ϴ� ������Ʈ
/// �̸� ��ġ�� ť�갡 ���� �� ������, Ȱ��ȭ�� �� ������ ��ġ���� �����Ͽ� ���� ��ġ�� ��ġ�� ���ƿ�
/// �̵� ��θ� �������� �ð�ȭ
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

    [Tooltip("������ ȿ���� ��� ǥ��")]
    public bool showLaserPath = true;

    // ����� ������
    private Vector3 originalPosition;      // ó�� ��ġ�� ��ġ
    private Vector3 startPosition;         // ���� ���� ��ġ
    private bool isMovingToOriginal;       // ���� ��ġ�� �̵� ��
    private bool hasArrived;               // ���� ��ġ�� �����ߴ��� ����

    // ������ ��ο� LineRenderer
    private LineRenderer pathLaser;

    // ���� �� �ʱ�ȭ
    void Awake()
    {
        originalPosition = transform.position;
        startPosition = originalPosition + startPositionOffset;

        // LineRenderer�� ������ �߰�
        SetupLaserRenderer();

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

        // ������ ��� ������Ʈ
        UpdateLaserPath();
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

            // ������ ��� ������Ʈ
            UpdateLaserPath();
        }
    }

    // ������ ������ ����
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

    // ������ ��� ������Ʈ
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

    // ť�� �ʱ�ȭ (���� ����)
    public void Reset()
    {
        isMovingToOriginal = false;
        hasArrived = false;

        // ������ ��� ������Ʈ
        UpdateLaserPath();
    }
}