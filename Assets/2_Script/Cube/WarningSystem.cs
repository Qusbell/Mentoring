using UnityEngine;
using System.Collections;


/// ������ �Ʒ��� �������� ť�갡 �� �Ʒ��� �ִ� ť���� ���� �߾ӿ� ������ ��� ǥ�ø� �����ִ� ��ũ��Ʈ
/// ť�갡 ����������� ��� ǥ�ð� �� ����������, ���� ������ �ε巴�� �����.

public class WarningSystem : MonoBehaviour
{
    [Header("�⺻ ����")]
    [Tooltip("�Ʒ��� Ȯ���� �ִ� �Ÿ�")]
    public float rayDistance = 10f;

    [Tooltip("������ ť�� ���̾�")]
    public LayerMask detectionLayer = 1;

    [Header("��� ǥ�� ȿ�� ����")]
    [Tooltip("��� ���� ��ȭ ���� �Ÿ� ���� (0.5 = ���� �Ÿ����� ��ȭ ����)")]
    [Range(0.2f, 0.8f)]
    public float colorChangeStartRatio = 0.5f;

    [Tooltip("��� ǥ�� ����� ���� �Ÿ� (���� �� �� �Ÿ����� ���̵� ����)")]
    public float fadeStartDistance = 0.5f;

    [Tooltip("��� ǥ�� ����� �ð� (��)")]
    public float fadeDuration = 0.3f;

    // ���� ����
    private GameObject warningPlane;      // ��� ǥ�� ������Ʈ
    private Vector3 initialPosition;      // ���� ��ġ
    private Vector3 targetPosition;       // ���� ��ġ
    private float totalDistance;          // �� �̵� �Ÿ�
    private bool isFading = false;        // ���̵� �� ����
    private Material planeMaterial;       // ��� ǥ�� ����

    // ���� ����
    private readonly Color warningColor = Color.red;  // ��� ����
    private const float startAlpha = 0.3f;            // �ʱ� ����
    private const float maxAlpha = 0.8f;              // �ִ� ����
    private const float emissionIntensity = 1f;       // �߱� ����


    /// ���� �� �ʱ�ȭ �� ù �˻� ����
    void Start()
    {
        // �ʱ� ��ġ ����
        initialPosition = transform.position;

        // ����� �α� �߰�
        Debug.Log($"[{gameObject.name}] ���� ��ġ: {transform.position}, ���̾� ����ũ: {detectionLayer.value}");

        // �Ʒ� ť�� Ȯ��
        CheckForCubeBelow();
    }


    /// Ȱ��ȭ�� �� �˻� ����
    void OnEnable()
    {
        // �ʱ� ��ġ�� ���� �˻�
        if (transform.position == initialPosition)
        {
            CheckForCubeBelow();
        }
    }

