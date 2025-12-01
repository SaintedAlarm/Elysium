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

// RIGHT CLICK: interact ONLY
if (Input.GetMouseButtonDown(1))
{
    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 100f))
    {
        Interactable interactable = hit.collider.GetComponent<Interactable>();

        if (interactable != null)
        {
            SetFocus(interactable);
        }
        else
        {
            // clicked on nothing interactable: clear focus
            RemoveFocus();
        }
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
