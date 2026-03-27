using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    private int currentDamage = 0;
    private Collider hitboxCollider;

    [Header("Hitbox Stats")]
    public float baseKnockback = 5f;

    void Start()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
    }

    public void EnableHitbox(int damageAmount)
    {
        currentDamage = damageAmount;
        hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                float appliedKnockback = baseKnockback * (currentDamage / 25f);
                enemy.TakeDamage(currentDamage, transform.root.position);

                if (Camera.main != null)
                {
                    CameraController cam = Camera.main.GetComponent<CameraController>();
                    if (cam != null)
                    {
                        cam.TriggerShake(0.15f, 0.2f);
                    }
                }

                DisableHitbox();
            }
        }
    }
}