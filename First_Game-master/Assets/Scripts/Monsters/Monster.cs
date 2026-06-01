using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    public float moveForce = 3f;
    public float jumpForce = 6f;
    public float jumpDelay = 1.5f;

    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine flashCoroutine;
    public bool isMenu = false;
    private float nextAttackTime;

    private Animator animator;
    private Rigidbody2D rb;
    private bool isGrounded;

    private float nextJumpTime;
    public Transform aimPoint;
    public Transform spriteTransform;
    public GameObject healthBarPrefab;
    private HealthBar healthBar;

    private Transform currentTarget; // ← текущая цель (союзник или башня)

    void Start()
    {
        if (isMenu)
        {
            //transform.localScale = new Vector3(185, 185, 1);
            spriteTransform.localScale = new Vector3(1, 1, 1);

        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);

        }
        //transform.localScale = new Vector3(100, 100, 1);

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        GameObject tower = GameObject.FindGameObjectWithTag("Tower");
        if (tower != null)
            currentTarget = tower.transform;

        GameObject bar = Instantiate(healthBarPrefab, transform);
        bar.transform.localPosition = new Vector3(0, 0.5f, 0);

        healthBar = bar.GetComponent<HealthBar>();
        healthBar.SetHealth(currentHealth, maxHealth);
    }

    void Update()
    {
        FindTarget(); // ← ВСЕГДА ищем ближайшую цель

        if (currentTarget == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            currentTarget.position
        );

        // если рядом → атакуем
        if (distance <= attackRange && isGrounded)
        {
            TryAttack();
            return;
        }

        // иначе → идём вперёд
        HandleJumpMovement();
    }

    void FindTarget()
    {
        // ищем ближайшего союзника (копейщик/лучник)
        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");

        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (GameObject ally in allies)
        {
            float dist = Vector2.Distance(transform.position, ally.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestTarget = ally.transform;
            }
        }

        // если союзник найден → бьём его
        if (closestTarget != null)
        {
            currentTarget = closestTarget;
            return;
        }

        // иначе → бьём башню
        GameObject tower = GameObject.FindGameObjectWithTag("Tower");
        if (tower != null)
            currentTarget = tower.transform;
    }

    public void ApplyFire()
    {
        TakeDamage(5);
    }

    public void ApplySlow()
    {
        // временно пусто
    }

    public void ApplyPoison()
    {
        TakeDamage(2);
    }
    void TryAttack()
    {
        if (Time.time < nextAttackTime)
            return;

        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Attack");

        nextAttackTime = Time.time + attackCooldown;
    }

    // вызывается через Animation Event
    public void DealDamage()
    {
        if (currentTarget == null)
            return;

        Ally ally = currentTarget.GetComponent<Ally>();

        if (ally != null)
        {
            Debug.Log("Монстр ударил союзника!");

            ally.TakeDamage(10);

            if (ally.IsDead())
                currentTarget = null;

            return;
        }

        Tower tower = currentTarget.GetComponent<Tower>();

        if (tower != null)
        {
            tower.TakeDamage(10);
        }
    }
    void HandleJumpMovement()
    {
        if (currentTarget == null)
            return;

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        if (distance <= attackRange)
            return;

        if (isGrounded && Time.time >= nextJumpTime)
        {
            Vector2 direction =
                (currentTarget.position - transform.position).normalized;

            rb.linearVelocity =
                new Vector2(direction.x * moveForce, jumpForce);

            nextJumpTime = Time.time + jumpDelay;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        healthBar.SetHealth(currentHealth, maxHealth);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(WhiteFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    IEnumerator WhiteFlash()
    {
        sr.color = Color.red;

        yield return new WaitForSeconds(0.12f);

        sr.color = originalColor;
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.MonsterKilled();
        }

        if (animator != null)
            animator.SetTrigger("Die");

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}