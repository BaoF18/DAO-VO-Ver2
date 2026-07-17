using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class DrunkNPC : Interactable
{
    public enum DrunkState { Idle, Wander, Talking }
    [Header("Current State")]
    public DrunkState currentState = DrunkState.Idle;

    [Header("--- WANDERING ---")]
    public Transform wanderCenter;
    public float wanderRadius = 15f;
    public float minIdleTime = 3f;
    public float maxIdleTime = 7f;
    private float idleTimer;

    [Header("--- COMPONENTS ---")]
    [Tooltip("Không nhất thiết kéo, sẽ tự tìm được trong components đã attached với object này")]
    public NavMeshAgent agent;
    public Animator anim;
    public AudioSource audioSource;

    [Header("--- INTERACTION & DIALOGUE ---")]
    public TextMeshPro overheadLabel;
    public string interactText = "E";
    public float showDistance = 3f;

    [Tooltip("Kéo data hội thoại của NPC này vào đây.")]
    public DialogueData drunkDialogueData;

    [Header("--- AUDIO ---")]
    public AudioClip[] mumbleSounds;
    [Tooltip("Khoảng thời gian nín lặng")]
    public float minMumblePause = 1f;
    public float maxMumblePause = 10f;
    private float mumblePauseTimer;

    private Transform playerTransform;
    private bool isWaitingForDialogueEnd = false;

    // Ghi đè biến kiểm tra trạng thái tương tác từ class Interactable
    public override bool IsInteracting => DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive && isWaitingForDialogueEnd;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (wanderCenter == null)
        {
            GameObject centerObj = new GameObject("WanderCenter_" + gameObject.name);
            centerObj.transform.position = transform.position;
            wanderCenter = centerObj.transform;
        }

        if (overheadLabel != null)
        {
            overheadLabel.text = interactText;
            overheadLabel.gameObject.SetActive(false);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        mumblePauseTimer = Random.Range(minMumblePause, maxMumblePause);
        SetState(DrunkState.Idle);
    }

    // Lắng nghe sự kiện kết thúc hội thoại giống y hệt RepairStation
    void OnEnable() { DialogueManager.DialogueEnded += OnDialogueFinished; }
    void OnDisable() { DialogueManager.DialogueEnded -= OnDialogueFinished; }

    void Update()
    {
        // Tính toán tốc độ để truyền vào Animator (Chuyển đổi giữa DrunkIdle và DrunkWalk)
        if (anim != null) anim.SetFloat("Speed", agent.velocity.magnitude);

        HandleMumbling();

        switch (currentState)
        {
            case DrunkState.Idle:
                HandleIdle();
                break;
            case DrunkState.Wander:
                HandleWander();
                break;
            case DrunkState.Talking:
                // Đang nói chuyện thì NPC sẽ đứng im nhìn sếp, không AI dò đường nữa
                FaceTarget(playerTransform.position);
                break;
        }
    }

    void LateUpdate()
    {
        // Ẩn/Hiện chữ "Nói chuyện (E)" khi đứng gần
        if (overheadLabel != null && Camera.main != null && playerTransform != null)
        {
            if (currentState != DrunkState.Talking)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                overheadLabel.gameObject.SetActive(distance <= showDistance);
            }
            else
            {
                overheadLabel.gameObject.SetActive(false); // Đang nói chuyện thì giấu chữ đi
            }

            if (overheadLabel.gameObject.activeSelf)
            {
                overheadLabel.transform.LookAt(Camera.main.transform);
                overheadLabel.transform.Rotate(0, 180, 0);
            }
        }
    }

    // ==========================================
    // LOGIC ÂM THANH: CHỜ PHÁT HẾT MỚI TÍNH TIẾP
    // ==========================================
    void HandleMumbling()
    {
        // 1. NẾU ĐANG PHÁT TIẾNG -> DỪNG LẠI, KHÔNG LÀM GÌ CẢ
        if (audioSource.isPlaying) return;

        // 2. NẾU ĐÃ PHÁT XONG -> BẮT ĐẦU ĐẾM NGƯỢC THỜI GIAN NGHỈ NGƠI
        mumblePauseTimer -= Time.deltaTime;

        if (mumblePauseTimer <= 0)
        {
            if (mumbleSounds != null && mumbleSounds.Length > 0)
            {
                // Phát random một câu mới
                audioSource.clip = mumbleSounds[Random.Range(0, mumbleSounds.Length)];
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Giọng lúc trầm lúc bổng cho giống say
                audioSource.Play();
            }
            // 3. ĐẶT LẠI THỜI GIAN NGHỈ NGẪU NHIÊN CHO CÂU TIẾP THEO
            mumblePauseTimer = Random.Range(minMumblePause, maxMumblePause);
        }
    }

    // ==========================================
    // LOGIC AI: ĐI LANG THANG
    // ==========================================
    void HandleIdle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            SetState(DrunkState.Wander);
        }
    }

    void HandleWander()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetState(DrunkState.Idle);
        }
    }

    void SetState(DrunkState newState)
    {
        currentState = newState;

        if (newState == DrunkState.Idle)
        {
            agent.isStopped = true;
            idleTimer = Random.Range(minIdleTime, maxIdleTime);
        }
        else if (newState == DrunkState.Wander)
        {
            agent.isStopped = false;
            Vector3 randomPoint = GetRandomNavMeshPoint(wanderCenter.position, wanderRadius);
            agent.SetDestination(randomPoint);
        }
    }

    Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius;
        randomDir += center;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDir, out hit, radius, 1);
        return hit.position;
    }

    // ==========================================
    // LOGIC TƯƠNG TÁC: NÓI CHUYỆN
    // ==========================================
    public override void OnInteract()
    {
        if (currentState == DrunkState.Talking) return;
        if (DialogueManager.Instance == null || drunkDialogueData == null) return;

        // Ép ông say dừng bước
        agent.isStopped = true;
        currentState = DrunkState.Talking;

        // Tắt tiếng lải nhải khi đang vào hội thoại (nếu muốn)
        audioSource.Stop();

        // Bắt đầu nhả Dialogue
        isWaitingForDialogueEnd = true;

        // LƯU Ý: Sếp đổi biến drunkDialogueData sang đúng chuẩn của DialogueManager nhé
        DialogueManager.Instance.StartDialogue(drunkDialogueData);
    }

    public override void OnInteractContinue()
    {
        if (DialogueManager.Instance != null) DialogueManager.Instance.ContinueDialogue();
    }

    private void OnDialogueFinished()
    {
        if (!isWaitingForDialogueEnd) return;
        isWaitingForDialogueEnd = false;

        // Nói chuyện xong thì tha cho đi lang thang tiếp
        SetState(DrunkState.Idle);
    }

    void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 3f);
        }
    }

    public override void OnInteractEnd() { }
}
