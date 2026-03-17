using UnityEngine;

/// <summary>
/// Base class cho các object tương tác.
/// Implement IInteractable với default behavior.
/// Có thể kế thừa class này hoặc implement IInteractable trực tiếp.
/// </summary>
public class Interactable : MonoBehaviour, IInteractable
{
    public virtual bool IsInteracting => false;

    public virtual void OnInteract()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }

    public virtual void OnInteractContinue() { }

    public virtual void OnInteractEnd() { }
}
