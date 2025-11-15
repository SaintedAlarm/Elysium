using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Core")]
    public int level = 1;

    [Tooltip("Physical power, used for melee/weapon damage.")]
    public float strength = 10f;

    [Tooltip("Magic power, used for spells.")]
    public float magicPower = 10f;

    [Tooltip("Flat damage reduction. Each point reduces incoming damage by 1.")]
    public float defense = 2f;

    [Header("Resources")]
    public float maxStamina = 100f;
    public float maxMagicka = 100f;

    [SerializeField] private float currentStamina;
    [SerializeField] private float currentMagicka;

    public float CurrentStamina => currentStamina;
    public float CurrentMagicka => currentMagicka;

    void Awake()
    {
        currentStamina = maxStamina;
        currentMagicka = maxMagicka;
    }

    // Basic damage calculation: attacker calls this to figure out how hard they hit
    public float GetAttackDamage(DamageType type)
    {
        switch (type)
        {
            case DamageType.Magic:
                return magicPower;
            case DamageType.Physical:
            default:
                return strength;
        }
    }

    // Target calls this to reduce incoming damage with defense
    public float ApplyDefense(float incomingDamage)
    {
        // Simple flat reduction
        float reduced = incomingDamage - defense;

        // Never go below 1, so hits always matter
        if (reduced < 1f)
            reduced = 1f;

        return reduced;
    }

    public bool TryUseStamina(float amount)
    {
        if (currentStamina < amount) return false;
        currentStamina -= amount;
        return true;
    }

    public bool TryUseMagicka(float amount)
    {
        if (currentMagicka < amount) return false;
        currentMagicka -= amount;
        return true;
    }

    public void RestoreStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + amount, 0f, maxStamina);
    }

    public void RestoreMagicka(float amount)
    {
        currentMagicka = Mathf.Clamp(currentMagicka + amount, 0f, maxMagicka);
    }
}
