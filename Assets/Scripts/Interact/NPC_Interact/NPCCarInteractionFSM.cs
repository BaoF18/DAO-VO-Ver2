using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class NPCCarInteractionFSM : Interactable
{
    // NEW: FSM state theo flow mới bắt buộc.
    private enum NPCState
    {
        DrivingToPoint,
        StoppingCar,
        ExitingCar,
        WaitingForPlayer,
        DialoguePlaying,
        ReadyForMiniGame,
        StandingUp,
        ShowingThanksText,
        EnteringCar,
        DrivingAway,
        Despawned
    }

    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string drivingTrigger = "Driving"; // NEW
    [SerializeField] private string stopTrigger = "Stop"; // NEW
    [SerializeField] private string exitCarTrigger = "ExitCar";
    [SerializeField] private string standUpTrigger = "StandUp";
    [SerializeField] private string enterCarTrigger = "EnterCar";
    [SerializeField] [Min(0.1f)] private float animationTimeoutSeconds = 4f;
    [SerializeField] [Min(0f)] private float stopBeforeExitSeconds = 0.75f; // NEW

    [Header("Thanks Text")]
    [SerializeField] private TextMeshProUGUI thanksText;
    [SerializeField] private string thanksMessage = "Cảm ơn";
    [SerializeField] [Range(1f, 2f)] private float thanksDurationSeconds = 1.5f;

    [Header("Drive Path")]
    [SerializeField] private WaypointPath driveToPointPath; // NEW
    [SerializeField] private WaypointPath driveAwayPath; // NEW
    [SerializeField] private NPCNavigationAgent navigationAgent;
    [SerializeField] [Min(1f)] private float movementTimeoutSeconds = 20f; // NEW

    [Header("Mini Game")]
    [SerializeField] [Range(2f, 3f)] private float holdFDurationSeconds = 2.5f; // NEW

    [Header("Events")]
    [SerializeField] private UnityEvent onInteractStarted;
    [SerializeField] private UnityEvent onDialogueFinished;
    [SerializeField] private UnityEvent onMiniGameCompleted;

    private NPCState state = NPCState.DrivingToPoint; // FIX
    private bool interactLocked;
    private int currentWaypointIndex = -1;
    private float moveCommandStartTime = -1f; // NEW
    private float miniGameHoldTimer; // NEW
    private WaypointPath activePath; // NEW

    private bool waitingAnimationEvent;
    private string expectedAnimationEvent;
    private NPCState expectedAnimationState; // NEW
    private Action pendingAnimationDone;
    private bool stateLock; // NEW

    private Coroutine animationTimeoutCoroutine;
    private Coroutine thanksCoroutine;
    private Coroutine stopThenExitCoroutine; // NEW

    public string CurrentState => state.ToString();

    public override bool IsInteracting =>
        state == NPCState.DialoguePlaying &&
        DialogueManager.Instance != null &&
        DialogueManager.Instance.IsDialogueActive;

    private void Awake()
    {
        HideThanksText();
    }

    private void Start()
    {
        if (!ValidateRequiredReferences())
        {
            enabled = false;
            return;
        }

        // NEW: spawn xong là lái ngay đến điểm nhiệm vụ.
        BeginDriveToPoint();
    }

    private void OnEnable()
    {
        DialogueManager.DialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        DialogueManager.DialogueEnded -= HandleDialogueEnded;

        if (animationTimeoutCoroutine != null)
        {
            StopCoroutine(animationTimeoutCoroutine);
            animationTimeoutCoroutine = null;
        }

        if (thanksCoroutine != null)
        {
            StopCoroutine(thanksCoroutine);
            thanksCoroutine = null;
        }

        if (stopThenExitCoroutine != null)
        {
            StopCoroutine(stopThenExitCoroutine);
            stopThenExitCoroutine = null;
        }

        waitingAnimationEvent = false;
        pendingAnimationDone = null;
        expectedAnimationEvent = null;
    }

    private void Update()
    {
        if (state == NPCState.DrivingToPoint || state == NPCState.DrivingAway)
        {
            TickDriving();
            return;
        }

        if (state == NPCState.ReadyForMiniGame)
        {
            TickMiniGame();
        }
    }

    public override void OnInteract()
    {
        if (!TryEnterStateLock()) return; // NEW
        try
        {
            if (interactLocked)
            {
                return;
            }

            if (state != NPCState.WaitingForPlayer)
            {
                return;
            }

            if (DialogueManager.Instance == null)
            {
                Debug.LogWarning("[NPCCarInteractionFSM] DialogueManager is missing.", this);
                return;
            }

            if (dialogueData == null)
            {
                Debug.LogWarning("[NPCCarInteractionFSM] dialogueData is not assigned.", this);
                return;
            }

            if (!TrySetState(NPCState.DialoguePlaying))
            {
                return;
            }

            interactLocked = true; // FIX: chống spam interact
            onInteractStarted?.Invoke();
            DialogueManager.Instance.StartDialogue(dialogueData);
        }
        finally
        {
            ExitStateLock();
        }
    }

    public override void OnInteractContinue()
    {
        if (!TryEnterStateLock()) return; // NEW
        try
        {
        if (state != NPCState.DialoguePlaying)
        {
            return;
        }

        if (DialogueManager.Instance == null)
        {
            return;
        }

        DialogueManager.Instance.ContinueDialogue();
        }
        finally
        {
            ExitStateLock();
        }
    }

    public override void OnInteractEnd()
    {
        // Không force-end dialogue để tránh skip logic.
    }

    private void HandleDialogueEnded()
    {
        if (state != NPCState.DialoguePlaying)
        {
            return;
        }

        OnDialogueFinished();
    }

    public void OnDialogueFinished()
    {
        if (!TryEnterStateLock()) return; // NEW
        try
        {
        if (state != NPCState.DialoguePlaying)
        {
            return;
        }

        onDialogueFinished?.Invoke();
        interactLocked = false;
        miniGameHoldTimer = 0f;
        TrySetState(NPCState.ReadyForMiniGame); // FIX
        }
        finally
        {
            ExitStateLock();
        }
    }

    private void HandleExitCarAnimationDone()
    {
        // FIX
        TrySetState(NPCState.WaitingForPlayer);
        interactLocked = false;
    }

    // Gọi trực tiếp từ mini-game khi player hoàn thành.
    public void OnMiniGameComplete()
    {
        if (!TryEnterStateLock()) return; // NEW
        try
        {
            if (state != NPCState.ReadyForMiniGame)
            {
                return;
            }

            onMiniGameCompleted?.Invoke();

            if (!TrySetState(NPCState.StandingUp))
            {
                return;
            }

            PlayAnimationAndWait(standUpTrigger, "STAND_UP_DONE", HandleStandUpAnimationDone);
        }
        finally
        {
            ExitStateLock();
        }
    }

    private void HandleStandUpAnimationDone()
    {
        if (!TrySetState(NPCState.ShowingThanksText))
        {
            return;
        }

        if (thanksCoroutine != null)
        {
            StopCoroutine(thanksCoroutine);
        }

        thanksCoroutine = StartCoroutine(ShowThanksThenEnterCar());
    }

    private IEnumerator ShowThanksThenEnterCar()
    {
        ShowThanksText();

        float clampedDuration = Mathf.Clamp(thanksDurationSeconds, 1f, 2f);
        yield return new WaitForSeconds(clampedDuration);

        thanksCoroutine = null;
        HideThanksText();

        if (state != NPCState.ShowingThanksText)
        {
            yield break;
        }

        if (!TrySetState(NPCState.EnteringCar))
        {
            yield break;
        }

        PlayAnimationAndWait(enterCarTrigger, "ENTER_CAR_DONE", BeginDriveAway); // FIX
    }

    private void BeginDriveToPoint() // NEW
    {
        state = NPCState.DrivingToPoint;
        activePath = driveToPointPath;
        BeginDrivePathInternal();
    }

    private void BeginDriveAway() // NEW
    {
        if (!TrySetState(NPCState.DrivingAway))
        {
            return;
        }

        activePath = driveAwayPath;
        BeginDrivePathInternal();
    }

    private void BeginDrivePathInternal() // NEW
    {
        if (activePath == null || activePath.Waypoints == null || activePath.Waypoints.Count == 0)
        {
            Debug.LogError("[NPCCarInteractionFSM] Active drive path is missing or empty.", this);
            return;
        }

        if (navigationAgent == null)
        {
            Debug.LogError("[NPCCarInteractionFSM] NPCNavigationAgent is missing.", this);
            return;
        }

        SetAnimatorTrigger(drivingTrigger);
        currentWaypointIndex = 0;
        MoveToCurrentWaypoint();
    }

    private void MoveToCurrentWaypoint()
    {
        if (activePath == null || activePath.Waypoints == null)
        {
            return;
        }

        if (currentWaypointIndex < 0 || currentWaypointIndex >= activePath.Waypoints.Count)
        {
            return;
        }

        Waypoint waypoint = activePath.Waypoints[currentWaypointIndex];
        if (waypoint == null)
        {
            Debug.LogWarning($"[NPCCarInteractionFSM] Waypoint at index {currentWaypointIndex} is null, skip.", this);
            MoveToNextWaypointOrFinalize();
            return;
        }

        moveCommandStartTime = Time.time;
        navigationAgent.MoveTo(waypoint.GetPosition());
    }

    private void MoveToNextWaypointOrFinalize() // FIX
    {
        if (activePath == null || activePath.Waypoints == null)
        {
            DespawnNow();
            return;
        }

        currentWaypointIndex++;
        if (currentWaypointIndex >= activePath.Waypoints.Count)
        {
            if (state == NPCState.DrivingToPoint)
            {
                HandleReachedDriveToPointEnd();
                return;
            }

            if (state == NPCState.DrivingAway)
            {
                DespawnNow();
            }

            return;
        }

        MoveToCurrentWaypoint();
    }

    private void TickDriving() // NEW
    {
        if (navigationAgent == null)
        {
            Debug.LogError("[NPCCarInteractionFSM] Missing NPCNavigationAgent while driving.", this);
            return;
        }

        if (moveCommandStartTime > 0f && Time.time - moveCommandStartTime >= movementTimeoutSeconds)
        {
            Debug.LogWarning($"[NPCCarInteractionFSM] Movement timeout at waypoint {currentWaypointIndex}. Skip next.", this);
            moveCommandStartTime = Time.time;
            MoveToNextWaypointOrFinalize();
            return;
        }

        if (navigationAgent.HasReachedDestination())
        {
            MoveToNextWaypointOrFinalize();
        }
    }

    private void HandleReachedDriveToPointEnd() // NEW
    {
        if (!TrySetState(NPCState.StoppingCar))
        {
            return;
        }

        navigationAgent.Stop();
        SetAnimatorTrigger(stopTrigger);

        if (stopThenExitCoroutine != null)
        {
            StopCoroutine(stopThenExitCoroutine);
        }

        stopThenExitCoroutine = StartCoroutine(StopThenExitRoutine());
    }

    private IEnumerator StopThenExitRoutine() // NEW
    {
        yield return new WaitForSeconds(stopBeforeExitSeconds);
        stopThenExitCoroutine = null;

        if (state != NPCState.StoppingCar)
        {
            yield break;
        }

        if (!TrySetState(NPCState.ExitingCar))
        {
            yield break;
        }

        PlayAnimationAndWait(exitCarTrigger, "EXIT_CAR_DONE", HandleExitCarAnimationDone);
    }

    private void TickMiniGame() // NEW
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            return;
        }

        if (kb.fKey.isPressed)
        {
            miniGameHoldTimer += Time.deltaTime;
            if (miniGameHoldTimer >= holdFDurationSeconds)
            {
                miniGameHoldTimer = 0f;
                OnMiniGameComplete();
            }
            return;
        }

        miniGameHoldTimer = 0f;
    }

    private void DespawnNow()
    {
        if (!TrySetState(NPCState.Despawned))
        {
            return;
        }

        Destroy(gameObject);
    }

    private void PlayAnimationAndWait(string triggerName, string doneEventName, Action onDone)
    {
        if (onDone == null)
        {
            return;
        }

        if (animator == null || string.IsNullOrWhiteSpace(triggerName))
        {
            Debug.LogError("[NPCCarInteractionFSM] Animator/trigger is missing, cannot play animation.", this);
            return;
        }

        waitingAnimationEvent = true;
        expectedAnimationEvent = doneEventName;
        expectedAnimationState = state;
        pendingAnimationDone = onDone;

        SetAnimatorTrigger(triggerName);

        if (animationTimeoutCoroutine != null)
        {
            StopCoroutine(animationTimeoutCoroutine);
        }

        animationTimeoutCoroutine = StartCoroutine(AnimationTimeoutRoutine(doneEventName));
    }

    private IEnumerator AnimationTimeoutRoutine(string eventName)
    {
        yield return new WaitForSeconds(animationTimeoutSeconds);

        animationTimeoutCoroutine = null;

        if (!waitingAnimationEvent)
        {
            yield break;
        }

        Debug.LogWarning($"[NPCCarInteractionFSM] Animation event timeout: {eventName}. Continue by fallback.", this);
        CompletePendingAnimation();
    }

    // Gọi hàm này bằng Animation Event ở frame cuối clip animation.
    public void OnNpcAnimationFinished(string eventName)
    {
        if (!TryEnterStateLock()) return; // NEW
        try
        {
        if (!waitingAnimationEvent)
        {
            return;
        }

        if (!string.Equals(expectedAnimationEvent, eventName, StringComparison.Ordinal))
        {
            return;
        }

        if (state != expectedAnimationState)
        {
            return;
        }

        CompletePendingAnimation();
        }
        finally
        {
            ExitStateLock();
        }
    }

    private void CompletePendingAnimation()
    {
        waitingAnimationEvent = false;
        expectedAnimationEvent = null;
        expectedAnimationState = state;

        if (animationTimeoutCoroutine != null)
        {
            StopCoroutine(animationTimeoutCoroutine);
            animationTimeoutCoroutine = null;
        }

        Action callback = pendingAnimationDone;
        pendingAnimationDone = null;
        callback?.Invoke();
    }

    private void ShowThanksText()
    {
        if (thanksText == null)
        {
            return;
        }

        thanksText.text = thanksMessage;
        thanksText.gameObject.SetActive(true);
    }

    private void HideThanksText()
    {
        if (thanksText == null)
        {
            return;
        }

        thanksText.gameObject.SetActive(false);
    }

    private bool TrySetState(NPCState next)
    {
        if (!IsValidTransition(state, next))
        {
            Debug.LogWarning($"[NPCCarInteractionFSM] Invalid transition: {state} -> {next}", this);
            return false;
        }

        state = next;
        return true;
    }

    private bool IsValidTransition(NPCState from, NPCState to)
    {
        switch (from)
        {
            case NPCState.DrivingToPoint:
                return to == NPCState.StoppingCar;
            case NPCState.StoppingCar:
                return to == NPCState.ExitingCar;
            case NPCState.ExitingCar:
                return to == NPCState.WaitingForPlayer;
            case NPCState.WaitingForPlayer:
                return to == NPCState.DialoguePlaying;
            case NPCState.DialoguePlaying:
                return to == NPCState.ReadyForMiniGame;
            case NPCState.ReadyForMiniGame:
                return to == NPCState.StandingUp;
            case NPCState.StandingUp:
                return to == NPCState.ShowingThanksText;
            case NPCState.ShowingThanksText:
                return to == NPCState.EnteringCar;
            case NPCState.EnteringCar:
                return to == NPCState.DrivingAway;
            case NPCState.DrivingAway:
                return to == NPCState.Despawned;
            case NPCState.Despawned:
                return false;
            default:
                return false;
        }
    }

    private bool ValidateRequiredReferences() // NEW
    {
        AutoAssignMissingReferences();

        bool valid = true;

        if (animator == null)
        {
            Debug.LogError("[NPCCarInteractionFSM] Missing Animator reference.", this);
            valid = false;
        }

        if (navigationAgent == null)
        {
            Debug.LogError("[NPCCarInteractionFSM] Missing NPCNavigationAgent reference.", this);
            valid = false;
        }

        if (driveToPointPath == null)
        {
            Debug.LogError("[NPCCarInteractionFSM] Missing Drive To Point Path reference.", this);
            valid = false;
        }

        if (driveAwayPath == null)
        {
            Debug.LogError("[NPCCarInteractionFSM] Missing Drive Away Path reference.", this);
            valid = false;
        }

        return valid;
    }

    private void AutoAssignMissingReferences()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (navigationAgent == null)
        {
            navigationAgent = GetComponent<NPCNavigationAgent>();
        }

        if (driveToPointPath != null && driveAwayPath != null)
        {
            return;
        }

        WaypointPath[] allPaths = FindObjectsByType<WaypointPath>(FindObjectsSortMode.None);
        if (allPaths == null || allPaths.Length == 0)
        {
            return;
        }

        int nearestIndex = -1;
        int secondNearestIndex = -1;
        float nearestDistance = float.MaxValue;
        float secondNearestDistance = float.MaxValue;

        for (int i = 0; i < allPaths.Length; i++)
        {
            WaypointPath candidate = allPaths[i];
            if (candidate == null)
            {
                continue;
            }

            float distance = GetPathDistanceSqr(candidate);
            if (distance < nearestDistance)
            {
                secondNearestDistance = nearestDistance;
                secondNearestIndex = nearestIndex;

                nearestDistance = distance;
                nearestIndex = i;
            }
            else if (distance < secondNearestDistance)
            {
                secondNearestDistance = distance;
                secondNearestIndex = i;
            }
        }

        if (driveToPointPath == null && nearestIndex >= 0)
        {
            driveToPointPath = allPaths[nearestIndex];
        }

        if (driveAwayPath == null)
        {
            if (secondNearestIndex >= 0)
            {
                driveAwayPath = allPaths[secondNearestIndex];
            }
            else if (nearestIndex >= 0)
            {
                driveAwayPath = allPaths[nearestIndex];
            }
        }
    }

    private float GetPathDistanceSqr(WaypointPath path)
    {
        if (path == null)
        {
            return float.MaxValue;
        }

        if (path.Waypoints == null || path.Waypoints.Count == 0)
        {
            return (path.transform.position - transform.position).sqrMagnitude;
        }

        float minDistance = float.MaxValue;
        Vector3 origin = transform.position;

        for (int i = 0; i < path.Waypoints.Count; i++)
        {
            Waypoint waypoint = path.Waypoints[i];
            if (waypoint == null)
            {
                continue;
            }

            float dist = (waypoint.GetPosition() - origin).sqrMagnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
            }
        }

        if (minDistance == float.MaxValue)
        {
            return (path.transform.position - origin).sqrMagnitude;
        }

        return minDistance;
    }

    private void SetAnimatorTrigger(string triggerName) // NEW
    {
        if (animator == null || string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        animator.SetTrigger(triggerName);
    }

    private bool TryEnterStateLock() // NEW
    {
        if (stateLock)
        {
            return false;
        }

        stateLock = true;
        return true;
    }

    private void ExitStateLock() // NEW
    {
        stateLock = false;
    }
}
