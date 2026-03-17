// File: NPCMovement.cs
// Mô tả: BỘ NÃO DI CHUYỂN TRUNG TÂM của NPC - điều phối toàn bộ movement, navigation và animation.
// Tương tự "AIController" trong GTA V hoặc "ActorMovement" trong Skyrim:
// - Behavior ra lệnh → NPCMovement xử lý → NPCNavigationAgent thực thi → NPCAnimationController đồng bộ.
// - Behavior KHÔNG BAO GIỜ gọi NavMesh trực tiếp, chỉ gọi qua NPCMovement.

using UnityEngine;

[RequireComponent(typeof(NPCNavigationAgent))]
public class NPCMovement : MonoBehaviour
{
    [Header("=== CẤU HÌNH ===")]
    [Tooltip("Kéo ScriptableObject NPCMovementConfig vào đây")]
    [SerializeField] private NPCMovementConfig config;

    [Header("=== BEHAVIOR MẶC ĐỊNH ===")]
    [Tooltip("Behavior sẽ chạy ngay khi game bắt đầu")]
    [SerializeField] private NPCBehaviorBase defaultBehavior;

    // Tham chiếu nội bộ
    private NPCNavigationAgent navAgent;
    private NPCAnimationController animController;
    private NPCBehaviorBase currentBehavior;
    private bool isRunning;
    private bool isPaused;

    // === PROPERTIES CHO EDITOR VÀ DEBUG ===
    public NPCMovementConfig Config => config;
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;

    void Awake()
    {
        navAgent = GetComponent<NPCNavigationAgent>();
        animController = GetComponent<NPCAnimationController>();
    }

    void Start()
    {
        // Áp dụng config ban đầu vào NavMeshAgent
        ApplyConfig();

        NPCBehaviorBase behaviorToStart = defaultBehavior;

        // SAFETY: Behavior PHẢI nằm trên cùng GameObject với NPCMovement.
        // Nếu reference đến behavior trên object khác (ví dụ child object "NPC_1"),
        // hai NPC sẽ share chung npcMovement → gây conflict.
        // Tự động tìm behavior trên chính object này thay thế.
        if (behaviorToStart != null && behaviorToStart.gameObject != gameObject)
        {
            NPCBehaviorBase localBehavior = GetComponent<NPCBehaviorBase>();
            if (localBehavior != null)
            {
                Debug.LogWarning(
                    $"[NPCMovement] {name}: Default Behavior đang reference component trên '{behaviorToStart.gameObject.name}' (khác object). " +
                    $"Tự động dùng {localBehavior.GetType().Name} trên '{gameObject.name}' thay thế.",
                    this);
                behaviorToStart = localBehavior;
            }
        }

        // Fallback: nếu chưa có default behavior, tự tìm trên object này
        if (behaviorToStart == null)
        {
            behaviorToStart = GetComponent<NPCBehaviorBase>();
        }

        if (behaviorToStart != null)
        {
            SetBehavior(behaviorToStart);
        }
    }

    void Update()
    {
        // Khi đang tạm dừng (dialogue, interact) → chỉ sync animation idle, không tick behavior
        if (!isPaused && currentBehavior != null)
        {
            currentBehavior.Tick();
        }

        // Luôn đồng bộ animation (để hiện idle animation khi pause)
        SyncAnimation();
    }

    /// <summary>
    /// Áp dụng NPCMovementConfig vào navigation agent
    /// </summary>
    private void ApplyConfig()
    {
        if (config == null) return;
        navAgent.SetSpeed(config.walkSpeed);
        navAgent.SetStoppingDistance(config.stoppingDistance);
        navAgent.SetAngularSpeed(config.rotationSpeed);
    }

    /// <summary>
    /// Đồng bộ animation - 0 = Idle, 1 = Walk
    /// </summary>
    private void SyncAnimation()
    {
        if (animController == null) return;
        float state = navAgent.GetCurrentSpeed() > 0.1f ? 1f : 0f;
        animController.UpdateAnimation(state);
    }

    // ============================
    // API CHO BEHAVIOR GỌI
    // (Behavior chỉ được gọi các hàm này)
    // ============================

    /// <summary>
    /// Di chuyển NPC đến vị trí đích
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        navAgent.MoveTo(destination);
    }

    /// <summary>
    /// Dừng NPC, xóa đường đi hiện tại
    /// </summary>
    public void Stop()
    {
        navAgent.Stop();
    }

    /// <summary>
    /// Kiểm tra NPC đã đến đích chưa
    /// </summary>
    public bool IsAtDestination()
    {
        return navAgent.HasReachedDestination();
    }

    /// <summary>
    /// Chuyển đổi giữa đi bộ và chạy
    /// </summary>
    public void SetRunning(bool running)
    {
        isRunning = running;
        if (config != null)
        {
            navAgent.SetSpeed(running ? config.runSpeed : config.walkSpeed);
        }
    }

    /// <summary>
    /// Tìm điểm ngẫu nhiên hợp lệ trên NavMesh - dùng cho WanderBehavior
    /// </summary>
    public bool GetRandomPoint(Vector3 center, float radius, out Vector3 point)
    {
        return navAgent.TryGetRandomPoint(center, radius, out point);
    }

    /// <summary>
    /// Chuyển sang behavior mới (tự động gọi Exit() cũ và Enter() mới)
    /// </summary>
    public void SetBehavior(NPCBehaviorBase newBehavior)
    {
        // Kết thúc behavior cũ
        if (currentBehavior != null)
        {
            currentBehavior.Exit();
        }

        currentBehavior = newBehavior;

        // Khởi tạo và bắt đầu behavior mới
        if (currentBehavior != null)
        {
            currentBehavior.Initialize(this);
            currentBehavior.Enter();
        }
    }

    /// <summary>
    /// Lấy behavior đang chạy - dùng cho Editor debug
    /// </summary>
    public NPCBehaviorBase GetCurrentBehavior()
    {
        return currentBehavior;
    }

    // ============================
    // TẠM DỪNG / TIẾP TỤC (dùng khi NPC interact, dialogue)
    // ============================

    /// <summary>
    /// Tạm dừng NPC - giữ nguyên trạng thái behavior, NPC đứng yên tại chỗ.
    /// Đường đi được giữ nguyên, Resume() sẽ đi tiếp từ vị trí hiện tại.
    /// </summary>
    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;
        navAgent.Pause();
    }

    /// <summary>
    /// Tiếp tục di chuyển sau khi Pause - NPC đi tiếp đường đi cũ.
    /// </summary>
    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
        navAgent.Resume();
    }

    /// <summary>
    /// Lấy tốc độ di chuyển hiện tại - dùng cho Editor debug
    /// </summary>
    public float GetCurrentSpeed()
    {
        return navAgent.GetCurrentSpeed();
    }
}
