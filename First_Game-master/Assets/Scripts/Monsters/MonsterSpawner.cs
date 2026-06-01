using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public float spawnRate = 2f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnMonster), 1f, spawnRate);
    }

    void SpawnMonster()
    {
        Instantiate(monsterPrefab, transform.position, Quaternion.identity);
    }
}