using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 2f;
    public DamageType damageType = DamageType.Physical;
    public float extraBaseDamage = 0f; // flat bonus on top of stats

    [Header("Timing")]
    public float attackCooldown = 0.5f;
    private float lastAttackTime;

    private CharacterStats stats;
    private Transform cachedTransform;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
        cachedTransform = transform;
    }

    public void AttackTarget(Health target)
    {
        if (target == null) return;
        if (target.IsDead) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        float dist = Vector3.Distance(cachedTransform.position, target.transform.position);
        if (dist > attackRange)
        {
            Debug.Log("Target too far to hit.");
            return;
        }

        lastAttackTime = Time.time;

        float rawDamage = extraBaseDamage;

        if (stats != null)
        {
            rawDamage += stats.GetAttackDamage(damageType);
        }
        else
        {
            rawDamage += 5f; // fallback
        }

        CharacterStats targetStats = target.GetComponent<CharacterStats>();
        float finalDamage = rawDamage;

        if (targetStats != null)
        {
            finalDamage = targetStats.ApplyDefense(rawDamage);
        }

        target.TakeDamage(finalDamage);
        Debug.Log($"Hit {target.name} for {finalDamage} damage.");
    }
}
