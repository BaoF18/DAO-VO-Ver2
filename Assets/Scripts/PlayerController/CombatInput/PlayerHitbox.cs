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
        // Chỉ đấm những thứ thuộc layer "Enemy"
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            bool hasHitSomething = false;

            // 1. Thử xem có phải giang hồ không?
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(currentDamage, transform.root.position);
                hasHitSomething = true;
            }
            else
            {
                // 2. Nếu không phải giang hồ, thử xem có phải con chó không?
                DogAI dog = other.GetComponent<DogAI>();
                if (dog != null)
                {
                    dog.TakeDamage(currentDamage, transform.root); // Gửi transform của Player sang cho chó gọi bầy
                    hasHitSomething = true;
                }
            }

            // Nếu thực sự đấm trúng mục tiêu (người hoặc chó)
            if (hasHitSomething)
            {
                // Rung màn hình tăng cảm giác lực
                if (Camera.main != null)
                {
                    CameraController cam = Camera.main.GetComponent<CameraController>();
                    if (cam != null)
                    {
                        cam.TriggerShake(0.15f, 0.2f);
                    }
                }

                // Tắt hitbox ngay lập tức để 1 nhát chém không trừ máu 2 lần
                DisableHitbox();
            }
        }
    }
}