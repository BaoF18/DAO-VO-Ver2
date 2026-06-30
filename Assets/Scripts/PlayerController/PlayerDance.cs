using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDance : MonoBehaviour
{
    [Header("Dance System Toggle")]
    [Tooltip("Khóa/Mở chức năng nhảy múa (Ví dụ: tắt khi đang nói chuyện NPC)")]
    public bool isDanceUnlocked = false;

    public InputActionAsset InputActions;
    private InputAction m_headDanceAction;
    private InputAction m_handDanceAction;
    private Animator m_animator;
   
    private int currentDanceType = 0; // 0: Không nhảy, 1: Nhảy đầu, 2: Nhảy tay
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_headDanceAction = InputActions.FindActionMap("Player").FindAction("HeadDance");
        m_handDanceAction = InputActions.FindActionMap("Player").FindAction("HandDance");
    }
    private void OnEnable()
    {
        m_headDanceAction.Enable();
        m_handDanceAction.Enable();
    }
    private void OnDisable()
    {
        m_headDanceAction.Disable();
        m_handDanceAction.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        if (!isDanceUnlocked) return;

        if(m_headDanceAction.WasPressedThisFrame())
        {
            toogleDance(1); // 1: Nhảy đầu
        }
        else if (m_handDanceAction.WasPressedThisFrame())
        {
            toogleDance(2); // 2: Nhảy tay
        }
    }
    private void toogleDance(int danceType)
    {
        // Nếu bấm lại cùng một loại nhảy, thì dừng nhảy
        if (currentDanceType == danceType)
        {
            StopDancing();
        }
        // Nếu bấm loại nhảy khác, thì chuyển sang loại nhảy mới
        else
        {
            currentDanceType = danceType;
            ExecuteDancing(danceType);
        }
    }
    private void ExecuteDancing(int danceType)
    {
        m_animator.SetInteger("DanceIndex", danceType);
        m_animator.SetTrigger("GoDance");
        m_animator.SetBool("IsDancing", true);

        Debug.Log($"Started dancing with type: {danceType}. Yeeeehaw!");
    }
    //public để gọi từ script di chuyển
    public void StopDancing()
    {
        currentDanceType = 0;
        m_animator.SetBool("IsDancing", false);
        Debug.Log("Stopped dancing.");
    }
}
