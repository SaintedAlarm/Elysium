using UnityEngine;

public class Interactable : MonoBehaviour
{
    public float radius = 3f;        // how close player must be to interact

    bool isFocus = false;
    bool hasInteracted = false;
    Transform player;

    // Called when something focuses this interactable
    public void OnFocused(Transform playerTransform)
    {
        isFocus = true;
        player = playerTransform;
        hasInteracted = false;
    }

    // Called when focus is lost
    public void OnDefocused()
    {
        isFocus = false;
        player = null;
        hasInteracted = false;
    }

    // This is what happens when the player is close enough
    public virtual void Interact()
    {
        Debug.Log("Interacting with " + transform.name);
        // Override in child classes to pick up item, open chest, etc.
    }

    void Update()
    {
        if (isFocus && !hasInteracted && player != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= radius)
            {
                Interact();
                hasInteracted = true;   // only once per focus
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
