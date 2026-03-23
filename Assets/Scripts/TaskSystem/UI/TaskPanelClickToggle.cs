using UnityEngine;

public class TaskPanelClickToggle : MonoBehaviour
{
    [SerializeField] private GameObject panelToToggle;
    [SerializeField] private bool hideOnStart = true;

    private bool useCanvasGroupVisibility;
    private CanvasGroup panelCanvasGroup;

    private void Awake()
    {
        if (panelToToggle == null)
        {
            panelToToggle = gameObject;
        }

        useCanvasGroupVisibility = panelToToggle == gameObject;
        if (useCanvasGroupVisibility)
        {
            panelCanvasGroup = panelToToggle.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = panelToToggle.AddComponent<CanvasGroup>();
            }
        }

    }

    private void Start()
    {
        if (hideOnStart)
        {
            SetPanelVisible(false);
        }
    }

    private void OnEnable()
    {
        TaskEvents.OnTaskPanelRevealRequested += HandlePanelRevealRequested;
    }

    private void OnDisable()
    {
        TaskEvents.OnTaskPanelRevealRequested -= HandlePanelRevealRequested;
    }

    private void HandlePanelRevealRequested()
    {
        ShowPanel();
    }

    public void TogglePanel()
    {
        bool nextState = !IsPanelVisible();
        SetPanelVisible(nextState);
        Debug.Log($"[TaskPanelClickToggle] Toggle panel => {(nextState ? "Open" : "Close")}");
    }

    public void ShowPanel()
    {
        if (panelToToggle != null && !IsPanelVisible())
        {
            SetPanelVisible(true);
            Debug.Log("[TaskPanelClickToggle] Auto show panel from player interaction.");
        }
    }

    public void HidePanel()
    {
        if (panelToToggle != null && IsPanelVisible())
        {
            SetPanelVisible(false);
            Debug.Log("[TaskPanelClickToggle] Auto hide panel after display duration.");
        }
    }

    private bool IsPanelVisible()
    {
        if (panelToToggle == null)
        {
            return false;
        }

        if (useCanvasGroupVisibility && panelCanvasGroup != null)
        {
            return panelCanvasGroup.alpha > 0.001f;
        }

        return panelToToggle.activeSelf;
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelToToggle == null)
        {
            return;
        }

        if (useCanvasGroupVisibility && panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = visible ? 1f : 0f;
            panelCanvasGroup.interactable = visible;
            panelCanvasGroup.blocksRaycasts = visible;
            return;
        }

        panelToToggle.SetActive(visible);
    }
}