    /// �Ʒ��� �ִ� ť�� Ȯ�� �� ��� ǥ�� ����
    private void CheckForCubeBelow()
    {
        RaycastHit hit;

        // ����ĳ��Ʈ�� �Ʒ� ť�� ���� - ����� ���� �߰�
        Debug.DrawRay(transform.position, Vector3.down * rayDistance, Color.yellow, 5f);

        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, detectionLayer))
        {
            // ����� �α� �߰�
            Debug.Log($"����ĳ��Ʈ ����: {hit.collider.gameObject.name}, �Ÿ�: {hit.distance}");

            // ������ ������Ʈ���� ������ ������Ʈ Ȯ��
            Renderer targetRenderer = hit.collider.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                Debug.Log($"��� ������ ũ��: {targetRenderer.bounds.size}, �߾�: {targetRenderer.bounds.center}");

                // �߿�: ��Ʈ ����Ʈ�� �ٷ� ��� - �̰��� �浹 ������ ��Ȯ�� ��ġ
                Vector3 warningPosition = hit.point;

                // ť�� ���� �߾ӿ� ��� ǥ�� ���� - ��Ʈ ����Ʈ ���� ���
                CreateWarningPlane(warningPosition, targetRenderer);

                // �� ť���� ���� ��ġ ���
                float landingY = warningPosition.y + GetComponent<Renderer>().bounds.extents.y;
                targetPosition = new Vector3(
                    transform.position.x,
                    landingY,
                    transform.position.z
                );

                // �� �̵� �Ÿ� ���
                totalDistance = Vector3.Distance(initialPosition, targetPosition);
                Debug.Log($"���� ��ġ: {targetPosition}, �� �̵� �Ÿ�: {totalDistance}");
            }
            else
            {
                Debug.LogWarning($"������ ������Ʈ {hit.collider.gameObject.name}�� Renderer ������Ʈ�� �����ϴ�!");
            }
        }
        else
        {
            Debug.LogWarning("�Ʒ������� ť�긦 �������� ���߽��ϴ�. ���̾� ������ Ȯ���ϼ���.");
        }
    }


    /// ť�� ���� �߾ӿ� ��� ǥ�� ����
    /// 
    /// <param name="position">��� ǥ�� ���� ��ġ (��Ʈ ����Ʈ)</param>
    /// <param name="targetRenderer">�Ʒ� ť���� ������ ������Ʈ</param>
    private void CreateWarningPlane(Vector3 position, Renderer targetRenderer)
    {
        // ���� ��� ǥ�� ����
        if (warningPlane != null)
        {
            Destroy(warningPlane);
        }

        // ����� �α� �߰�
        Debug.Log($"��� ǥ�� ���� ��ġ: {position}");

        // ��� ����
        warningPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        warningPlane.name = "Warning_Plane";

        // ��ġ �� ȸ�� ���� (��Ʈ ����Ʈ ��ġ �״�� ���)
        warningPlane.transform.position = position;
        warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0); // X�� ���� 90�� ȸ�� (�ٴڰ� ����)

        // �Ʒ� ť�� ũ�⿡ �°� ũ�� ����
        float planeWidth = targetRenderer.bounds.size.x * 0.9f; // �ణ �۰� ����� ��踦 ����� �ʵ���
        float planeLength = targetRenderer.bounds.size.z * 0.9f;
        warningPlane.transform.localScale = new Vector3(planeWidth, planeLength, 1f);

        Debug.Log($"��� ǥ�� ũ��: {planeWidth} x {planeLength}");

        // �浹ü ��Ȱ��ȭ (�浹 ó�� ����)
        warningPlane.GetComponent<Collider>().enabled = false;

        // Ignore Raycast ���̾�� ����
        warningPlane.layer = LayerMask.NameToLayer("Ignore Raycast");

        // ��Ƽ���� ���� �� ����
        Renderer planeRenderer = warningPlane.GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            planeMaterial = new Material(Shader.Find("Standard"));

            // ������ ����
            planeMaterial.SetFloat("_Mode", 3);
            planeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            planeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            planeMaterial.SetInt("_ZWrite", 0);
            planeMaterial.DisableKeyword("_ALPHATEST_ON");
            planeMaterial.EnableKeyword("_ALPHABLEND_ON");
            planeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            planeMaterial.renderQueue = 3000;

            // �ʱ� ���� �� ���� ����
            Color color = warningColor;
            color.a = startAlpha;
            planeMaterial.color = color;

            // �߱� ȿ�� �߰�
            planeMaterial.EnableKeyword("_EMISSION");
            planeMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);

            planeRenderer.material = planeMaterial;
        }
    }


    /// �� ������ ������Ʈ - �Ÿ��� ���� ��� ǥ�� ȿ�� ����
    void Update()
    {
        // ��� ǥ�ð� ������ �ƹ��͵� ���� ����
        if (warningPlane == null || planeMaterial == null) return;

        // ���� ���� ��ġ���� �Ÿ� ���
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // ������ ���� (�ʱ� ��ġ���� �ణ�̶� ����������)
        bool isMoving = Vector3.Distance(transform.position, initialPosition) > 0.05f;

        if (isMoving && !isFading)
        {
            // ���� �����̸� ������� ����
            if (distanceToTarget <= fadeStartDistance)
            {
                StartCoroutine(FadeOutWarning());
            }
            else
            {
                // �Ÿ��� ���� ��� ǥ�� ���� ������Ʈ
                UpdateWarningIntensity(distanceToTarget);
            }
        }
    }


    /// �Ÿ��� ���� ��� ǥ�� ���� ������Ʈ
    /// <param name="currentDistance">���� ���� ��ġ���� �Ÿ�</param>
    private void UpdateWarningIntensity(float currentDistance)
    {
        // �Ÿ� ���� ��� (1 = �� �Ÿ�, 0 = ����� �Ÿ�)
        float distanceRatio = Mathf.Clamp01(currentDistance / totalDistance);

        // ���� ��ȭ ������ �����̸� �ʱ� ���� ����
        if (distanceRatio > colorChangeStartRatio)
        {
            Color color = warningColor;
            color.a = startAlpha;
            planeMaterial.color = color;
            planeMaterial.SetColor("_EmissionColor", warningColor * startAlpha * emissionIntensity);
            return;
        }

        // ��ȭ ���� ���� ��� (�Ÿ��� ����������� ���� Ŀ��)
        float changeProgress = 1f - (distanceRatio / colorChangeStartRatio);

        // ���� ��� (���� �� ����������)
        float alpha = Mathf.Lerp(startAlpha, maxAlpha, changeProgress);

        // ���� �� �߱� ȿ��
        Color newColor = warningColor;
        newColor.a = alpha;
        planeMaterial.color = newColor;
        planeMaterial.SetColor("_EmissionColor", warningColor * alpha * emissionIntensity);
    }

    /// ��� ǥ�ø� ������ ������� �ϴ� �ڷ�ƾ
    private IEnumerator FadeOutWarning()
    {
        if (warningPlane == null || planeMaterial == null) yield break;

        isFading = true;

        // ���� ���� ����
        Color startColor = planeMaterial.color;
        Color emissionColor = planeMaterial.GetColor("_EmissionColor");

        // ���̵� �ƿ� ȿ��
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);

            // ���� ���������� ȿ��
            Color newColor = startColor;
            newColor.a = Mathf.Lerp(startColor.a, 0f, t);
            planeMaterial.color = newColor;

            // �߱� ȿ���� �Բ� ����
            Color newEmission = Color.Lerp(emissionColor, Color.black, t);
            planeMaterial.SetColor("_EmissionColor", newEmission);

            yield return null;
        }

        // ������ ���������� ����
        RemoveWarning();
    }

    /// ��� ǥ�� ����
    private void RemoveWarning()
    {
        if (warningPlane != null)
        {
            Destroy(warningPlane);
            warningPlane = null;
        }

        isFading = false;
    }

    /// ��Ȱ��ȭ�� �� ��� ǥ�� ����
    void OnDisable()
    {
        RemoveWarning();
    }

    /// �ı��� �� ��� ǥ�� ����
    void OnDestroy()
    {
        RemoveWarning();
    }

    /// �� �信�� ����ĳ��Ʈ ��� �ð�ȭ (������) ���ӿ����� �Ⱥ���
    void OnDrawGizmos()
    {
        // ����ĳ��Ʈ ��� �ð�ȭ
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * rayDistance);

        // ������ ť�갡 ������ ��ġ ǥ��
        if (warningPlane != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(warningPlane.transform.position, 0.1f);
        }
    }
}