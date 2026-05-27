using UnityEngine;
using UnityEngine.AI;

public class GangsterEvent : Interactable
{
    [Header("Cấu hình Task")]
    public string requiredTaskId = "4";

    [Header("Điểm Di Chuyển")]
    public Transform spawnPoint;
    public Transform doorDestination;

    [Header("Dữ Liệu Thoại")]
    public DialogueData gangsterDialogue;

    private NavMeshAgent agent;
    private Collider gangsterCollider;
    private GameObject visualMesh;
    private Animator gangsterAnimator; //ANIMATOR

    private bool isSpawned = false;
    private bool hasArrived = false;
    private bool isWaitingForDialogueEnd = false;

    public override bool IsInteracting => DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive && isWaitingForDialogueEnd;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        gangsterCollider = GetComponent<Collider>();

        if (transform.childCount > 0)
        {
            visualMesh = transform.GetChild(0).gameObject;
            // TỰ ĐỘNG TÌM ANIMATOR TRÊN CƠ THỂ ĐỨA CON
            gangsterAnimator = visualMesh.GetComponent<Animator>();
        }

        if (visualMesh != null) visualMesh.SetActive(false);
        if (gangsterCollider != null) gangsterCollider.enabled = false;
        if (agent != null) agent.enabled = false;
    }

    void OnEnable() { DialogueManager.DialogueEnded += OnDialogueFinished; }
    void OnDisable() { DialogueManager.DialogueEnded -= OnDialogueFinished; }

    void Update()
    {
        if (!isSpawned && TaskManager.Instance != null && TaskManager.Instance.CurrentTask != null)
        {
            if (TaskManager.Instance.CurrentTask.Id == requiredTaskId)
            {
                TriggerGangsterArrival();
            }
        }

        if (isSpawned && !hasArrived && agent != null && agent.enabled && doorDestination != null)
        {
            float distanceToDoor = Vector3.Distance(transform.position, doorDestination.position);

            // ==========================================================
            // LOGIC ANIMATION: NẾU ĐANG DI CHUYỂN THÌ ĐỔI THOẠI WALK
            if (gangsterAnimator != null)
            {
                // Lấy tốc độ thực tế của NavMeshAgent
                float currentSpeed = agent.velocity.magnitude;
                // Truyền tốc độ vào biến "Speed" trong Animator
                // (Nếu Speed > 0.1 -> Chuyển sang Walk, nếu = 0 -> Về Idle)
                gangsterAnimator.SetFloat("Speed", currentSpeed);
            }
            // ==========================================================

            if (distanceToDoor <= 1.5f)
            {
                hasArrived = true;
                if (gangsterCollider != null) gangsterCollider.enabled = true;

                // Dừng agent
                agent.isStopped = true;

                // ÉP VỀ IDLE LÚC TỚI NƠI
                if (gangsterAnimator != null) gangsterAnimator.SetFloat("Speed", 0f);

                Debug.Log("[GangsterEvent] Giang hồ đã đến điểm hẹn và đang đứng đợi!");
            }
        }
    }

    private void TriggerGangsterArrival()
    {
        isSpawned = true;

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        if (visualMesh != null) visualMesh.SetActive(true);

        if (agent != null)
        {
            agent.enabled = true;
            if (doorDestination != null)
            {
                agent.SetDestination(doorDestination.position);
            }
        }
    }

    public override void OnInteract()
    {
        if (!hasArrived || DialogueManager.Instance == null || gangsterDialogue == null) return;
        isWaitingForDialogueEnd = true;
        DialogueManager.Instance.StartDialogue(gangsterDialogue);
    }

    public override void OnInteractContinue()
    {
        if (DialogueManager.Instance != null) DialogueManager.Instance.ContinueDialogue();
    }

    private void OnDialogueFinished()
    {
        if (!isWaitingForDialogueEnd) return;
        isWaitingForDialogueEnd = false;

        if (TaskManager.Instance != null && TaskManager.Instance.CurrentTask != null)
        {
            if (TaskManager.Instance.CurrentTask.Id == requiredTaskId)
            {
                // 1. Báo cáo hoàn thành Task 4
                TaskManager.Instance.CompleteTask(TaskManager.Instance.CurrentTask);
                Debug.Log("[GangsterEvent] Hoàn thành Task 4 thành công!");

                // 2. Tắt ngay Collider để người chơi không thể spam phím E được nữa
                if (gangsterCollider != null) gangsterCollider.enabled = false;

                // 3. Cho hắn tàng hình/bốc hơi khỏi game sau 1 giây (để đỡ bị giật cục)
                Destroy(gameObject, 1f);
            }
        }
    }

    public override void OnInteractEnd() { }
}