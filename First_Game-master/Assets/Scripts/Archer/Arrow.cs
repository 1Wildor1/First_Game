using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 20;
    public float critChance = 0.2f;
    public float critMultiplier = 2f;

    private bool fire;
    private bool ice;
    private bool poison;

    public float startDelay = 0.1f;

    private bool canMove = false;
    private bool hasHit = false;

    private Vector2 direction;

    public void Init(Transform target)
    {
        if (target != null)
        {
            Transform aim = target.GetComponent<Monster>()?.aimPoint;

            Vector2 targetPos =
                aim != null
                ? (Vector2)aim.position
                : (Vector2)target.position;

            targetPos += Random.insideUnitCircle * 0.1f;

            direction = (targetPos - (Vector2)transform.position).normalized;

            // 🔥 Поворот стрелы сразу
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            direction = Vector2.right;
        }

        StartCoroutine(StartFlyDelay());
    }

    IEnumerator StartFlyDelay()
    {
        yield return new WaitForSeconds(startDelay);
        canMove = true;
    }

    void Update()
    {
        if (!canMove) return;

        // 🔥 движение вперёд по сохранённому direction
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    public void SetModifiers(bool fire, bool ice, bool poison)
    {
        this.fire = fire;
        this.ice = ice;
        this.poison = poison;

        int finalDamage = damage;

        if (fire) finalDamage += 10;
        if (ice) finalDamage += 5;
        if (poison) finalDamage += 3;

        damage = finalDamage;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (collision.CompareTag("Monster"))
        {
            hasHit = true;

            Monster monster = collision.GetComponent<Monster>();

            if (monster != null)
            {
                int finalDamage = damage;

                // крит
                if (Random.value < critChance)
                {
                    finalDamage =
                        Mathf.RoundToInt(damage * critMultiplier);
                }

                monster.TakeDamage(finalDamage);

                if (fire) monster.ApplyFire();
                if (ice) monster.ApplySlow();
                if (poison) monster.ApplyPoison();
            }

            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    public void SetDamage(int value)
    {
        damage = value;
    }
}