using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class GruntEnemyAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float aggroRange = 10f;     // how close the player needs to be
    public float attackRange = 2f;

    [Header("Damage To Player")]
    public float damageToPlayer = 5f;
    public float attackCooldown = 1.5f;

    [Header("Damage To Base")]
    public float damageToGoal = 20f;
    public bool destroyOnHitGoal = true;

    private NavMeshAgent agent;
    private Transform goalTarget;
    private Transform player;
    private Health playerHealth;

    private float lastAttackTime = -999f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.5f;

        // Find Base
        GameObject goalObj = GameObject.FindGameObjectWithTag("Goal");
        if (goalObj != null)
        {
            goalTarget = goalObj.transform;
        }
        else
        {
            Debug.LogWarning("GruntEnemyAI: No object with tag 'Goal' found in scene.");
        }

        // Find Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = player.GetComponent<Health>();
        }
        else
        {
            Debug.LogWarning("GruntEnemyAI: No object with tag 'Player' found in scene.");
        }

        // Start by going to base
        if (goalTarget != null)
        {
            agent.SetDestination(goalTarget.position);
        }
    }

    void Update()
    {
        if (!agent.enabled) return;

        bool hasPlayer = (player != null && playerHealth != null);

        if (hasPlayer)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);

            // If player is close enough, chase them
            if (distToPlayer <= aggroRange)
            {
                HandleChaseAndAttackPlayer(distToPlayer);
                return;
            }
        }

        // Default behaviour: go to base
        if (goalTarget != null)
        {
            agent.isStopped = false;
            agent.SetDestination(goalTarget.position);
        }
    }

    void HandleChaseAndAttackPlayer(float distToPlayer)
    {
        // Chase until within attack range
        if (distToPlayer > attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            // Stop and hit player
            agent.isStopped = true;

            // Face player
            Vector3 dir = player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                playerHealth.TakeDamage(damageToPlayer);
                lastAttackTime = Time.time;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hitting the Base still damages it
        if (other.CompareTag("Goal"))
        {
            Health goalHealth = other.GetComponent<Health>();
            if (goalHealth != null)
            {
                goalHealth.TakeDamage(damageToGoal);
            }

            if (destroyOnHitGoal)
            {
                Health myHealth = GetComponent<Health>();
                if (myHealth != null)
                {
                    myHealth.TakeDamage(9999);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
