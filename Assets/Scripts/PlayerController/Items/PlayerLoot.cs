using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerLoot : MonoBehaviour
{
    [Header("Loot Radius Settings")]
    public float lootRange = 3f;
    public LayerMask lootLayerMask;

    [Header("UI Prompt")]
    public TextMeshProUGUI promptText;
    public string promptMessage = "Press F to Loot";

    public InputActionAsset InputActions;
    private InputAction m_lootAction;
    private ItemLoot m_currentItem;

    private bool isLooting = false;

    private void Awake()
    {
        m_lootAction = InputActions.FindActionMap("Player").FindAction("Pickup");
    }

    private void OnEnable() => m_lootAction.Enable();
    private void OnDisable() => m_lootAction.Disable();

    private void Update()
    {
        if (isLooting) return;

        ScanForLoot();

        if (m_currentItem != null && m_lootAction.WasPressedThisFrame())
        {
            isLooting = true;
            m_currentItem.OnInteract();

            if (promptText != null) promptText.text = string.Empty;
        }
    }

    private void ScanForLoot()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, lootRange, lootLayerMask);

        ItemLoot closestItem = null;
        float minDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            ItemLoot item = hit.GetComponentInParent<ItemLoot>();
            if (item != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestItem = item;
                }
            }
        }

        m_currentItem = closestItem;

        if (m_currentItem != null)
        {
            if (promptText != null) promptText.text = promptMessage;
        }
        else
        {
            if (promptText != null && promptText.text == promptMessage) promptText.text = string.Empty;
        }
    }
    //Event animation
    public void ExecuteLoot()
    {
        if (m_currentItem != null)
        {
            m_currentItem.ConfirmCollected();
            m_currentItem = null;
        }
        isLooting = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lootRange);
    }
}