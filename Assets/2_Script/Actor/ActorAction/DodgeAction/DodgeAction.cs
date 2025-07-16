using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DodgeAction : ActorAction
{
    private Rigidbody rigid;
    private Collider myCollider;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();

        // 마찰 0인 Material 생성
        zeroFrictionMaterial = new PhysicMaterial();
        zeroFrictionMaterial.dynamicFriction = 0f;
        zeroFrictionMaterial.staticFriction = 0f;
        zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
    }


    // 대시 거리
    [SerializeField] protected float dodgePower = 8;

    // 대시 회복 속도
    [SerializeField] protected float dodgeRecupTime = 3;

    // 대시 스택
    [SerializeField]
    protected int dodgeMaxStatck = 2;
    [SerializeField]
    protected int _dodgeStack = 2; // 최대 스택

    protected int dodgeStack
    {
        get
        { return _dodgeStack; }
        set
        {
            if (dodgeMaxStatck <= value)
            {
                Timer.Instance.StopEndlessTimer(this, "_Recup"); // 재생 종료
                _dodgeStack = dodgeMaxStatck;
            }
            else { _dodgeStack = value; }
        }
    }


    // 미끄러지기 시간 (마찰계수 줄이기 시간)
    [SerializeField] protected float dodgeSlideTime = 0.2f;

    

    // 땃쥐 중?
    public bool isDodge { get; protected set; }

    // 마찰계수
    private PhysicMaterial originalMaterial;   // 원래 Material 저장
    private PhysicMaterial zeroFrictionMaterial;


    public bool isCanDash
    {
        get
        { return 0 < dodgeStack; }
    }


    public void Dodge()
    {
        if (isCanDash)
        {
            // 대시
            rigid.velocity = Vector3.zero;
            rigid.AddForce(this.transform.forward * dodgePower, ForceMode.Impulse);

            // 스택
            dodgeStack--;  // -1스택
            Timer.Instance.StartEndlessTimer(this, "_Recup", dodgeRecupTime, () => { dodgeStack++; }); // 스택 재생 시작

            // 마찰계수 조정
            myCollider.material = zeroFrictionMaterial;
            Timer.Instance.StartTimer(this, "_Material", 0.2f, () => { myCollider.material = originalMaterial; });

            // 땃쥐 지속시간 (콤보 넣기 시간)
            isDodge = true;
            Timer.Instance.StartTimer(this, "_IsDodge", 1, () => { isDodge = false; }); // <- 나중에 지속시간 정정
        }
    }

}
