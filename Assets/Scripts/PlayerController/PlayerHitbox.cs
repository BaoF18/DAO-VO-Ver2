using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerHitbox : MonoBehaviour
{
    private int currentDamage = 0;
    private Collider hitboxCollider;

    [Header("Hitbox Stats")]
    public float baseKnockback = 5f;

    private void Awake()
    {
        EnsureCollider();
        DisableHitbox();
    }

    public void EnableHitbox(int damageAmount)
    {
        if (!EnsureCollider())
        {
            return;
        }

        currentDamage = damageAmount;
        hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (!EnsureCollider())
        {
            return;
        }

        hitboxCollider.enabled = false;
    }

    private bool EnsureCollider()
    {
        if (hitboxCollider == null)
        {
            hitboxCollider = GetComponent<Collider>();
        }

        if (hitboxCollider == null)
        {
            Debug.LogError($"[PlayerHitbox] Missing Collider on {name}.", this);
            return false;
        }

        if (!hitboxCollider.isTrigger)
        {
            hitboxCollider.isTrigger = true;
        }

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
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