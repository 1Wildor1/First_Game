using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float speed = 5f;
    public float explosionRadius = 2f;
    public int damage = 50;

    public AudioClip explosionSound;

    private AudioSource audioSource;
    private Rigidbody2D rb;

    public float rotationSpeed = 360f;

    private Vector2 target;

    private bool exploded = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 targetPos)
    {
        target = targetPos;
    }

    void Update()
    {
        if (exploded) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        transform.Rotate(
            0,
            0,
            rotationSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            Explode();
        }
    }

    void Explode()
    {
        if (exploded) return;

        exploded = true;

        audioSource.PlayOneShot(explosionSound);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Monster"))
            {
                hit.GetComponent<Monster>()?.TakeDamage(damage);
            }
        }

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        rb.simulated = false;

        Destroy(gameObject, 1f);
    }
}