using UnityEngine;
using UnityEngine.AI;

public class PlayerMotor : MonoBehaviour
{
    NavMeshAgent agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent < NavMeshAgent>();
    }

    // Update is called once per frame
    public void MoveToPoint (Vector3 point)
    {
        agent.SetDestination(point);
    }
}
