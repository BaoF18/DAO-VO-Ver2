using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Because the Prefabs cannot be able to connect to the UI directly, we need a "Broadcast Station" that any prefabs can detect this UI.
/// </summary>
public class SharedUI : MonoBehaviour
{
    // Biến Instance này chính là "Trạm phát sóng"
    public static SharedUI Instance;

    [Tooltip("Kéo cục Panel (Screen) vào đây")]
    public GameObject uiPanel;

    [Tooltip("Kéo cục chứa thanh chạy (có component Image) vào đây")]
    public Image fillImage;

    void Awake()
    {
        // Khi game bắt đầu, tự động đăng ký bản thân làm trạm phát sóng
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}