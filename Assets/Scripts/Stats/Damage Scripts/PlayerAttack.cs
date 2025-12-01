using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Melee Attack (Q)")]
    public float meleeRange = 2f;
    public DamageType meleeDamageType = DamageType.Physical;
    public float meleeExtraBaseDamage = 0f;
    public float meleeCooldown = 0.5f;

    [Header("Magic Attack (E)")]
    public float magicRange = 8f;
    public DamageType magicDamageType = DamageType.Magic;
    public float magicExtraBaseDamage = 5f;
    public float magicCooldown = 1.0f;

    private float lastMeleeTime;
    private float lastMagicTime;

    private CharacterStats stats;
    private Transform cachedTransform;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
        cachedTransform = transform;
    }

    void Update()
    {
        // Q = melee attack
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryAttack(AttackMode.Melee);
        }

        // E = magic attack
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryAttack(AttackMode.Magic);
        }
    }

    private enum AttackMode { Melee, Magic }

    void TryAttack(AttackMode mode)
    {
        // Aim with center of the camera instead of mouse click
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f)); 
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return;

        Health target = hit.collider.GetComponent<Health>();
        if (target == null || target.IsDead)
            return;

        if (mode == AttackMode.Melee)
        {
            DoAttack(target, meleeRange, meleeDamageType,
                     meleeExtraBaseDamage, meleeCooldown, ref lastMeleeTime);
        }
        else
        {
            DoAttack(target, magicRange, magicDamageType,
                     magicExtraBaseDamage, magicCooldown, ref lastMagicTime);
        }
    }

    void DoAttack(Health target, float range, DamageType type, float baseBonus, float cooldown, ref float lastTime)
    {
        if (Time.time < lastTime + cooldown)
            return;

        float dist = Vector3.Distance(cachedTransform.position, target.transform.position);
        if (dist > range)
            return;

        lastTime = Time.time;

        float rawDamage = baseBonus + (stats != null ? stats.GetAttackDamage(type) : 5f);

        CharacterStats targetStats = target.GetComponent<CharacterStats>();
        float finalDamage = targetStats != null ? targetStats.ApplyDefense(rawDamage) : rawDamage;

        target.TakeDamage(finalDamage);
        // Debug.Log($"[{type}] dealt {finalDamage} to {target.name}");
    }
}
