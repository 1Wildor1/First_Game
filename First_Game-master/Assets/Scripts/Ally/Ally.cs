using UnityEngine;

public class Ally : MonoBehaviour
{
    public int maxHealth = 100;
    protected int currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int value)
    {
        currentHealth -= value;

        Debug.Log(name + " HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    protected virtual void Die()
    {
        Debug.Log(name + " погиб");
        Destroy(gameObject);
    }
}
