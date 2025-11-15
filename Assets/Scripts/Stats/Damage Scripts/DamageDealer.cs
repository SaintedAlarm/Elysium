using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageDealer : MonoBehaviour
{
    [Header("Damage")]
    public float baseDamage = 10f;
    public DamageType damageType = DamageType.Physical;

    [Tooltip("If true, take damage value from attached CharacterStats.")]
    public bool useStatsForDamage = true;

    [Tooltip("If true, this object is destroyed after dealing damage (e.g., projectile).")]
    public bool destroyOnHit = false;

    private CharacterStats attackerStats;

    void Awake()
    {
        // Try to find stats on this object or its parents
        attackerStats = GetComponentInParent<CharacterStats>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Get the target's health
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth == null) return;

        // Optional: target stats (for defense)
        CharacterStats targetStats = other.GetComponent<CharacterStats>();

        float rawDamage = baseDamage;

        // Add attacker stats if available
        if (useStatsForDamage && attackerStats != null)
        {
            rawDamage += attackerStats.GetAttackDamage(damageType);
        }

        float finalDamage = rawDamage;

        // Let the target’s stats reduce the damage
        if (targetStats != null)
        {
            finalDamage = targetStats.ApplyDefense(rawDamage);
        }

        targetHealth.TakeDamage(finalDamage);

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
