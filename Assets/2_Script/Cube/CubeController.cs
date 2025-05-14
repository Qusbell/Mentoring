using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // �̺�Ʈ �ý��� ����� ���� �߰�

/// <summary>
/// ť�� Ȱ��ȭ�� Ʈ���Ÿ� �����ϴ� ������Ʈ
/// </summary>
public class CubeController : MonoBehaviour
{
    // Ʈ���� ���� Ÿ�� ����
    public enum TriggerType
    {
        TimeTrigger,  // �ð� Ʈ����: ���� �ð� ��� �� ������Ʈ Ȱ��ȭ
        AreaTrigger,  // ���� Ʈ����: Ư�� ������ �÷��̾ ������ Ȱ��ȭ
        Manual        // ���� Ʈ����: �ڵ忡�� ���� ȣ���Ͽ� Ȱ��ȭ
    }

    // ť�� Ȱ��ȭ ������ �����ϴ� Ŭ����
    [System.Serializable]
    public class CubeData
    {
        [Header("������Ʈ ����")]
        [Tooltip("Ȱ��ȭ�� ť��")]
        public GameObject targetCube;

        [Header("Ʈ���� ����")]
        [Tooltip("Ȱ��ȭ Ʈ���� ����")]
        public TriggerType triggerType = TriggerType.TimeTrigger;

        [Tooltip("�ð� Ʈ������ ���, ��ٸ� �ð�")]
        public float delayTime = 0f;

        [Tooltip("���� Ʈ������ ���, �浹 ������ ���� ������Ʈ")]
        public GameObject triggerArea;

        [Tooltip("���� Ʈ������ ��� �±� (�⺻: Player)")]
        public string targetTag = "Player";

        [HideInInspector] public float timer = 0f;
        [HideInInspector] public bool hasActivated = false;
    }

    [Header("ť�� Ȱ��ȭ ����")]
    public CubeData[] activationSettings;

    [Header("��ü ���� ����")]
    [Tooltip("��ü ���� �� �߰� ��� �ð� (��)")]
    public float globalStartDelay = 0f;

    [Tooltip("��ü ���� ������ ��� ����")]
    public bool useGlobalStartDelay = true;

    [Tooltip("�ܺ� Ʈ���� ��� ���� (üũ�ϸ� StartExecution ȣ�� ������ �������� ����)")]
    public bool waitForExternalTrigger = false;

    [Header("���� ���� ����")]
    [Tooltip("��� ť�갡 Ȱ��ȭ�� �� Ʈ������ ���� ��Ʈ�ѷ�")]
    public CubeController nextController;

    [Tooltip("���� ��Ʈ�ѷ��� �Ѿ�� �� ��� �ð� (��)")]
    public float nextControllerDelay = 0f;

    [Tooltip("��� ť�갡 Ȱ��ȭ�Ǹ� �߻��ϴ� �̺�Ʈ")]
    public UnityEvent onAllCubesActivated;

    [Header("����� �ɼ�")]
    [Tooltip("�� �����Ϳ��� ���� Ʈ���Ÿ� �ð�ȭ")]
    public bool showTriggerAreas = true;

    // ���� ������ Ÿ�̸�
    private float globalStartTimer = 0f;
    private bool globalDelayPassed = false;
    private bool hasStartedExecution = false;
    private bool hasTriggeredNextController = false;
    private int activatedCubeCount = 0;

    // ���� �� ��� ť�� Ȯ��
    void Start()
    {
        // �ܺ� Ʈ���� ��� ������ �ƴϸ� �ڵ����� ���� ����
        if (!waitForExternalTrigger)
        {
            hasStartedExecution = true;

            // �۷ι� �����̰� 0�̰ų� ������� �ʴ� ��� �ٷ� ����
            if (globalStartDelay <= 0f || !useGlobalStartDelay)
            {
                globalDelayPassed = true;
            }
        }

        foreach (var data in activationSettings)
        {
            if (data.targetCube != null)
            {
                // CubeMover ������Ʈ�� ������ �߰�
                CheckAndAddMoverComponent(data.targetCube);
            }
        }
    }

