using UnityEngine;

public class Interactable : MonoBehaviour
{
    public float radius = 3f;

    void OGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
