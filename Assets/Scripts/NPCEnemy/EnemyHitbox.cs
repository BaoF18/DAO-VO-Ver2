using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 20; // Lực đấm của quái
    private Collider hitboxCollider;

    void Start()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
    }

    public void EnableHitbox() { hitboxCollider.enabled = true; }
    public void DisableHitbox() { hitboxCollider.enabled = false; }

    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem có đấm trúng Player không (Dựa vào Tag)
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);

                // Rung camera nhẹ khi bị quái đấm trúng cho có lực!
                if (Camera.main != null)
                {
                    CameraController cam = Camera.main.GetComponent<CameraController>();
                    if (cam != null) cam.TriggerShake(0.2f, 0.3f);
                }

                DisableHitbox(); // Đấm trúng 1 phát là tắt luôn để không trừ máu x2
            }
        }
    }
}