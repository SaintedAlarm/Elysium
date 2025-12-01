using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private enum AttackMode { Melee, Magic }

    [Header("Targeting")]
    public Health currentTarget;
    public float maxLockDistance = 20f;

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
        // Clear dead / too-far target
        if (currentTarget != null)
        {
            if (currentTarget.IsDead)
            {
                ClearTarget();
            }
            else
            {
                float dist = Vector3.Distance(cachedTransform.position, currentTarget.transform.position);
                if (dist > maxLockDistance)
                {
                    ClearTarget();
                }
            }
        }

        // Face the target if we have one
        if (currentTarget != null)
        {
            Vector3 dir = currentTarget.transform.position - cachedTransform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                cachedTransform.rotation = Quaternion.Slerp(
                    cachedTransform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }

        // Q = melee
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryAttack(AttackMode.Melee);
        }

        // E = magic
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryAttack(AttackMode.Magic);
        }
    }

    public void SetTarget(Health target)
    {
        currentTarget = target;
        if (currentTarget != null)
        {
            Debug.Log("Locked onto: " + currentTarget.name);
        }
    }

    public void ClearTarget()
    {
        if (currentTarget != null)
        {
            Debug.Log("Cleared target: " + currentTarget.name);
        }
        currentTarget = null;
    }

    void TryAttack(AttackMode mode)
    {
        if (currentTarget == null)
        {
            Debug.Log("No target locked.");
            return;
        }

        float range = (mode == AttackMode.Melee) ? meleeRange : magicRange;
        DamageType type = (mode == AttackMode.Melee) ? meleeDamageType : magicDamageType;
        float extraDamage = (mode == AttackMode.Melee) ? meleeExtraBaseDamage : magicExtraBaseDamage;
        float cooldown = (mode == AttackMode.Melee) ? meleeCooldown : magicCooldown;
        ref float lastTime = ref (mode == AttackMode.Melee ? ref lastMeleeTime : ref lastMagicTime);

        DoAttack(mode.ToString().ToUpper(), currentTarget, range, type, extraDamage, cooldown, ref lastTime);
    }

    void DoAttack(string label,
                  Health target,
                  float range,
                  DamageType type,
                  float extraBaseDamage,
                  float cooldown,
                  ref float lastTime)
    {
        if (target == null || target.IsDead)
        {
            ClearTarget();
            return;
        }

        if (Time.time < lastTime + cooldown)
        {
            return;
        }

        float dist = Vector3.Distance(cachedTransform.position, target.transform.position);
        if (dist > range)
        {
            // optional debug
            // Debug.Log($"{label}: target too far ({dist:F1} > {range})");
            return;
        }

        lastTime = Time.time;

        float rawDamage = extraBaseDamage;
        if (stats != null)
        {
            rawDamage += stats.GetAttackDamage(type);
        }
        else
        {
            rawDamage += 5f;
        }

        CharacterStats targetStats = target.GetComponent<CharacterStats>();
        float finalDamage = targetStats != null ? targetStats.ApplyDefense(rawDamage) : rawDamage;

        target.TakeDamage(finalDamage);
        Debug.Log($"{label} hit {target.name} for {finalDamage} damage (type {type}).");
    }

    // keep for backwards compatibility with any old calls
    public void AttackTarget(Health target)
    {
        SetTarget(target);
        TryAttack(AttackMode.Melee);
    }
}
