using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    [Header("Combat Hitboxes")]
    public PlayerHitbox rightHandHitbox;
    public PlayerHitbox leftHandHitbox;
    public PlayerHitbox leftFootHitbox;

    [Header("Base Stats")]
    public int baseDamage = 25;


    public void Event_EnableRightHand(float motionValue)
    {
        if (rightHandHitbox == null) return;

        int finalDamage = Mathf.RoundToInt(baseDamage * motionValue);
        rightHandHitbox.EnableHitbox(finalDamage);
    }
    public void Event_DisableRightHand()
    {
        if (rightHandHitbox != null) rightHandHitbox.DisableHitbox();
    }

    public void Event_EnableLeftHand(float motionValue)
    {
        if (leftHandHitbox == null) return;
        int finalDamage = Mathf.RoundToInt(baseDamage * motionValue);
        leftHandHitbox.EnableHitbox(finalDamage);
    }
    public void Event_DisableLeftHand()
    {
        if (leftHandHitbox != null) leftHandHitbox.DisableHitbox();
    }

    public void Event_EnableLeftFoot(float motionValue)
    {
        if (leftFootHitbox == null) return;
        int finalDamage = Mathf.RoundToInt(baseDamage * motionValue);
        leftFootHitbox.EnableHitbox(finalDamage);
    }
    public void Event_DisableLeftFoot()
    {
        if (leftFootHitbox != null) leftFootHitbox.DisableHitbox();
    }
}