using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    // ���� ���
    [SerializeField] private Transform target;
    // ��󿡼��� ��ġ
    [SerializeField] private Vector3 offset;



    // Update is called once per frame
    void Update()
    {
        // �� ������Ʈ(ī�޶�)�� ��ġ��
        // ������κ��� offeset��ŭ ������ ��ġ
        transform.position = target.position + offset;
    }
}
