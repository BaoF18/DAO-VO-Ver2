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

    private Camera cam;
    private IInteractable currentInteractable;

    private InputAction interactAction;

    void Awake()
    {
        interactAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/e");
        interactAction.Enable();
    }

    void Start()
    {
        cam = Camera.main;
        if (promptText != null)
            promptText.text = string.Empty;
    }

    void OnEnable()
    {
        interactAction.Enable();
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
            Component comp = currentInteractable as Component;
            if (comp != null)
            {
                float dist = Vector3.Distance(transform.position, comp.transform.position);
                if (dist > interactRange)
                {
                    currentInteractable.OnInteractEnd();
                    currentInteractable = null;
                    if (promptText != null)
                        promptText.text = string.Empty;
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
                if (promptText != null)
                    promptText.text = promptMessage;
                return;
            }
        }
        currentInteractable = null;
        if (promptText != null)
            promptText.text = string.Empty;
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
