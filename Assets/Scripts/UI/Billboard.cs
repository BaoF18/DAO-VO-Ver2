using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        // Hàm LateUpdate đảm bảo UI xoay SAU KHI Camera đã di chuyển xong
        if (mainCam != null)
        {
            transform.forward = mainCam.transform.forward;
        }
    }
}