    // ť�꿡 CubeMover ������Ʈ�� �ִ��� Ȯ���ϰ� ������ �߰�
    private void CheckAndAddMoverComponent(GameObject cube)
    {
        CubeMover mover = cube.GetComponent<CubeMover>();
        if (mover == null)
        {
            Debug.LogWarning($"ť�� '{cube.name}'�� CubeMover ������Ʈ�� �����ϴ�. �ڵ����� �߰��˴ϴ�.");
            cube.AddComponent<CubeMover>();
        }
    }

    // �� �����Ӹ��� ���� Ȯ�� (�ð� Ʈ���� ó����)
    void Update()
    {
        // ������ ���۵��� �ʾ����� ó������ ����
        if (!hasStartedExecution) return;

        // �۷ι� ������ üũ
        if (!globalDelayPassed && useGlobalStartDelay)
        {
            globalStartTimer += Time.deltaTime;
            if (globalStartTimer >= globalStartDelay)
            {
                globalDelayPassed = true;
                Debug.Log($"[{gameObject.name}] ��ü ���� ������ {globalStartDelay}�ʰ� �������ϴ�. ť�� Ȱ��ȭ�� �����մϴ�.");
            }
            else
            {
                return; // �����̰� ������ �ʾ����� ť�� Ȱ��ȭ ���� ���� �� ��
            }
        }

        foreach (var data in activationSettings)
        {
            // �̹� Ȱ��ȭ�� ť��� ��ŵ
            if (data.hasActivated) continue;

            // �ð� Ʈ���� ó��
            if (data.triggerType == TriggerType.TimeTrigger)
            {
                data.timer += Time.deltaTime;
                if (data.timer >= data.delayTime)
                {
                    ActivateCube(data);
                }
            }
        }

        // ��� ť�� Ȱ��ȭ üũ
        CheckAllCubesActivated();
    }

    // ��� ť�갡 Ȱ��ȭ�Ǿ����� Ȯ��
    private void CheckAllCubesActivated()
    {
        // �̹� ���� ��Ʈ�ѷ��� Ʈ���������� ��ŵ
        if (hasTriggeredNextController) return;

        // ��� ť�갡 Ȱ��ȭ�Ǿ����� Ȯ��
        bool allActivated = true;
        foreach (var data in activationSettings)
        {
            if (!data.hasActivated)
            {
                allActivated = false;
                break;
            }
        }

        // ��� ť�갡 Ȱ��ȭ�Ǿ����� �̺�Ʈ �߻�
        if (allActivated && activationSettings.Length > 0)
        {
            hasTriggeredNextController = true;
            Debug.Log($"[{gameObject.name}] ��� ť�갡 Ȱ��ȭ�Ǿ����ϴ�. �̺�Ʈ�� �߻���ŵ�ϴ�.");

            // �̺�Ʈ �߻�
            onAllCubesActivated?.Invoke();

            // ���� ��Ʈ�ѷ� Ʈ����
            StartCoroutine(TriggerNextController());
        }
    }

    // ���� ��Ʈ�ѷ� Ʈ����
    private IEnumerator TriggerNextController()
    {
        if (nextController != null)
        {
            // ��� �ð� ����
            if (nextControllerDelay > 0)
            {
                Debug.Log($"[{gameObject.name}] {nextControllerDelay}�� �� ���� ��Ʈ�ѷ��� �����մϴ�.");
                yield return new WaitForSeconds(nextControllerDelay);
            }

            Debug.Log($"[{gameObject.name}] ���� ��Ʈ�ѷ� [{nextController.gameObject.name}]�� �����մϴ�.");
            nextController.StartExecution();
        }
    }

    // ���� Ʈ���� ���� �� ȣ���
    public void OnAreaTrigger(GameObject triggerArea, GameObject other)
    {
        // ������ ���۵��� �ʾҰų� �۷ι� �����̰� ������ �ʾ����� Ʈ���� ����
        if (!hasStartedExecution || (!globalDelayPassed && useGlobalStartDelay)) return;

        // �� Ȱ��ȭ �����͸� Ȯ��
        foreach (var data in activationSettings)
        {
            // �̹� Ȱ��ȭ�� ť��� ��ŵ
            if (data.hasActivated) continue;

            // ���� Ʈ���� �����̰� ������ �±װ� ��ġ�ϴ��� Ȯ��
            if (data.triggerType == TriggerType.AreaTrigger &&
                data.triggerArea == triggerArea &&
                other.CompareTag(data.targetTag))
            {
                ActivateCube(data);
            }
        }
    }

