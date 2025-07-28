using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class DodgeAction : ActorAction
{
    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();

        // 비활성화 상태
        this.enabled = false;

        // 기존 레이어 저장
        originalLayer = this.gameObject.layer;
    }


    // 대시 거리
    [SerializeField] protected float dodgePower = 8;

    // 미끄러지기 시간
    [SerializeField] protected float dodgeSlideTime = 0.4f;

    [SerializeField] protected float dodgeAngle = 30f;

    // 닷지 코스트
    [field: SerializeField] public int dodgeCost { get; set; } = 1;

    // 프리펩에서 지정해서, 실제로 회전시킬 오브젝트
    [SerializeField] private GameObject ratateObjectWhenDodge;


    // 땃쥐 중?
    public bool isDodge { get; protected set; }

    // 원래 레이어
    private int originalLayer = -1;


    public void Dodge()
    {
        // --- dodge 중에는 dodge X ---
        if (isDodge) { return; }
        
        // --- dodge true ---
        this.enabled = true;
        isDodge = true;
        rigid.useGravity = false;
        ratateObjectWhenDodge.transform.Rotate(dodgeAngle, 0, 0); // 앞으로 기울기
        this.gameObject.layer = LayerMask.NameToLayer("IgnoreOtherActor");
        rigid.velocity = Vector3.zero;


        // --- dodge false ---
        Timer.Instance.StartTimer(this, "_Dodge", dodgeSlideTime,
            () => 
            {
                this.enabled = false;
                isDodge = false;
                rigid.useGravity = true;
                ratateObjectWhenDodge.transform.Rotate(-dodgeAngle, 0, 0);
                this.gameObject.layer = originalLayer;

                rigid.velocity = Vector3.zero;
            });
    }


    // dodge 시 이동
    // x, z로만 힘
    private void FixedUpdate()
    {
        Vector3 currentVelocity = rigid.velocity; // 현재 속도 저장
        Vector3 forwardVelocity = transform.forward * dodgePower; // x,z 축 속도 계산
        rigid.velocity = new Vector3(forwardVelocity.x, currentVelocity.y, forwardVelocity.z); // y 축은 그대로 유지
    }
}
