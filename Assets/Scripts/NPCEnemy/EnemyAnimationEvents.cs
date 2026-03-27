using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    public EnemyHitbox rightHandHitbox;
    public void Event_EnableEnemyHitbox()
    {
        if (rightHandHitbox != null) rightHandHitbox.EnableHitbox();
    }

    public void Event_DisableEnemyHitbox()
    {
        if (rightHandHitbox != null) rightHandHitbox.DisableHitbox();
    }
}