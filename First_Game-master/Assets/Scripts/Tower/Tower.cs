using UnityEngine;

public class Tower : MonoBehaviour
{
    public int maxHealth = 500;
    private int currentHealth;

    [Header("Tower HP Bar")]
    public GameObject healthBarPrefab;   // большой HP bar prefab
    public Transform hpBarPosition;      // точка, где будет располагаться HP bar

    private HealthBar healthBar;

    void Start()
    {
        currentHealth = maxHealth;

        // создаём HP bar в указанной точке
        if (healthBarPrefab != null && hpBarPosition != null)
        {
            GameObject bar = Instantiate(
                healthBarPrefab,
                hpBarPosition.position,
                Quaternion.identity
            );

            // если нужно, чтобы bar был дочерним объектом
            bar.transform.SetParent(hpBarPosition);

            healthBar = bar.GetComponent<HealthBar>();
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log("Tower HP: " + currentHealth);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseGame();
        }

        Destroy(gameObject);
    }
}