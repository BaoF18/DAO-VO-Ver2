using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class RepairStation : Interactable
{
    private enum StationState { Hidden, WaitPreTalk, WaitRepair, WaitPostTalk }
    private StationState currentState = StationState.Hidden;

    [Header("Giao diện & Mô hình tĩnh")]
    public GameObject staticBikeModel;
    public GameObject staticNpcModel;
    public TextMeshPro overheadLabel;

    [Header("UI Minigame")]
    public float repairDuration = 2.5f;
    public GameObject repairUIPanel;
    public Image fillImage;

    [Header("Sự kiện hoàn thành")]
    public UnityEvent onRepairComplete;

    private float holdTimer = 0f;
    private bool isWaitingForDialogueEnd = false;

    private VehicleJobConfig currentJob;

    // THÊM BIẾN QUẢN LÝ COLLIDER
    private Collider stationCollider;

    public override bool IsInteracting => DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive && isWaitingForDialogueEnd;

    void Awake()
    {
        // Tự động tìm Sphere Collider và tắt nó đi lúc mới vào game
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

        // Bật Collider lên để người chơi có thể tương tác
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
                case StationState.WaitRepair: overheadLabel.text = "Cần sửa: " + currentJob.jobName; break;
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

        if (Keyboard.current != null && Keyboard.current.fKey.isPressed)
        {
            holdTimer += Time.deltaTime;
            if (repairUIPanel != null) repairUIPanel.SetActive(true);
            if (fillImage != null) fillImage.fillAmount = holdTimer / repairDuration;

            if (holdTimer >= repairDuration)
            {
                holdTimer = 0f;
                if (repairUIPanel != null) repairUIPanel.SetActive(false);
                ChangeState(StationState.WaitPostTalk);
            }
        }
        else
        {
            holdTimer = Mathf.Max(0f, holdTimer - Time.deltaTime);
            if (fillImage != null) fillImage.fillAmount = holdTimer / repairDuration;
            if (holdTimer <= 0 && repairUIPanel != null) repairUIPanel.SetActive(false);
        }
    }

    private void FinishJob()
    {
        ChangeState(StationState.Hidden);

        if (staticBikeModel != null) staticBikeModel.SetActive(false);
        if (staticNpcModel != null) staticNpcModel.SetActive(false);
        if (stationCollider != null) stationCollider.enabled = false;

        currentJob = null;

        // Chỉ việc hét lên "Xong rồi!" thông qua Event, để Spawner tự đi mà đếm
        onRepairComplete?.Invoke();
    }

    public override void OnInteractEnd() { }
}