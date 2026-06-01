using System.Collections;
using UnityEngine;

public class Archer : MonoBehaviour
{
    public GameObject normalArrowPrefab;
    public GameObject fireArrowPrefab;
    public GameObject iceArrowPrefab;
    public GameObject poisonArrowPrefab;
    public Transform firePoint;
    public float fireDelay = 0.2f; // скорость серии
    public GridManager grid;
    public int baseDamage = 20;
    public float damageMultiplier = 1f;
    private int arrowsToShoot = 0;
    private bool isShooting = false;
    public float attackRange = 10f;
    public GameObject bombPrefab;
    private Animator anim;
    public AudioClip shootSound;

    private AudioSource audioSource;

    private void Start()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    public void AddArrows(int amount)
    {
        arrowsToShoot += amount;

        if (!isShooting)
        {
            StartCoroutine(ShootCoroutine());
        }
    }

    public void TryStartShooting()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootCoroutine());
        }
    }
    IEnumerator ShootCoroutine()
    {
        isShooting = true;

        while (true) // 🔥 бесконечный цикл
        {
            if (grid == null)
                yield break;
            
            if (anim != null && anim.GetCurrentAnimatorStateInfo(0).IsName("TrowBomb"))
            {
                yield return null;
                continue; // ❗ ВАЖНО
            }
            if (!grid.attackEnabled || grid.arrows <= 0)
            {
                if (anim != null)
                    anim.SetBool("IsShooting", false);

                yield return new WaitForSeconds(0.1f);
                continue;
            }

            if (FindClosestMonster() != null)
            {
                if (anim != null)
                    anim.SetBool("IsShooting", true);

                Shoot();
            }
            else
            {
                if (anim != null)
                    anim.SetBool("IsShooting", false);
            }

            yield return new WaitForSeconds(fireDelay);
        }
    }
   
    public void ThrowBomb()
    {
        Debug.Log("THROW BOMB BUTTON CLICKED");
        if (grid.bombs <= 0) return;

        Transform target = FindClosestMonster();
        if (target == null) return;

        grid.bombs--;
        grid.UpdateUI();
        // запускаем анимацию
        GetComponent<Animator>()?.SetTrigger("Throw");
    }

    public void SpawnBomb()
    {
        Debug.Log("SPAWN BOMB 💣"); 
        Transform target = FindClosestMonster();
        if (target == null) return;

        GameObject bombObj = Instantiate(bombPrefab, firePoint.position, Quaternion.identity);

        Bomb bomb = bombObj.GetComponent<Bomb>();
        bomb.Init(target.position);
    }
    public void PlayShootSound()
    {
        audioSource.PlayOneShot(shootSound);
    }
    bool Shoot()
    {
        if (grid == null)
            return false;

        if (grid.arrows <= 0)
            return false;

        Transform target = FindClosestMonster();

        if (target == null)
            return false;

        GameObject selectedArrow = normalArrowPrefab;

        bool useFireEffect = false;
        bool useIceEffect = false;
        bool usePoisonEffect = false;

        switch (grid.currentArrow)
        {
            case ArrowType.Normal:

                selectedArrow = normalArrowPrefab;

                break;

            case ArrowType.Fire:
                grid.fire--;

                if (grid.fire <= 0)
                {
                    grid.currentArrow = ArrowType.Normal;
                    grid.UpdateUI();

                    selectedArrow = normalArrowPrefab;
                    break;
                }

                selectedArrow = fireArrowPrefab;
                useFireEffect = true;

                

                break;

            case ArrowType.Ice:
                grid.ice--;
                if (grid.ice <= 0)
                {
                    grid.currentArrow = ArrowType.Normal;
                    grid.UpdateUI();

                    selectedArrow = normalArrowPrefab;
                    break;
                }

                selectedArrow = iceArrowPrefab;
                useIceEffect = true;

               

                break;

            case ArrowType.Poison:
                grid.poison--;

                if (grid.poison <= 0)
                {
                    grid.currentArrow = ArrowType.Normal;
                    grid.UpdateUI();

                    selectedArrow = normalArrowPrefab;
                    break;
                }

                selectedArrow = poisonArrowPrefab;
                usePoisonEffect = true;


                break;
        }

        // списываем обычную стрелу
        grid.arrows--;

        // обновляем UI
        grid.UpdateUI();

        //// звук только если выстрел действительно происходит
        //if (audioSource != null && shootSound != null)
        //{
        //    audioSource.PlayOneShot(shootSound);
        //}

        // создаём стрелу
        GameObject arrowObj = Instantiate(
            selectedArrow,
            firePoint.position,
            Quaternion.identity
        );

        Arrow arrow = arrowObj.GetComponent<Arrow>();

        if (arrow == null)
            return false;

        int finalDamage =
            Mathf.RoundToInt(baseDamage * damageMultiplier);

        arrow.SetDamage(finalDamage);

        arrow.Init(target);

        arrow.SetModifiers(
            useFireEffect,
            useIceEffect,
            usePoisonEffect
        );

        return true;
    }
    Transform FindClosestMonster()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject m in monsters)
        {
            float dist = Vector2.Distance(transform.position, m.transform.position);

            // ❗ проверка дальности
            if (dist > attackRange) continue;

            if (dist < minDist)
            {
                minDist = dist;
                closest = m.transform;
            }
        }

        return closest;
    }
}