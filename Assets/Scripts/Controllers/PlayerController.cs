using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerAttack))]
public class PlayerController : MonoBehaviour
{
    public Interactable focus;
    public LayerMask movementMask;

    Camera cam;
    PlayerMotor motor;
    PlayerAttack attack;

    void Start()
    {
        cam = Camera.main;
        motor = GetComponent<PlayerMotor>();
        attack = GetComponent<PlayerAttack>();

        if (cam == null)
        {
            Debug.LogError("PlayerController: No camera tagged MainCamera found!");
        }
    }

    void Update()
    {
        // LEFT CLICK: move to ground
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, movementMask))
            {
                motor.MoveToPoint(hit.point);
                RemoveFocus();
            }
        }

// RIGHT CLICK: interact OR lock-on target
if (Input.GetMouseButtonDown(1))
{
    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 100f))
    {
        // 1) Try interactable first
        Interactable interactable = hit.collider.GetComponent<Interactable>();

        if (interactable != null)
        {
            SetFocus(interactable);
            attack.ClearTarget();   // stop targeting enemies when interacting
            return;
        }

        // 2) If no interactable, try locking onto an enemy (Health)
        Health targetHealth = hit.collider.GetComponentInParent<Health>();
        if (targetHealth != null)
        {
            RemoveFocus();                  // stop following interactables
            motor.StopFollowingTarget();    // stay where you are
            attack.SetTarget(targetHealth); // LOCK ON
            return;
        }

        // 3) Clicked on nothing interesting: clear both
        RemoveFocus();
        attack.ClearTarget();
    }
}



    void SetFocus(Interactable newFocus)
    {
        if (newFocus == focus) return;

        // defocus old
        if (focus != null)
        {
            focus.OnDefocused();
        }

        focus = newFocus;
        focus.OnFocused(transform);      // tell interactable who is focusing it
        motor.FollowTarget(newFocus);    // tell motor to follow it
        Debug.Log("SetFocus: " + focus.name);
    }

    void RemoveFocus()
    {
        if (focus != null)
        {
            focus.OnDefocused();
            focus = null;
        }
        motor.StopFollowingTarget();
    }
    }
}
