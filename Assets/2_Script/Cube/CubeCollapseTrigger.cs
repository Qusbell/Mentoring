using System.Collections;
using UnityEngine; // Unity�� �ٽ� Ŭ�������� �����ϴ� ���ӽ����̽�

// �ر��ϴ� ť���� �۵��� �����
public class CubeCollapseTrigger : MonoBehaviour
{
    [Header("�ر� Ʈ���� ����")]
    public bool useTimeTrigger = false;         // �ð� ����� �ر����� ����
    public float delayTime = 3f;                // �ر����� ��ٸ� �ð� (��)

    public bool usePlayerTrigger = false;       // �÷��̾ Ư�� ��ġ�� ���� �ر����� ����
    public Transform player;                    // �÷��̾� ������Ʈ
    public Vector3 triggerPosition;             // �÷��̾ �����ؾ� �ϴ� ��ġ
    public float triggerRadius = 0.5f;          // Ʈ���� ��ġ �ݰ� (�Ÿ� ��� ����)

    [Header("�ر� ����")]
    public float collapseSpeed = 5f;            // �ر� �ӵ�
    public float collapseDistance = 10f;        // �󸶳� �������� (�Ÿ�)

    [Header("�ر� ���� (�⺻: �Ʒ� ����)")]
    public Vector3 customCollapseDirection = Vector3.down; // ���ϴ� �ر� ���� ����

    private bool isCollapsing = false;          // ���� �ر� ������ ����
    private Vector3 collapseDirection;          // ���� ���� �ر� ����
    private Vector3 startPosition;              // ���� ��ġ �����

    void Start()
    {
        startPosition = transform.position;

        // ����� ���� ������ ����ȭ�ؼ� ���� (���⸸ ����)
        collapseDirection = customCollapseDirection.normalized;

        // �ð� Ʈ���Ű� �����ٸ� ���� �ð� �� �ر� ����
        if (useTimeTrigger)
        {
            StartCoroutine(CollapseAfterDelay());
        }
    }

    void Update()
    {
        // �÷��̾ Ʈ���� ��ġ ��ó�� �����ߴ��� Ȯ��
        if (usePlayerTrigger && player != null)
        {
            float distanceToTrigger = Vector3.Distance(player.position, triggerPosition);
            if (distanceToTrigger < triggerRadius && !isCollapsing)
            {
                StartCoroutine(CollapseSequence());
            }
        }

        // �ر� ���̸� ������ �������� ��� �̵�
        if (isCollapsing)
        {
            transform.Translate(collapseDirection * collapseSpeed * Time.deltaTime);

            // ������ �Ÿ� �̻� �������ٸ� ť�� ����
            if (Vector3.Distance(startPosition, transform.position) >= collapseDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    // ���� �ð� �� �ر� ����
    IEnumerator CollapseAfterDelay()
    {
        yield return new WaitForSeconds(delayTime);
        yield return StartCoroutine(CollapseSequence());
    }

    // �ر� ���� �� �ر� ����
    IEnumerator CollapseSequence()
    {
        // ���Ʒ��� ��¦ ��鸮�� ����
        Vector3 up = transform.position + Vector3.up * 0.3f;
        Vector3 down = transform.position - Vector3.up * 0.3f;

        for (int i = 0; i < 3; i++)
        {
            transform.position = up;
            yield return new WaitForSeconds(0.05f);
            transform.position = down;
            yield return new WaitForSeconds(0.05f);
        }

        // ���������� �ر� ����
        isCollapsing = true;
    }
}