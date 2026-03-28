using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    public float moveSpeed = 2f; 
    public float disappearTimer = 1f;
    private Color textColor;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount)
    {
        // Gán con số sát thương vào text
        textMesh.SetText(damageAmount.ToString());
        textColor = textMesh.color;

        
    }

    private void Update()
    {
        // 1. Bay từ từ lên trời
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. Ép text luôn quay mặt về phía Camera (để người chơi luôn đọc được)
        transform.LookAt(transform.position + Camera.main.transform.forward);

        // 3. Mờ dần rồi biến mất
        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            float fadeAmount = 5f;
            textColor.a -= fadeAmount * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject); // Xóa khỏi bộ nhớ khi đã tàng hình
            }
        }
    }
}