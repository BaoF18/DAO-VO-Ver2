
using UnityEngine;
using UnityEngine.InputSystem;


public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform followTarget;

    [Header("Distance | Framing")]
    [SerializeField] public float distance = 3f;
    [SerializeField] public Vector2 framingOffet;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity;

    [Header("Look Up/Down Limits")]
    [Tooltip("Maximum vertical angle (look up).")]
    [SerializeField] public float maxVerticalAngel = 60f;
    [Tooltip("Minimum vertical angle (look down)")]
    [SerializeField] public float minVerticalAngel = -40f;

    [Header("Input")]
    [Tooltip("Input Action for looking around.")]
    [SerializeField] private InputActionReference lookActionReference;


    private float rotationY;
    private float rotationX;




    private void OnEnable()
    {
        if(lookActionReference != null && lookActionReference.action != null)
        {
            lookActionReference.action.Enable();
        }
    }
    private void OnDisable()
    {
        if(lookActionReference != null && lookActionReference.action != null)
        {
            lookActionReference.action.Disable();
        }
    }


    private void Start()
    {
        // Lock the cursor to the center of the screen
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        Vector2 delta = Vector2.zero;
        // Read input from the look action
        if (lookActionReference != null && lookActionReference.action != null)
        {
            delta = lookActionReference.action.ReadValue<Vector2>();

        }
        // Fallback to Mouse.current if no input from action
        if (Mouse.current != null)
        {
            delta = Mouse.current.delta.ReadValue();
        }
        // Update rotation based on mouse movement
        rotationY += delta.x * mouseSensitivity;
        rotationX -= delta.y * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngel, maxVerticalAngel);
        
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        // Calculate the focus point with framing offset
        var focusPoint = followTarget.position + new Vector3(framingOffet.x, framingOffet.y);
        // Make the camera follow the target at a fixed "distance" and "heightCamera"
        transform.position = focusPoint - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }
    // Expose the planar rotation (rotation around the Y-axis) for external use
    public Quaternion planarRotation => Quaternion.Euler(0, rotationY, 0);

}
