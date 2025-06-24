using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ܼ�ȭ�� ť�� �ر� ������Ʈ
/// �÷��̾� ����, �ð� ���, �ܺ� Ʈ����, ������ Ʈ���ſ� ���� �ر��Ǵ� ť��
/// ������ Ʈ���� ��忡���� �ٸ� ť��鵵 �Բ� �ر� ����
/// </summary>
public class CubeCollapser : MonoBehaviour
{
    [Header("Ʈ���� ����")]
    [Tooltip("�ر� Ʈ���� ����")]
    public TriggerType triggerType = TriggerType.PlayerProximity;

    // Ʈ���� Ÿ�� ����
    public enum TriggerType
    {
        Time,            // �ð� ��� (���� �ð� �� �ر�)
        PlayerProximity, // �÷��̾� ����
        ExternalTrigger, // �ܺ� ȣ�⿡ ���� Ʈ����
        AreaTrigger      // ������ Ʈ���� (�ٸ� ť��鵵 �Բ� �ر�)
    }

    [Tooltip("�÷��̾� �±�")]
    public string playerTag = "Player";

    [Tooltip("�÷��̾� ���� Ʈ���� �Ÿ�")]
    public float triggerDistance = 0.1f;

    [Tooltip("�ر� �� ��� �ð� (��)")]
    public float warningDelay = 1f;

    [Header("������ Ʈ���� ���� (AreaTrigger ����)")]
    [Tooltip("�Բ� �ر���ų �ٸ� ť���")]
    public List<CubeCollapser> additionalCubes = new List<CubeCollapser>();

    [Tooltip("�� ť�� �� �ر� ���� (��, 0�̸� ���ÿ� �ر�)")]
    public float collapseInterval = 0.1f;

    [Tooltip("�� ���� Ʈ���ŵǴ��� ����")]
    public bool oneTimeUse = true;

    // ���� ���� ���� (Inspector���� ���� �Ұ�)
    private const float COLLAPSE_SPEED = 15f;         // �ر� �ӵ�
    private const float DEACTIVATE_DISTANCE = 10f;   // ��Ȱ��ȭ �Ÿ�
    private const float DEACTIVATE_TIME = 2f;        // ��Ȱ��ȭ �ð�
    private const float SHAKE_DURATION = 2.0f;       // ��鸲 ���� �ð�
    private const float INITIAL_SHAKE_INTENSITY = 0.05f; // �ʱ� ��鸲 ����
    private const float MAX_SHAKE_INTENSITY = 0.2f;  // �ִ� ��鸲 ����
    private const float SHAKE_SPEED = 5f;           // ��鸲 �ӵ�
    private const float SHAKE_ACCELERATION = 5.0f;   // ��鸲 ����ȭ ����

    // ť�� ���� ����
    private enum CubeState
    {
        Idle,       // ��� ����
        Shaking,    // ��鸲 ����
        Falling,    // �������� ����
        Collapsed   // �ر� �Ϸ�
    }

    // ���� ����
    private CubeState currentState = CubeState.Idle;
    private Transform playerTransform;
    private Vector3 originalPosition;
    private float currentShakeIntensity;
    private float fallenDistance = 0f;
    private float shakeTimer = 0f;
    private float sqrTriggerDistance;
    private bool hasTriggered = false; // ������ Ʈ���ſ�

    // ���� �� �ʱ�ȭ
    void Awake()
    {
        // ���� ��ġ ����
        originalPosition = transform.position;

        // �Ÿ� ��� ����ȭ�� ���� ������ �̸� ���
        sqrTriggerDistance = triggerDistance * triggerDistance;

        // ������ Ʈ���� ��忡�� �ݶ��̴� ����
        if (triggerType == TriggerType.AreaTrigger)
        {
            SetupAreaTrigger();
        }
    }

    void Start()
    {
        // �÷��̾� ã�� (�� ���� ����)
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // �ð� Ʈ������ ��� �ڵ����� �ر� ����
        if (triggerType == TriggerType.Time)
        {
            StartCoroutine(StartCollapseProcedure());
        }
    }

