using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Hệ thống Interact của Player - hoàn toàn generic.
/// Chỉ giao tiếp qua IInteractable, không biết về Dialogue hay NPC.
/// </summary>
public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactLayerMask = ~0;
    public TextMeshProUGUI promptText;
    public string promptMessage = "Press E";
    [SerializeField] private bool autoAssignPromptText = true;

    private Camera cam;
    private IInteractable currentInteractable;

    private InputAction interactAction;

    void Awake()
    {
        interactAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/e");
        interactAction.Enable();

        TryAutoAssignPromptText();
    }

    void Start()
    {
        cam = Camera.main;
        TryAutoAssignPromptText();
        SetPrompt(string.Empty);
    }

    void OnEnable()
    {
        interactAction.Enable();
        TryAutoAssignPromptText();
    }

    void OnDisable()
    {
        interactAction.Disable();
    }

    void Update()
    {
        CheckForInteractable();
        HandleInteractInput();
    }

    void HandleInteractInput()
    {
        if (interactAction.WasPressedThisFrame())
        {
            TryInteract();
        }
    }

    void CheckForInteractable()
    {
        // Khi đang tương tác, kiểm tra khoảng cách thay vì raycast
        // Để player quay mặt đi vẫn tương tác, nhưng rời xa thì ngừng
        if (currentInteractable != null && currentInteractable.IsInteracting)
        {
            SetPrompt(string.Empty);
            Component comp = currentInteractable as Component;
            if (comp != null)
            {
                float dist = Vector3.Distance(transform.position, comp.transform.position);
                if (dist > interactRange)
                {
                    currentInteractable.OnInteractEnd();
                    currentInteractable = null;
                    SetPrompt(string.Empty);
                }
            }
            return;
        }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayerMask, QueryTriggerInteraction.Collide))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                SetPrompt(promptMessage);
                return;
            }
        }
        currentInteractable = null;
        SetPrompt(string.Empty);
    }

    private void SetPrompt(string value)
    {
        EnsurePromptTarget();

        if (promptText == null)
        {
            Debug.LogWarning("[PlayerInteract] promptText is NULL. Cannot render prompt.");
            return;
        }

        bool hasValue = !string.IsNullOrEmpty(value);
        if (promptText.gameObject.activeSelf != hasValue)
        {
            promptText.gameObject.SetActive(hasValue);
        }

        if (promptText.enabled != hasValue)
        {
            promptText.enabled = hasValue;
        }

        promptText.text = value;
    }

    private void EnsurePromptTarget()
    {
        if (promptText == null || !promptText.gameObject.activeInHierarchy)
        {
            TryAutoAssignPromptText();
        }

        EnsurePromptNotUnderDialoguePanel();
    }

    private void EnsurePromptNotUnderDialoguePanel()
    {
        if (promptText == null)
        {
            return;
        }

        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null || dialogueManager.dialoguePanel == null)
        {
            return;
        }

        if (!promptText.transform.IsChildOf(dialogueManager.dialoguePanel.transform))
        {
            return;
        }

        Transform newParent = dialogueManager.dialoguePanel.transform.parent;
        if (newParent == null)
        {
            Debug.LogWarning("[PlayerInteract] Prompt text is under DialoguePanel with no parent. Cannot reparent.");
            return;
        }

        promptText.transform.SetParent(newParent, true);
        Debug.LogWarning($"[PlayerInteract] Prompt text moved out of DialoguePanel to '{newParent.name}'.");
    }

    private void TryAutoAssignPromptText()
    {
        if (!autoAssignPromptText && promptText != null)
        {
            return;
        }

        if (promptText != null && promptText.gameObject.activeInHierarchy)
        {
            return;
        }

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        TextMeshProUGUI fallback = null;

        for (int i = 0; i < allTexts.Length; i++)
        {
            TextMeshProUGUI candidate = allTexts[i];
            if (candidate == null)
            {
                continue;
            }

            string lowerName = candidate.name.ToLowerInvariant();
            bool looksLikePrompt = lowerName.Contains("press") || lowerName.Contains("interact");
            if (!looksLikePrompt)
            {
                continue;
            }

            if (IsUnderDialoguePanel(candidate.transform))
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = candidate;
            }

            if (candidate.gameObject.activeInHierarchy)
            {
                promptText = candidate;
                return;
            }
        }

        if (promptText == null)
        {
            promptText = fallback;
        }

        if (promptText != null)
        {
            Debug.Log($"[PlayerInteract] Auto-assign promptText => {promptText.name}");
        }
        else
        {
            Debug.LogWarning("[PlayerInteract] Auto-assign promptText failed. No candidate found.");
        }
    }

    private bool IsUnderDialoguePanel(Transform target)
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager != null)
        {
            if (dialogueManager.nameText == target || dialogueManager.dialogueText == target)
            {
                return true;
            }

            if (dialogueManager.dialoguePanel != null && target.IsChildOf(dialogueManager.dialoguePanel.transform))
            {
                return true;
            }
        }

        return false;
    }

    void TryInteract()
    {
        if (currentInteractable == null) return;

        if (currentInteractable.IsInteracting)
        {
            currentInteractable.OnInteractContinue();
        }
        else
        {
            currentInteractable.OnInteract();
        }
    }
}
