using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DashAction : ActorAction
{
    // rigid
    private Rigidbody rigid;

    private void Awake()
    { rigid = GetComponent<Rigidbody>(); }


    // 대시 거리
    [SerializeField] protected float dashPower;

    // 대시 회복 속도
    [SerializeField] protected float dashRecupTime;

    // 대시 스택
    [SerializeField] protected int dashStack;

    public void Dash()
    {
        if (0 < dashStack)
        {
            rigid.AddForce(this.transform.forward * dashPower, ForceMode.Impulse);

            dashStack--;  // -1스택
            Timer.Instance.StartTimer(this, dashRecupTime, () => { dashStack++; }); // 일정 시간 후 다시 +1스택
        }
    }

}
