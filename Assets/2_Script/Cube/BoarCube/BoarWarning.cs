using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 멧돼지 경고 표시 관리
/// </summary>
public class BoarWarning : MonoBehaviour
{
    private BoarCube main;
    private List<GameObject> activeWarningPlanes = new List<GameObject>();

    // 공유 리소스
    private static Material sharedWarningMaterial;
    private static Queue<GameObject> warningPlanePool = new Queue<GameObject>();
    private static readonly Color warningColor = Color.red;
    private static readonly int POOL_SIZE = 20;

    public void Initialize(BoarCube mainComponent)
    {
        main = mainComponent;
        InitializeSharedResources();
        EnsureWarningPlanePool();
    }

    public void ShowWarning()
    {
        ClearWarning();

        Vector3 direction = GetDirection();
        float pathLength = GetPathLength();
        float actualWidth = GetActualAttackWidth();

        int segmentCount = Mathf.CeilToInt(pathLength / actualWidth);
        segmentCount = Mathf.Max(1, segmentCount);

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 segmentPos = Vector3.Lerp(GetStartPosition(), GetEndPosition(), t);
            CreateWarningPlaneAt(segmentPos, actualWidth);
        }

        if (main.showDebugLog)
            Debug.Log($"[{gameObject.name}] 경고 표시 생성: {activeWarningPlanes.Count}개");
    }

    public void ClearWarning()
    {
        foreach (GameObject plane in activeWarningPlanes)
        {
            ReturnWarningPlaneToPool(plane);
        }
        activeWarningPlanes.Clear();
    }

    // ==================== 유틸리티 ====================

    private Vector3 GetDirection()
    {
        return (GetEndPosition() - GetStartPosition()).normalized;
    }

    private float GetPathLength()
    {
        return Vector3.Distance(GetStartPosition(), GetEndPosition());
    }

    private Vector3 GetStartPosition()
    {
        return transform.position + main.startPositionOffset;
    }

    private Vector3 GetEndPosition()
    {
        return transform.position;
    }

    private float GetActualAttackWidth()
    {
        if (main.attackWidth > 0f)
            return main.attackWidth;

        Renderer cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
        {
            return Mathf.Max(cubeRenderer.bounds.size.x, cubeRenderer.bounds.size.z);
        }

        return Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
    }

    // ==================== 풀링 시스템 ====================

    private static void InitializeSharedResources()
    {
        if (sharedWarningMaterial == null)
        {
            sharedWarningMaterial = new Material(Shader.Find("Standard"));
            sharedWarningMaterial.SetFloat("_Mode", 3);
            sharedWarningMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            sharedWarningMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            sharedWarningMaterial.SetInt("_ZWrite", 0);
            sharedWarningMaterial.EnableKeyword("_ALPHABLEND_ON");
            sharedWarningMaterial.renderQueue = 3000;
            sharedWarningMaterial.EnableKeyword("_EMISSION");
        }
    }

    private static void EnsureWarningPlanePool()
    {
        while (warningPlanePool.Count < POOL_SIZE)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plane.name = "PooledWarningPlane";
            Object.Destroy(plane.GetComponent<Collider>());
            plane.layer = LayerMask.NameToLayer("Ignore Raycast");
            plane.GetComponent<Renderer>().material = sharedWarningMaterial;
            plane.SetActive(false);
            warningPlanePool.Enqueue(plane);
        }
    }

    private void CreateWarningPlaneAt(Vector3 position, float width)
    {
        LayerMask groundLayerMask = (1 << 0) | (1 << 8); // Default + Cube

        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayerMask))
        {
            GameObject warningPlane = GetWarningPlaneFromPool();
            warningPlane.transform.position = hit.point + Vector3.up * 0.01f;
            warningPlane.transform.rotation = Quaternion.Euler(90, 0, 0);
            warningPlane.transform.localScale = new Vector3(width, width, 1f);

            UpdateWarningPlaneColor(warningPlane);
            activeWarningPlanes.Add(warningPlane);
        }
    }

    private static GameObject GetWarningPlaneFromPool()
    {
        EnsureWarningPlanePool();

        if (warningPlanePool.Count > 0)
        {
            GameObject plane = warningPlanePool.Dequeue();
            plane.SetActive(true);
            return plane;
        }

        // 새로 생성
        GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.Destroy(newPlane.GetComponent<Collider>());
        newPlane.layer = LayerMask.NameToLayer("Ignore Raycast");
        newPlane.GetComponent<Renderer>().material = sharedWarningMaterial;
        return newPlane;
    }

    private static void ReturnWarningPlaneToPool(GameObject plane)
    {
        if (plane != null)
        {
            plane.SetActive(false);
            if (warningPlanePool.Count < POOL_SIZE * 2)
                warningPlanePool.Enqueue(plane);
            else
                Object.Destroy(plane);
        }
    }

    private void UpdateWarningPlaneColor(GameObject plane)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material instanceMaterial = new Material(sharedWarningMaterial);
            Color finalColor = warningColor;
            finalColor.a = main.warningAlpha;
            instanceMaterial.color = finalColor;
            instanceMaterial.SetColor("_EmissionColor", warningColor * 0.5f);
            renderer.material = instanceMaterial;
        }
    }

    void OnDestroy()
    {
        ClearWarning();
    }
}