using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // Internal state
    private bool isOpen = false;

    private void Start()
    {
        // Set initial state according to startOpen
        SetOpenState(startOpen);
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
    }

    // Public getters
    public bool IsOpen => isOpen;

    // Convenience methods
    public void OpenAll() => SetOpenState(true);
    public void CloseAll() => SetOpenState(false);
}
