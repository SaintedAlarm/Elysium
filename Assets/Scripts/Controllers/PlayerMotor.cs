using UnityEngine;
using UnityEngine.AI;

public class PlayerMotor : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform target;  // thing to follow

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // If we have a target, keep updating our destination
        if (target != null)
        {
            agent.SetDestination(target.position);
            FaceTarget();
        }
    }

    // Called by PlayerController when left-clicking on ground
    public void MoveToPoint(Vector3 point)
    {
        target = null; // stop following anything
        agent.stoppingDistance = 0f;
        agent.updateRotation = true;
        agent.SetDestination(point);
    }

    // Called by PlayerController when right-clicking an interactable
    public void FollowTarget(Interactable newTarget)
    {
        target = newTarget.transform;
        agent.stoppingDistance = newTarget.radius * 0.8f; // stop just inside radius
        agent.updateRotation = false;
    }

    // Called by PlayerController when focus is removed
    public void StopFollowingTarget()
    {
        target = null;
        agent.stoppingDistance = 0f;
        agent.updateRotation = true;
    }

    private void FaceTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
