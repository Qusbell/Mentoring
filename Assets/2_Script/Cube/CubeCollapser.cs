using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ر� ť�� ������Ʈ
/// �ð� ��� �Ǵ� �÷��̾� ������ ���� �ر� ��� ����
/// </summary>
public class CubeCollapser : MonoBehaviour
{
    [Header("�ر� ����")]
    [Tooltip("�ر��ϴ� ť������ ����")]
    public bool isCollapsible = true;

    [Tooltip("�ر� �ӵ� (�ʴ� ����)")]
    public float collapseSpeed = 5f;

    [Tooltip("�ر� �� ��� �ð� (��)")]
    public float warningDelay = 1f;

    [Tooltip("�ر� Ʈ���� ����")]
    public TriggerType triggerType = TriggerType.PlayerProximity;

    // Ʈ���� Ÿ�� ����
    public enum TriggerType
    {
        Time,            // �ð� ��� (���� �ð� �� �ر�)
        PlayerProximity  // �÷��̾� ����
    }

    [Tooltip("�÷��̾� ���� Ʈ������ ��� �Ÿ� ����")]
    public float triggerDistance = 2f;

    [Tooltip("�÷��̾��� �±�")]
    public string playerTag = "Player";

    [Tooltip("�ر� �� ������Ʈ ��Ȱ��ȭ���� �ð�")]
    public float destroyDelay = 2f;

    // ���� ����
    private bool isCollapsing = false;
    private bool hasCollapsed = false;
    private Transform playerTransform;

    // ���� �� �ʱ�ȭ
    void Start()
    {
        // �÷��̾� ã��
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // �ð� Ʈ������ ��� �ڵ����� �ر� ����
        if (triggerType == TriggerType.Time && isCollapsible)
        {
            StartCoroutine(StartCollapseSequence());
        }
    }

    // �� ������ ����
    void Update()
    {
        // �̹� �ر� ���̰ų� �ر��� �Ϸ�� ��� ����
        if (isCollapsing || hasCollapsed || !isCollapsible) return;

        // �÷��̾� ���� Ʈ���� Ȯ��
        if (triggerType == TriggerType.PlayerProximity && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= triggerDistance)
            {
                StartCoroutine(StartCollapseSequence());
            }
        }

        // �ر� ���� ��� �Ʒ��� �̵�
        if (isCollapsing && !hasCollapsed)
        {
            // �Ʒ� �������� �̵�
            transform.Translate(Vector3.down * collapseSpeed * Time.deltaTime);
        }
    }

    // �ر� ������ ����
    private IEnumerator StartCollapseSequence()
    {
        // �ر� �� ��� �ð�
        yield return new WaitForSeconds(warningDelay);

        // �ر� ����
        isCollapsing = true;

        // ���� �ð� �� ������Ʈ ��Ȱ��ȭ
        yield return new WaitForSeconds(destroyDelay);
        gameObject.SetActive(false);
    }

    // ���� �ر� Ʈ���� (�����ͳ� �ٸ� ��ũ��Ʈ���� ȣ�� ����)
    public void ForceCollapse()
    {
        if (!isCollapsing && !hasCollapsed && isCollapsible)
        {
            StartCoroutine(StartCollapseSequence());
        }
    }

    // �ر� ť�� �ʱ�ȭ (���� ��)
    public void Reset()
    {
        StopAllCoroutines();
        isCollapsing = false;
        hasCollapsed = false;
    }

    // ����׿�: ������ Ʈ���� ���� �ð�ȭ
    void OnDrawGizmos()
    {
        if (triggerType == TriggerType.PlayerProximity)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // ��Ȳ��, ������
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
}