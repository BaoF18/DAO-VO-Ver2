using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class RepairStation : Interactable
{
    private enum StationState { Hidden, WaitPreTalk, WaitRepair, WaitPostTalk }
    private StationState currentState = StationState.Hidden;

    [Header("Static models")]
    [Tooltip("Danh sách các mô hình xe tĩnh (Element 0: Xe 1, Element 1: Xe 2...)")]
    public GameObject[] staticBikeModels;

    [Tooltip("Danh sách các mô hình NPC tĩnh (Element 0: Khách 1, Element 1: Khách 2...)")]
    public GameObject[] staticNpcModels;

    public TextMeshPro overheadLabel;

    [Header("UI Minigame (Spam Phím)")]
    public float repairDuration = 5f;
    public float fillPerTap = 0.5f;
    public float drainRate = 1.0f;
    public GameObject repairUIPanel;
    public Image fillImage;

    [Header("Completion Event")]
    public UnityEvent onRepairComplete;

    private float holdTimer = 0f;
    private bool isWaitingForDialogueEnd = false;

    private VehicleJobConfig currentJob;
    private Collider stationCollider;

    // =========================================================
    // BIẾN ĐẾM TỰ ĐỘNG: Chiếc đầu tiên sẽ là 0, xong tăng lên 1, 2...
    private int currentJobIndex = 0;
    // =========================================================

    public override bool IsInteracting => DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive && isWaitingForDialogueEnd;

    void Awake()
    {
        stationCollider = GetComponent<Collider>();
        if (stationCollider != null) stationCollider.enabled = false;
    }

    void OnEnable() { DialogueManager.DialogueEnded += OnDialogueFinished; }
    void OnDisable() { DialogueManager.DialogueEnded -= OnDialogueFinished; }

    public void SetupAndShowJob(VehicleJobConfig jobConfig)
    {
        if (jobConfig == null)
        {
            Debug.LogWarning("Chiếc xe này không có script VehicleJobConfig!");
            return;
        }

        currentJob = jobConfig;

        // CHỈ BẬT ĐÚNG chiếc xe tương ứng với thứ tự hiện tại
        if (staticBikeModels != null && currentJobIndex < staticBikeModels.Length)
        {
            if (staticBikeModels[currentJobIndex] != null)
                staticBikeModels[currentJobIndex].SetActive(true);
        }

        // CHỈ BẬT ĐÚNG ông NPC tương ứng với thứ tự hiện tại
        if (staticNpcModels != null && currentJobIndex < staticNpcModels.Length)
        {
            if (staticNpcModels[currentJobIndex] != null)
                staticNpcModels[currentJobIndex].SetActive(true);
        }

        if (stationCollider != null) stationCollider.enabled = true;

        ChangeState(StationState.WaitPreTalk);
    }

    private void ChangeState(StationState newState)
    {
        currentState = newState;

        if (overheadLabel != null)
        {
            overheadLabel.gameObject.SetActive(true);

            switch (newState)
            {
                case StationState.WaitPreTalk: overheadLabel.text = "Nói chuyện (E)"; break;
                case StationState.WaitRepair: overheadLabel.text = "Cần sửa: " + currentJob.jobName + " (Spam F)"; break;
                case StationState.WaitPostTalk: overheadLabel.text = "Tính tiền (E)"; break;
                case StationState.Hidden: overheadLabel.gameObject.SetActive(false); break;
            }
        }
    }

    public override void OnInteract()
    {
        if (DialogueManager.Instance == null || currentJob == null) return;

        if (currentState == StationState.WaitPreTalk && currentJob.preRepairDialogue != null)
        {
            isWaitingForDialogueEnd = true;
            DialogueManager.Instance.StartDialogue(currentJob.preRepairDialogue);
        }
        else if (currentState == StationState.WaitPostTalk && currentJob.postRepairDialogue != null)
        {
            isWaitingForDialogueEnd = true;
            DialogueManager.Instance.StartDialogue(currentJob.postRepairDialogue);
        }
    }

    public override void OnInteractContinue()
    {
        if (DialogueManager.Instance != null) DialogueManager.Instance.ContinueDialogue();
    }

    private void OnDialogueFinished()
    {
        if (!isWaitingForDialogueEnd) return;
        isWaitingForDialogueEnd = false;

        if (currentState == StationState.WaitPreTalk) ChangeState(StationState.WaitRepair);
        else if (currentState == StationState.WaitPostTalk) FinishJob();
    }

    void Update()
    {
        if (currentState != StationState.WaitRepair) return;

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            holdTimer += fillPerTap;
            if (repairUIPanel != null) repairUIPanel.SetActive(true);
        }
        else
        {
            holdTimer -= drainRate * Time.deltaTime;
        }

        holdTimer = Mathf.Clamp(holdTimer, 0f, repairDuration);

        if (fillImage != null) fillImage.fillAmount = holdTimer / repairDuration;

        if (holdTimer <= 0 && repairUIPanel != null) repairUIPanel.SetActive(false);

        if (holdTimer >= repairDuration)
        {
            holdTimer = 0f;
            if (repairUIPanel != null) repairUIPanel.SetActive(false);
            ChangeState(StationState.WaitPostTalk);
        }
    }

    private void FinishJob()
    {
        ChangeState(StationState.Hidden);

        // TẮT ĐÚNG chiếc xe vừa sửa xong
        if (staticBikeModels != null && currentJobIndex < staticBikeModels.Length)
        {
            if (staticBikeModels[currentJobIndex] != null)
                staticBikeModels[currentJobIndex].SetActive(false);
        }

        // TẮT ĐÚNG ông NPC vừa tính tiền xong
        if (staticNpcModels != null && currentJobIndex < staticNpcModels.Length)
        {
            if (staticNpcModels[currentJobIndex] != null)
                staticNpcModels[currentJobIndex].SetActive(false);
        }

        if (stationCollider != null) stationCollider.enabled = false;

        currentJob = null;
        onRepairComplete?.Invoke();

        // =========================================================
        // TĂNG BIẾN ĐẾM: Sửa xong xe này thì lượt sau sẽ gọi xe + NPC kế tiếp
        currentJobIndex++;
        // =========================================================
    }

    public override void OnInteractEnd() { }
}