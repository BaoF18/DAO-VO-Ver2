using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerLoot m_playerLoot;

    private void Awake()
    {
        m_playerLoot = GetComponentInParent<PlayerLoot>();
    }

    public void FinishPickupEvent()
    {
        if (m_playerLoot != null)
        {
            m_playerLoot.ExecuteLoot();
        }
    }
}