    // ť�� Ȱ��ȭ
    void ActivateCube(CubeData data)
    {
        if (data.targetCube != null && !data.hasActivated)
        {
            // ť�� Ȱ��ȭ
            data.targetCube.SetActive(true);
            data.hasActivated = true;
            activatedCubeCount++;

            Debug.Log($"[{gameObject.name}] ť�� [{data.targetCube.name}]�� Ȱ��ȭ�Ǿ����ϴ�. ({activatedCubeCount}/{activationSettings.Length})");
        }
    }

    // Ư�� �ε����� ť�긦 �������� Ȱ��ȭ
    public void ActivateCubeByIndex(int index)
    {
        // ������ ���۵��� �ʾҰų� �۷ι� �����̰� ������ �ʾ����� Ȱ��ȭ ����
        if (!hasStartedExecution || (!globalDelayPassed && useGlobalStartDelay)) return;

        if (index >= 0 && index < activationSettings.Length)
        {
            ActivateCube(activationSettings[index]);

            // ��� ť�� Ȱ��ȭ üũ
            CheckAllCubesActivated();
        }
    }

    // ��� ť�� ���� �ʱ�ȭ (�׽�Ʈ/����ۿ�)
    public void ResetAll()
    {
        // �۷ι� Ÿ�̸� �ʱ�ȭ
        globalStartTimer = 0f;
        globalDelayPassed = globalStartDelay <= 0f || !useGlobalStartDelay;
        hasTriggeredNextController = false;
        activatedCubeCount = 0;

        // waitForExternalTrigger ������ ���� ���� ���� ����
        hasStartedExecution = !waitForExternalTrigger;

        foreach (var data in activationSettings)
        {
            data.hasActivated = false;
            data.timer = 0f;

            if (data.targetCube != null && data.targetCube.activeSelf)
            {
                data.targetCube.SetActive(false);
            }
        }
    }

    // ��ü ���� ������ ���� (�ܺο��� ȣ�� ����)
    public void SetGlobalStartDelay(float delayInSeconds)
    {
        globalStartDelay = Mathf.Max(0f, delayInSeconds); // ���� ����
        useGlobalStartDelay = true;
        ResetAll(); // ������ ���������� �ʱ�ȭ
    }

    // ���� ���� (�ܺο��� ȣ�� ����)
    public void StartExecution()
    {
        if (!hasStartedExecution)
        {
            hasStartedExecution = true;
            globalStartTimer = 0f;
            Debug.Log($"[{gameObject.name}] ������ �����մϴ�.");
        }
    }

    // ������ ��� �Ϸ� (�׽�Ʈ/����׿�)
    public void SkipGlobalDelay()
    {
        globalDelayPassed = true;
    }

    // ������ ���� ��� Ʈ���� (�׽�Ʈ/����׿�)
    public void TriggerAllCubesActivated()
    {
        if (!hasTriggeredNextController)
        {
            hasTriggeredNextController = true;
            onAllCubesActivated?.Invoke();
            StartCoroutine(TriggerNextController());
        }
    }

    // ����׿�: ������ ���� Ʈ���ſ� ť�긦 ������
    void OnDrawGizmos()
    {
        if (!showTriggerAreas || activationSettings == null) return;

        foreach (var data in activationSettings)
        {
            // Ʈ���� ���� ǥ��
            if (data.triggerType == TriggerType.AreaTrigger && data.triggerArea != null)
            {
                Collider triggerCollider = data.triggerArea.GetComponent<Collider>();
                if (triggerCollider != null)
                {
                    // ���� Ʈ���Ŵ� ������ �ڽ��� ǥ��
                    Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                    Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
                }
            }
        }

        // ���� ��Ʈ�ѷ� ���� ǥ��
        if (nextController != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nextController.transform.position);

            // ȭ��ǥ ǥ��
            Vector3 direction = (nextController.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, nextController.transform.position);
            Vector3 arrowPos = transform.position + direction * (distance * 0.8f);

            // ȭ��ǥ ��� �׸���
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * 0.5f;
            Gizmos.DrawLine(arrowPos, arrowPos - direction * 0.5f + right);
            Gizmos.DrawLine(arrowPos, arrowPos - direction * 0.5f - right);
        }
    }
}