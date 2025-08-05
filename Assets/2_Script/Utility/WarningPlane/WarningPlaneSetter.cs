using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningPlaneSetter
{
    public static GameObject SetWarning(
        MonoBehaviour component,
        float width, float length,
        float warningTime,
        Vector3 posVec, Vector3 rotationVec)
    {
        GameObject warningPlane = WarningPlainPool.Instance.GetWarningPlaneFromPool();

        // --- 방향 ---
        // 이동 벡터에서 각도(Yaw) 계산 (월드 Y축 기준)
        float angleY = Mathf.Atan2(rotationVec.x, rotationVec.z) * Mathf.Rad2Deg;
        // => (0,1)기준 앞, (1,0)기준 오른쪽, (-1,0)기준 왼쪽

        // 평면이 '바닥에 평행'하도록 회전
        // 기본 (90, angleY, 0)
        warningPlane.transform.rotation = Quaternion.Euler(90f, angleY, 0f);

        // --- 위치 ---
        posVec = posVec + rotationVec * (length / 2);
        posVec.y += 0.05f;
        warningPlane.transform.position = posVec;

        // --- 크기 ---
        WarningPlaneCustom.Instance.UpdateSize(warningPlane, width, length);

        // --- 활성화 ---
        warningPlane.SetActive(true);

        // --- 경고 진해지기 ---
        float warningAlpha = 1f;
        float opacityRate = 1f / (warningTime * 0.8f);
        System.Action tempAction = () =>
        {
            WarningPlaneCustom.Instance.UpdateColor(warningPlane, warningAlpha);
            warningAlpha -= opacityRate * Time.deltaTime;
            if (warningAlpha <= 0f)
            {
                warningAlpha = 0f;
                Timer.Instance.StopTimer(component, "_Warning");
            }
        };

        Timer.Instance.StartRepeatTimer(component, "_Warning", warningTime, tempAction);

        return warningPlane;
    }


    public static void DelWarning(MonoBehaviour component, ref GameObject warningPlane)
    {
        if (warningPlane == null) { return; }

        Timer.Instance.StopTimer(component, "_Warning");
        WarningPlaneCustom.Instance.SetBase(warningPlane);

        if (WarningPlainPool.Instance != null)
        { WarningPlainPool.Instance.ReturnWarningPlaneToPool(warningPlane); }

        warningPlane = null;
    }

}
