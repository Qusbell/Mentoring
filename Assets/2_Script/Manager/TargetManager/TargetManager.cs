using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TargetManager : MonoBehaviour
{
    // 싱글톤
    public static TargetManager instance { get; private set; }

    // 타겟 (위치)
    private Transform _target = null;
    public Transform target
    {
        get
        {
            if (_target == null)
            { Targeting(); }
            return _target.transform;
        }
        private set
        { _target = value; }
    }


    // 타겟 리스트
    public List<Target> targetList = new List<Target>();

    void Awake()
    {
        if (instance == null) { instance = this; }
        else
        {
            Debug.Log("TargetManager 다수 존재, 단일 존재 위반");
            Destroy(gameObject);
            return;
        }
    }
    

    // 타겟팅
    public void Targeting()
    {
        // 타겟 후보군 가져오기
        targetList = FindObjectsOfType<Target>().ToList();

        // 타겟이 존재하는 경우 : 타겟팅
        if (0 < targetList.Count)
        {
            // 우선순위에 따른 정렬 (오름차순)
            targetList.Sort((a, b) => a.targetPriority.CompareTo(b.targetPriority));

            // 최고 우선 순위 Targeting
            target = targetList[0].transform;
        }
        else
        { Debug.Log("씬에 target 컴포넌트 오브젝트가 존재하지 않음"); }


        // 디버그용 로그
        if (_target == null)
        { Debug.Log("Targeting 완료했지만 target == null"); }
    }
}
