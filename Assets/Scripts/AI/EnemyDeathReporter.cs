using UnityEngine;

public class EnemyDeathReporter : MonoBehaviour
{
    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.AddListener(OnEnemyDeath);
        }
    }

    void OnEnemyDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemyKilled();
        }
    }
}
