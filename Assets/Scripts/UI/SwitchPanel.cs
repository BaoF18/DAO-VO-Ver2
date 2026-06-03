using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SwitchPanel : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Danh sách các UI panel (GameObject) s? ???c b?t khi OPEN (l?u ý: n?u tr??c ?ây b? ng??c, behavior ?ã ???c ??o l?i ? ?ây)")]
    public List<GameObject> panelsToOpen = new List<GameObject>();

    [Tooltip("Danh sách các UI panel (GameObject) s? ???c b?t khi CLOSED")]
    public List<GameObject> panelsToClose = new List<GameObject>();

    [Header("Toggle Keys")]
    [Tooltip("Danh sách các phím (Input System Key) có th? dùng ?? toggle panel. Ch?n nhi?u phím n?u mu?n.")]
    public List<Key> toggleKeys = new List<Key> { Key.Tab };

    [Header("Initial State")]
    [Tooltip("N?u true thì s? b?t ??u ? tr?ng thái open (panelsToOpen active)")]
    public bool startOpen = false;

    [Header("Cursor")]
    [Tooltip("N?u true thì khi m? panel s? hi?n con tr? chu?t (unlock + visible). Khi ?óng panel s? ?n và khóa con tr? (lock + invisible).")]
    public bool unlockCursorOnOpen = true;

    [Header("UI Buttons")]
    [Tooltip("Danh sách các Button UI mà khi b?n nh?n s? ?n và khoá con tr? ?? tr? v? gameplay.")]
    public List<Button> uiButtonsHideCursor = new List<Button>();

    // Internal state
    private bool isOpen = false;

    private void Start()
    {
        // Set initial state according to startOpen
        SetOpenState(startOpen);

        // Register click listeners for configured UI buttons
        if (uiButtonsHideCursor != null)
        {
            foreach (var btn in uiButtonsHideCursor)
            {
                if (btn == null) continue;
                btn.onClick.AddListener(OnUiButtonClicked);
            }
        }
    }

    private void OnDestroy()
    {
        // Unregister listeners to avoid memory leaks
        if (uiButtonsHideCursor != null)
        {
            foreach (var btn in uiButtonsHideCursor)
            {
                if (btn == null) continue;
                btn.onClick.RemoveListener(OnUiButtonClicked);
            }
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Check any selected key was pressed this frame
        foreach (var k in toggleKeys)
        {
            // safety: skip unspecified keys
            if (k == Key.None) continue;

            // Use Input System keyboard lookup
            var keyControl = keyboard[k];
            if (keyControl != null && keyControl.wasPressedThisFrame)
            {
                Toggle();
                break;
            }
        }
    }

    /// <summary>
    /// Toggle tr?ng thái gi?a open và closed
    /// </summary>
    public void Toggle()
    {
        SetOpenState(!isOpen);
    }

    /// <summary>
    /// ??t tr?ng thái m?/?óng. L?u ý: panelsToOpen s? active khi open==true, panelsToClose s? active khi open==false.
    /// </summary>
    /// <param name="open"></param>
    public void SetOpenState(bool open)
    {
        isOpen = open;

        if (isOpen)
        {
            // Activate panelsToOpen
            if (panelsToOpen != null)
            {
                foreach (var go in panelsToOpen)
                {
                    if (go == null) continue;
                    if (!go.activeSelf) go.SetActive(true);
                }
            }

            // Deactivate panelsToClose
            if (panelsToClose != null)
            {
                foreach (var go in panelsToClose)
                {
                    if (go == null) continue;
                    if (go.activeSelf) go.SetActive(false);
                }
            }
        }
        else
        {
            // Deactivate panelsToOpen
            if (panelsToOpen != null)
            {
                foreach (var go in panelsToOpen)
                {
                    if (go == null) continue;
                    if (go.activeSelf) go.SetActive(false);
                }
            }

            // Activate panelsToClose
            if (panelsToClose != null)
            {
                foreach (var go in panelsToClose)
                {
                    if (go == null) continue;
                    if (!go.activeSelf) go.SetActive(true);
                }
            }
        }

        // Handle cursor visibility/lock if enabled
        if (unlockCursorOnOpen)
        {
            if (isOpen)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    // Called when any configured UI button is clicked
    private void OnUiButtonClicked()
    {
        // Hide and lock cursor to return control to gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Optionally close panels if your workflow requires it
        // SetOpenState(false);
    }

    // Public getters
    public bool IsOpen => isOpen;

    // Convenience methods
    public void OpenAll() => SetOpenState(true);
    public void CloseAll() => SetOpenState(false);
}
