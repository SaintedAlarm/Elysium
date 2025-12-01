using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public FloatData startingHealthData;  // <-- ADDED

    [SerializeField] private float currentHealth;
    public bool IsDead { get; private set; }

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged01; // 0–1 for UI sliders

    [Header("Death Behavior")]
    public bool destroyOnDeath = true;
    public bool disableOnDeath = false;

    void Awake()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        IsDead = false;

        // <-- ADDED THIS BLOCK
        if (startingHealthData != null)
        {
            maxHealth = startingHealthData.value;
        }

        currentHealth = maxHealth;
        onHealthChanged01?.Invoke(currentHealth / maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        onHealthChanged01?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0f && !IsDead)
        {
            IsDead = true;
            onDeath?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else if (disableOnDeath)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        onHealthChanged01?.Invoke(currentHealth / maxHealth);
    }
}
