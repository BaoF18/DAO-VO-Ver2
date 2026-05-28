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
    public GameObject staticBikeModel;
    public GameObject staticNpcModel;
    public TextMeshPro overheadLabel;

    [Header("UI Minigame (Spam Phím)")]
    [Tooltip("Tổng điểm/thời gian cần đạt để sửa xong")]
    public float repairDuration = 5f;
    [Tooltip("Mỗi lần bấm F sẽ tăng bao nhiêu tiến độ")]
    public float fillPerTap = 0.5f;
    [Tooltip("Tốc độ tụt tiến độ mỗi giây nếu lười bấm")]
    public float drainRate = 1.0f;

    public GameObject repairUIPanel;
    public Image fillImage;

    [Header("Completion Event")]
    public UnityEvent onRepairComplete;

    private float holdTimer = 0f;
    private bool isWaitingForDialogueEnd = false;

    private VehicleJobConfig currentJob;
    private Collider stationCollider;

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

        if (staticBikeModel != null) staticBikeModel.SetActive(true);
        if (staticNpcModel != null) staticNpcModel.SetActive(true);

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

        // Sử dụng wasPressedThisFrame để bắt sự kiện nhấp phím (không tính giữ phím)
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            holdTimer += fillPerTap;
            if (repairUIPanel != null) repairUIPanel.SetActive(true);
        }
        else
        {
            // Trừ dần thanh tiến độ theo thời gian nếu không thao tác
            holdTimer -= drainRate * Time.deltaTime;
        }

        // Ép giá trị holdTimer không được tụt xuống dưới 0 và không vượt quá repairDuration
        holdTimer = Mathf.Clamp(holdTimer, 0f, repairDuration);

        // Cập nhật UI thanh máu/tiến độ
        if (fillImage != null) fillImage.fillAmount = holdTimer / repairDuration;

        // Ẩn panel nếu tiến độ tuột về 0
        if (holdTimer <= 0 && repairUIPanel != null) repairUIPanel.SetActive(false);

        // Kiểm tra điều kiện thắng minigame
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

        if (staticBikeModel != null) staticBikeModel.SetActive(false);
        if (staticNpcModel != null) staticNpcModel.SetActive(false);
        if (stationCollider != null) stationCollider.enabled = false;

        currentJob = null;
        onRepairComplete?.Invoke();
    }

    public override void OnInteractEnd() { }
}