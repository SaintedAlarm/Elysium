using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waypointTolerance = 0.5f;

    [Header("Detection")]
    public float aggroRange = 10f;      // start chasing
    public float loseAggroRange = 15f;  // give up and go back to patrol

    [Header("Combat")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float baseDamage = 5f;
    public DamageType damageType = DamageType.Physical;

    private State state = State.Patrol;
    private NavMeshAgent agent;
    private Transform player;
    private Health playerHealth;
    private CharacterStats enemyStats;

    private int patrolIndex = 0;
    private float lastAttackTime = -999f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<CharacterStats>();
    }

    void Start()
    {
        // Start patrolling if we have points
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            patrolIndex = 0;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }

        // Find player by tag
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerHealth = player.GetComponent<Health>();
            }
            else
            {
                Debug.LogWarning("EnemyAI: No GameObject with tag 'Player' found.");
            }
        }
    }

    void Update()
    {
        // If no player or dead player, just patrol
        if (player == null || playerHealth == null || playerHealth.IsDead)
        {
            PatrolUpdate();
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
                PatrolUpdate();
                if (distToPlayer <= aggroRange)
                {
                    state = State.Chase;
                }
                break;

            case State.Chase:
                ChaseUpdate(distToPlayer);
                break;

            case State.Attack:
                AttackUpdate(distToPlayer);
                break;
        }
    }

    void PatrolUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            agent.isStopped = false;
            return;
        }

        agent.isStopped = false;

        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void ChaseUpdate(float distToPlayer)
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Lost the player
        if (distToPlayer > loseAggroRange)
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
            state = State.Patrol;
            return;
        }

        // In attack range
        if (distToPlayer <= attackRange)
        {
            state = State.Attack;
            agent.isStopped = true;
        }
    }

    void AttackUpdate(float distToPlayer)
    {
        // Face the player
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // Player stepped out of melee range: go back to chase
        if (distToPlayer > attackRange + 0.5f)
        {
            state = State.Chase;
            agent.isStopped = false;
            return;
        }

        // Attack on cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            DoAttack();
            lastAttackTime = Time.time;
        }
    }

    void DoAttack()
    {
        if (playerHealth == null || playerHealth.IsDead) return;

        float rawDamage = baseDamage;

        if (enemyStats != null)
        {
            rawDamage += enemyStats.GetAttackDamage(damageType);
        }

        float finalDamage = rawDamage;

        CharacterStats playerStats = player.GetComponent<CharacterStats>();
        if (playerStats != null)
        {
            finalDamage = playerStats.ApplyDefense(rawDamage);
        }

        playerHealth.TakeDamage(finalDamage);
        // Debug.Log($"Enemy hit player for {finalDamage}");
    }
}