    // ������ Ʈ���ſ� �ݶ��̴� ����
    private void SetupAreaTrigger()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // �ݶ��̴��� ������ �ڽ� �ݶ��̴� �߰�
            col = gameObject.AddComponent<BoxCollider>();
            Debug.Log($"[{gameObject.name}] ������ Ʈ���ſ� BoxCollider�� �ڵ� �߰��Ǿ����ϴ�.");
        }

        // Ʈ���ŷ� ����
        col.isTrigger = true;
    }

    // �� ������ ���� 
    void Update()
    {
        // ���� ���¿� ���� ó��
        switch (currentState)
        {
            case CubeState.Idle:
                CheckPlayerProximity();
                break;

            case CubeState.Shaking:
                UpdateShaking();
                break;

            case CubeState.Falling:
                UpdateFalling();
                break;

            case CubeState.Collapsed:
                // �̹� �ر��� - �ƹ��͵� ���� ����
                break;
        }
    }

    // �÷��̾� ���� Ȯ��
    private void CheckPlayerProximity()
    {
        if (triggerType != TriggerType.PlayerProximity || playerTransform == null) return;

        float sqrDistance = (transform.position - playerTransform.position).sqrMagnitude;
        if (sqrDistance <= sqrTriggerDistance)
        {
            StartCoroutine(StartCollapseProcedure());
        }
    }

    // ��鸲 ���� ������Ʈ
    private void UpdateShaking()
    {
        // ��鸲 Ÿ�̸� ����
        shakeTimer += Time.deltaTime;

        // Ư�� �ð��� ������ ��鸲 �ܰ� �Ϸ�
        if (shakeTimer >= SHAKE_DURATION)
        {
            // ��鸲 ����, �������� ����
            currentState = CubeState.Falling;

            // ������Ʈ ��ġ�� ���� ��ġ�� ��Ȯ�� ������ (��鸲 ����)
            transform.position = new Vector3(
                originalPosition.x,
                transform.position.y,  // Y ����
                originalPosition.z
            );

            return;
        }

        // ������� ���� ��鸲 ���� ��� (���������� ����)
        float progress = shakeTimer / SHAKE_DURATION; // 0 ~ 1 ����

        // ���� ��鸲 ���� (�ð��� �������� �� ���� ����)
        float intensityFactor = Mathf.Pow(progress, SHAKE_ACCELERATION);

        // �ʱ� �������� �ִ� ������ ����
        currentShakeIntensity = Mathf.Lerp(INITIAL_SHAKE_INTENSITY, MAX_SHAKE_INTENSITY, intensityFactor);

        // �ð� ����� ���� �� ������ ��鸲 (������� ���� �ӵ� ����)
        float currentShakeSpeed = SHAKE_SPEED * (1f + progress);

        // �ð��� ���� ��鸲 ��ġ ���
        float time = Time.time * currentShakeSpeed;
        float xOffset = Mathf.Sin(time * 0.9f) * currentShakeIntensity;
        float zOffset = Mathf.Sin(time * 1.1f) * currentShakeIntensity;

        // ������� ���������� �� ���������� ������ �߰�
        if (progress > 0.7f)
        {
            xOffset += Mathf.Sin(time * 2.7f) * currentShakeIntensity * 0.3f;
            zOffset += Mathf.Sin(time * 3.1f) * currentShakeIntensity * 0.3f;
        }

        // ��ġ ���� (Y���� ����, X�� Z�� ����)
        transform.position = new Vector3(
            originalPosition.x + xOffset,
            transform.position.y,  // Y���� ���� ���� ����
            originalPosition.z + zOffset
        );
    }

    // �������� ���� ������Ʈ
    private void UpdateFalling()
    {
        // ���� ��ġ ����
        float prevY = transform.position.y;

        // �Ʒ� �������� �̵� (���� �ӵ�)
        transform.Translate(Vector3.down * COLLAPSE_SPEED * Time.deltaTime);

        // ������ �Ÿ� ���� ���
        fallenDistance += (prevY - transform.position.y);

        // �Ÿ� ��� ��Ȱ��ȭ üũ
        if (fallenDistance >= DEACTIVATE_DISTANCE)
        {
            DeactivateCube();
        }
    }

    // �ر� ���� ����
    private IEnumerator StartCollapseProcedure()
    {
        // �̹� ���� ���̸� ���
        if (currentState != CubeState.Idle) yield break;

        // ������ Ʈ���� ��忡�� �ٸ� ť��鵵 �Բ� �ر�
        if (triggerType == TriggerType.AreaTrigger)
        {
            StartCoroutine(TriggerAdditionalCubes());
        }

        // ��� ��� �ð�
        yield return new WaitForSeconds(warningDelay);

        // ��鸲 �ܰ� ����
        currentState = CubeState.Shaking;
        shakeTimer = 0f;
        currentShakeIntensity = INITIAL_SHAKE_INTENSITY;

        // �ð� ��� ��Ȱ��ȭ ����
        yield return new WaitForSeconds(SHAKE_DURATION + DEACTIVATE_TIME);

        // ���� ��Ȱ��ȭ���� �ʾҴٸ�
        if (currentState != CubeState.Collapsed)
        {
            DeactivateCube();
        }
    }

    // �߰� ť��� ���������� �ر� Ʈ����
    private IEnumerator TriggerAdditionalCubes()
    {
        foreach (var cube in additionalCubes)
        {
            if (cube != null && cube != this && cube.currentState == CubeState.Idle)
            {
                cube.TriggerCollapse();

                // ������ �����Ǿ� ������ ���
                if (collapseInterval > 0)
                {
                    yield return new WaitForSeconds(collapseInterval);
                }
            }
        }
    }

    // ť�� ��Ȱ��ȭ
    private void DeactivateCube()
    {
        currentState = CubeState.Collapsed;
        gameObject.SetActive(false);
    }

    // ���� �ر� Ʈ���� (�����ͳ� �ٸ� ��ũ��Ʈ���� ȣ�� ����)
    public void TriggerCollapse()
    {
        if (currentState == CubeState.Idle)
        {
            StartCoroutine(StartCollapseProcedure());
        }
    }

    // OnTriggerEnter �̺�Ʈ ó�� (Ʈ���� �ݶ��̴��� �浹 ��)
    private void OnTriggerEnter(Collider other)
    {
        // �ܺ� Ʈ���� ��� �Ǵ� ������ Ʈ���� ��忡�� ó��
        if ((triggerType == TriggerType.ExternalTrigger || triggerType == TriggerType.AreaTrigger) &&
            currentState == CubeState.Idle)
        {
            // ������ Ʈ���� ��忡�� �� ���� Ʈ���� Ȯ��
            if (triggerType == TriggerType.AreaTrigger && oneTimeUse && hasTriggered)
            {
                return;
            }

            // �÷��̾� �±װ� ������ ��� �±� Ȯ��
            if (!string.IsNullOrEmpty(playerTag))
            {
                if (other.CompareTag(playerTag))
                {
                    if (triggerType == TriggerType.AreaTrigger)
                    {
                        hasTriggered = true;
                        Debug.Log($"[{gameObject.name}] ������ Ʈ���� �ߵ�! ����� {additionalCubes.Count}�� ť��� �Բ� �ر� ����");
                    }
                    StartCoroutine(StartCollapseProcedure());
                }
            }
            else // �±� ������ �� �� ��� ��� �浹 ó��
            {
                if (triggerType == TriggerType.AreaTrigger)
                {
                    hasTriggered = true;
                }
                StartCoroutine(StartCollapseProcedure());
            }
        }
    }

    // �ر� ť�� �ʱ�ȭ (���� ��)
    public void Reset()
    {
        StopAllCoroutines();
        currentState = CubeState.Idle;
        fallenDistance = 0f;
        shakeTimer = 0f;
        hasTriggered = false;
        transform.position = originalPosition;
    }

    // ����׿�: ������ Ʈ���� ���� �ð�ȭ
    void OnDrawGizmos()
    {
        if (triggerType == TriggerType.PlayerProximity)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // ��Ȳ��, ������
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
        else if (triggerType == TriggerType.AreaTrigger)
        {
            // ������ Ʈ���� ���� ǥ��
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f); // ������, ������
                if (col is BoxCollider)
                {
                    BoxCollider box = col as BoxCollider;
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.matrix = oldMatrix;
                }
                else
                {
                    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                }
            }

            // ����� ť���� ���ἱ ǥ��
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // �����
            foreach (var cube in additionalCubes)
            {
                if (cube != null)
                {
                    Gizmos.DrawLine(transform.position, cube.transform.position);
                }
            }
        }
    }
}