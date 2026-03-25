using UnityEngine;

/// <summary>
/// Attach this script to any item prefab that you want the player to be able to pick up. It will handle the interaction logic and trigger the appropriate animation on the player when they interact with it.
/// </summary>
public class ItemLoot : Interactable
{
    [Header("Item Info")]
    public string itemName = "Đá";
    public int amount = 1;

    // Biến để lưu tạm cái Animator của Player khi nhặt đồ
    private Animator playerAnim;

    public override void OnInteract()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            playerAnim = player.GetComponentInChildren<Animator>();

            if (playerAnim != null)
            {
                Debug.Log("Player đang cúi xuống nhặt " + itemName);

                playerAnim.SetTrigger("Pickup");
            }
        }
    }

    public void ConfirmCollected()
    {
        Debug.Log("Đã nhặt thành công: " + amount + " " + itemName);
        // TODO later: Add items to invetory
        // Inventory.Instance.AddItem(itemName, amount);
        Destroy(gameObject);
    }
}