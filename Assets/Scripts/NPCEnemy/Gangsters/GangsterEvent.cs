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

    [Header("Giao diện trên đầu (Chữ E)")]
    public GameObject overheadUI;
    public float showDistance = 4f; // Đứng cách 4 mét thì hiện

    private NavMeshAgent agent;
    private Collider gangsterCollider;
    private GameObject visualMesh;
    private Animator gangsterAnimator;
    private Transform playerTransform; // Đo khoảng cách với Player

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
            gangsterAnimator = visualMesh.GetComponent<Animator>();
        }

        if (visualMesh != null) visualMesh.SetActive(false);
        if (gangsterCollider != null) gangsterCollider.enabled = false;
        if (agent != null) agent.enabled = false;
    }

    void Start()
    {
        // Tự động tìm người chơi
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        // Tắt chữ E lúc mới vào game
        if (overheadUI != null) overheadUI.SetActive(false);
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

            if (gangsterAnimator != null)
            {
                float currentSpeed = agent.velocity.magnitude;
                gangsterAnimator.SetFloat("Speed", currentSpeed);
            }

            if (distanceToDoor <= 1.5f)
            {
                hasArrived = true;
                if (gangsterCollider != null) gangsterCollider.enabled = true;
                agent.isStopped = true;
                if (gangsterAnimator != null) gangsterAnimator.SetFloat("Speed", 0f);
            }
        }

        // ==========================================
        // LOGIC CHỮ E HIỆN TRÊN ĐẦU GIANG HỒ
        // ==========================================
        if (hasArrived && overheadUI != null && playerTransform != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Chỉ hiện chữ E khi: Đứng gần + Đang chưa vào hội thoại
            bool shouldShow = (distToPlayer <= showDistance) && !IsInteracting;
            overheadUI.SetActive(shouldShow);

            if (shouldShow && Camera.main != null)
            {
                overheadUI.transform.rotation = Camera.main.transform.rotation;
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

        // Tắt chữ E ngay khi bắt đầu nói chuyện
        if (overheadUI != null) overheadUI.SetActive(false);

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
                TaskManager.Instance.CompleteTask(TaskManager.Instance.CurrentTask);

                if (gangsterCollider != null) gangsterCollider.enabled = false;
                Destroy(gameObject, 1f);
            }
        }
    }

    public override void OnInteractEnd() { }
}