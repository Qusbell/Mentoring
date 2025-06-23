using UnityEngine;


public class ProjectileHit : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private string targetTag = "Player";

    private void Awake()
    {
        // �ݶ��̴� Ʈ���� on
        Collider collider = GetComponent<Collider>();
        if (collider != null) { collider.isTrigger = true; }
        else { Debug.Log("Projectile�� �ݶ��̴� ����"); }
    }


    // �ݶ��̴� �ʼ� Ʈ����
    private void OnTriggerEnter(Collider other)
    {
        // �±� ����
        if (other.CompareTag(targetTag))
        {
            ApplyDamage(other.gameObject);
            Destroy(gameObject);
        }
        // ť�� ���� ��
        else if (other.CompareTag("Cube"))
        { Destroy(gameObject); } 
    }


    // ������ ����
    private void ApplyDamage(GameObject target)
    {
        // ����: Health ������Ʈ�� �ִٰ� ����
        DamageReaction health = target.GetComponent<DamageReaction>();
        if (health != null)
        { health.TakeDamage(damage); }
    }
}
