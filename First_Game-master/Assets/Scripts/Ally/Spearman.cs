using UnityEngine;

public class Spearman : Ally
{
    public int damage = 20;
    public float attackRange = 1.5f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1.2f;

    private float nextAttackTime;
    private Transform target;

    private Animator anim;

    [Header("HP Bar")]
    public GameObject healthBarPrefab;
    private HealthBar healthBar;

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();

        // создаём HP bar
        GameObject bar = Instantiate(healthBarPrefab, transform);
        bar.transform.localPosition = new Vector3(0, 0.8f, 0);

        healthBar = bar.GetComponent<HealthBar>();
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        FindClosestMonster();

        if (target == null)
        {
            if (anim != null)
            {
                anim.SetBool("IsMoving", false);
                anim.SetBool("IsAttacking", false);
            }

            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            target.position
        );

        if (distance <= attackRange)
        {
            TryAttack();
        }
        else
        {
            if (anim != null)
                anim.SetBool("IsAttacking", false);

            MoveToTarget();
        }
    }

    void FindClosestMonster()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");

        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject monster in monsters)
        {
            float dist = Vector2.Distance(
                transform.position,
                monster.transform.position
            );

            if (dist < minDist)
            {
                minDist = dist;
                closest = monster.transform;
            }
        }

        target = closest;
    }

    void MoveToTarget()
    {
        if (anim != null)
        {
            anim.SetBool("IsMoving", true);
            anim.SetBool("IsAttacking", false);
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );
    }

    void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        if (anim != null)
        {
            anim.SetBool("IsMoving", false);
            anim.SetBool("IsAttacking", true);
        }
    }

    public void DealDamage()
    {
        if (target == null) return;

        Monster monster = target.GetComponent<Monster>();

        if (monster != null)
        {
            monster.TakeDamage(damage);
        }
    }

    public void EndAttack()
    {
        if (anim != null)
            anim.SetBool("IsAttacking", false);
    }

    public override void TakeDamage(int value)
    {
        base.TakeDamage(value);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth, maxHealth);
        }
    }
}